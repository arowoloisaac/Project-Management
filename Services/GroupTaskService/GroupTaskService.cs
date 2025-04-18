using Microsoft.EntityFrameworkCore;
using Task_Management_System.Data;
using Task_Management_System.DTOs.GroupIssueDto;
using Task_Management_System.DTOs.IssueDto;
using Task_Management_System.Enums;
using Task_Management_System.Models;
using Task_Management_System.Services.UserConfiguration;

namespace Task_Management_System.Services.GroupIssueService
{
    public class GroupTaskService: IGroupTaskService
    {
        private readonly ApplicationDbContext context;
        private readonly IUserConfig config;

        public GroupTaskService(ApplicationDbContext context, IUserConfig config)
        {
            this.context = context;
            this.config = config;
        }

        public async Task AssignIssue(Guid taskId, Guid assignTo, Guid projectId)
        {
            var retrieveProject = await ValidateProject(projectId);

            var retrieveTask = await ValidateIssue(taskId, projectId);

            var user = await config.GetUserById(assignTo.ToString());

            retrieveTask.AssignedTo = user;

            context.Issues.Update(retrieveTask);
            await context.SaveChangesAsync();
        }

        public async Task<string> CreateIssues(Guid projectId, CreateGroupIssueDto issueDto, string mail)
        {
            var user = await config.GetUser(mail);

            var checkProject = await ValidateProject(projectId);

            var validateIssueName = await context.Issues
                .Where(issue => issue.Name == issueDto.Name && issue.Project.Id == projectId).SingleOrDefaultAsync();

            if (validateIssueName == null)
            {
                if (issueDto.StartDate <= issueDto.EndDate)
                {
                    int date = CheckDate(issueDto.StartDate, issueDto.EndDate);

                    if (date >= issueDto.EstimatedTimeInMinutes)
                    {
                        var newIssue = await context.Issues.AddAsync(new Issue
                        {
                            Id = Guid.NewGuid(),
                            Name = issueDto.Name,
                            Description = issueDto.Description,
                            EstimatedTimeInMinutes = issueDto.EstimatedTimeInMinutes,
                            CreatedBy = user,
                            CreatedDate = DateTime.Now,
                            StartDate = issueDto.StartDate,
                            EndDate = issueDto.EndDate,
                            Complexity = issueDto.Complexity,
                            Progress = Progress.Todo,
                            IssueType = issueDto.IssueType,
                            Project = checkProject,
                            AssignedTo = issueDto.AssignedTo.HasValue ? await config.GetUserById(issueDto.AssignedTo.Value.ToString()) : null,
                            User = user,

                        });
                        await context.SaveChangesAsync();
                        return "Issue successfully created";
                    }
                    else
                    {
                        throw new Exception("Estimated time cannot be greated than the dates between");
                    }
                }
                else
                {
                    throw new Exception("start date must be less than end date or equal");
                }
            }
            else
            {
                throw new Exception("cannot create issue with same name");
            }
        }

        public async Task<string> CreateChildTask(Guid projectId, CreateGroupIssueDto issueDto, Guid parentIssueId, string mail)
        {
            var user = await config.GetUser(mail);

            var checkProject = await ValidateProject(projectId);

            var getParentIssue = await ValidateIssue(parentIssueId, checkProject.Id);

            var validateIssueName = await context.Issues
                .Where(issue => issue.Name == issueDto.Name && issue.Project.Id == projectId).SingleOrDefaultAsync();

            if (validateIssueName != null)
            {
                throw new Exception("task with name already exist in this project");
            }
            else
            {
                DateOnly start = issueDto.StartDate;
                DateOnly end = issueDto.EndDate;
                if (getParentIssue.StartDate >= start && getParentIssue.EndDate >= end)
                {
                    if (start <= end)
                    {
                        int date = CheckDate(issueDto.StartDate, issueDto.EndDate);

                        if (date >= issueDto.EstimatedTimeInMinutes)
                        {
                            var newIssue = await context.Issues.AddAsync(new Issue
                            {
                                Id = Guid.NewGuid(),
                                Name = issueDto.Name,
                                Description = issueDto.Description,
                                AssignedTo = issueDto.AssignedTo.HasValue ? await config.GetUserById(issueDto.AssignedTo.Value.ToString()) : null,
                                EstimatedTimeInMinutes = issueDto.EstimatedTimeInMinutes,
                                CreatedBy = user,
                                CreatedDate = DateTime.Now,
                                StartDate = issueDto.StartDate,
                                EndDate = issueDto.EndDate,
                                Complexity = issueDto.Complexity,
                                Progress = Progress.Todo,
                                IssueType = issueDto.IssueType,
                                Project = getParentIssue.Project,
                                ParentIssue = getParentIssue
                            });

                            //this is to add the time of the sub issue to the parent issue
                            getParentIssue.EstimatedTimeInMinutes += issueDto.EstimatedTimeInMinutes;
                            context.Issues.Update(getParentIssue);

                            await context.SaveChangesAsync();
                            return "Issue successfully created";
                        }
                        else
                        {
                            throw new Exception("Estimated time cannot be greated than the dates between");
                        }
                    }
                    else
                    {
                        throw new Exception("start date must be less than end date or equal");
                    }
                }
                else
                {
                    throw new Exception($"Make sure the date suite in line with the parent issue");
                }
            }
        }

        public async Task<List<RetrieveIssue>> GetIssue(Guid projectId)
        {
            var retrieveProject = await ValidateProject(projectId);

            var retrieveIssues = await context.Issues.Include(user => user.AssignedTo)
                .Where(iss => iss.Project == retrieveProject).ToListAsync();

            if (retrieveIssues == null)
            {
                return new List<RetrieveIssue>();
            }

            var response = retrieveIssues.Select(issue => new RetrieveIssue
            {
                id = issue.Id,
                Name = issue.Name,
                Complexity = issue.Complexity,
                Progress = issue.Progress,
                IssueType = issue.IssueType,
                IssueLevel = issue.IssueLevel,
                AssignedTo = issue.AssignedTo != null? issue.AssignedTo.FirstName : "",
            }).ToList();

            return response;
        }

        public async Task<string> DeleteIssues(Guid issueId, Guid projectId, bool isDeleteChildren)
        {
            try
            {
                var checkProject = await ValidateProject(projectId);

                var issue = await ValidateIssue(issueId, checkProject.Id);

                var issueChildren = await context.Issues
                        .Where(filter => filter.ParentIssue.Id == issueId)
                        .ToListAsync();

                if (isDeleteChildren)
                {
                    context.Issues.RemoveRange(issueChildren);
                }
                else
                {
                    foreach (var child in issueChildren)
                    {
                        child.ParentIssue = null;
                    }
                }

                context.Issues.Remove(issue);

                await context.SaveChangesAsync();

                return "deleted successfully";
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IssueAndChild>> GetIssueAndChild(Guid projectId)
        {
            var issues = await context.Issues.Include(user => user.AssignedTo)
               .Where(issue => issue.Project.Id == projectId)
               .ToListAsync();

            if (!issues.Any())
                return new List<IssueAndChild>();

            var response = issues.Select(issue => new IssueAndChild
            {
                Id = issue.Id,
                Name = issue.Name,
                Progress = issue.Progress,
                Complexity = issue.Complexity,
                IssueType = issue.IssueType,
                IssueLevel = issue.IssueLevel,
                EndDate = issue.EndDate,
                StartDate = issue.StartDate,
                AssignedTo = issue.AssignedTo == null ? null : issue.AssignedTo.FirstName,
                SubIssue = context.Issues
                    .Where(sub => sub.ParentIssue.Id == issue.Id)
                    .Select(sub => new IssueAndChild
                    {
                        Id = sub.Id,
                        Name = sub.Name,
                        IssueType = sub.IssueType,
                        AssignedTo = issue.AssignedTo == null ? null : issue.AssignedTo.FirstName,
                    })
                    .ToList(),
            }).ToList();

            return response;
        }

        public async Task<RetrieveGroupIssueDto> GetIssueById(Guid projectId, Guid issueId)
        {
            var exactIssue = await ValidateIssue(issueId, projectId);

            return new RetrieveGroupIssueDto
            {
                id = exactIssue.Id,
                Name = exactIssue.Name,
                Description = exactIssue.Description,
                Complexity = exactIssue.Complexity,
                IssueType = exactIssue.IssueType,
                Progress = exactIssue.Progress,
                IssueLevel = exactIssue.IssueLevel,
                EstimatedTimeInMinute = exactIssue.EstimatedTimeInMinutes,
                TimeSpent = exactIssue.TimeSpent,
                StartDate = exactIssue.StartDate,
                EndDate = exactIssue.EndDate,
                AssignedTo = exactIssue.AssignedTo != null ? exactIssue.AssignedTo.FirstName: "",
                AssignedToId = exactIssue.AssignedTo != null ? exactIssue.AssignedTo.Id : null,
            };
        }

        public Task<IEnumerable<RetrieveIssue>> GetIssues(IssueType? issueType, Complexity? complexity, Progress? progress, Guid projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<IssueResponse> GetIssuesPaginated(IssueType? issueType, Complexity? complexity, Progress? progress, int? page, int itemPerPage, Guid projectId)
        {
            IQueryable<Issue> query = context.Issues;

            int defaultItemsPerPage = 10;

            var items = itemPerPage == 0 ? defaultItemsPerPage : itemPerPage;

            if (issueType.HasValue)
            {
                query = query.Where(filter => filter.IssueType == issueType.Value);
            }
            if (complexity.HasValue)
            {
                query = query.Where(filter => filter.Complexity == complexity.Value);
            }
            if (progress.HasValue)
            {
                query = query.Where(filter => filter.Progress == progress.Value);
            }

            var getIssues = await query.Where(find => find.Project.Id == projectId).Include(user => user.AssignedTo).ToListAsync();

            if (getIssues.Count <= 0)
            {
                return new IssueResponse(new List<RetrieveIssue>(), 0, 0, 0, 0, 0, 0);
            }

            else
            {
                int pageResult = items;
                int currentPage = page.HasValue && page > 0 ? page.Value : 1;

                int totalItems = await query.CountAsync();
                int pageCount = (int)Math.Ceiling((double)totalItems / pageResult);

                var issues = await query.Skip((currentPage - 1) * pageResult)
                                        .Take(pageResult)
                                        .ToListAsync();

                totalItems = issues.Count;

                if (totalItems < 1)
                {
                    throw new Exception("Page doesn't exist");
                }

                int itemStart = (currentPage - 1) * pageResult + 1; ;

                int itemEnd = Math.Min(currentPage * pageResult, totalItems) + (itemStart - 1);


                var retrievedIssues = getIssues.Select(tsk => new RetrieveIssue
                {
                    id = tsk.Id,
                    Name = tsk.Name,
                    Progress = tsk.Progress,
                    Complexity = tsk.Complexity,
                    IssueType = tsk.IssueType,
                    EstimatedTimeInMinute = tsk.EstimatedTimeInMinutes,
                    TimeSpent = tsk.TimeSpent,
                    StartDate = tsk.StartDate,
                    EndDate = tsk.EndDate,
                    AssignedTo = tsk.AssignedTo != null ? tsk.AssignedTo.FirstName : " ",
                }).ToList();

                var reponse = new IssueResponse(retrievedIssues, currentPage, totalItems, pageCount, itemStart, itemEnd, getIssues.Count);
                return reponse;
            }
        }

        public async Task<IEnumerable<RetrieveIssue>> GetSubIssues(Guid parentId, Guid projectId)
        {
            var checkProject = await ValidateProject(projectId);

            var getParentIssue = await ParentIssue(parentId, checkProject.Id);

            var getSubIssues = await context.Issues.Where(iss => iss.ParentIssue.Id == getParentIssue.Id).ToListAsync();

            if (getSubIssues.Count < 1)
            {
                return new List<RetrieveIssue>();
            }

            var mappedIssue = getSubIssues.Select(sub => new RetrieveIssue
            {
                id = sub.Id,
                Name = sub.Name,
                IssueType = sub.IssueType,
            }).OrderDescending().ToList();

            return mappedIssue;
        }

        public async Task UnassignIssue(Guid taskId, Guid projectId)
        {
            var retrieveProject = await ValidateProject(projectId);

            var retrieveTask = await ValidateIssue(taskId, projectId);

            retrieveTask.AssignedTo = null;

            context.Issues.Update(retrieveTask);
            await context.SaveChangesAsync();
        }

        public async Task<string> UpdateIssues(Guid issueId, UpdateGroupTaskDto dto, Guid projectId, string mail)
        {
            var user = await config.GetUser(mail);

            await ValidateIssueUpdate(issueId, dto, projectId, user.Id);

            return "Task successful";
        }

        public async Task AddRelatedIssue(Guid originId, Guid stateId, Guid projectId)
        {
            var checkProject = await ValidateProject(projectId);

            var getOriginIssue = await ValidateIssue(originId, projectId);
            var getStateId = await ValidateIssue(stateId, projectId);

            var addToDb = context.IssueRelations.Add(new IssueRelation
            {
                Id = Guid.NewGuid(),
                IssueId = getOriginIssue.Id,
                RelatedIssueId = getStateId.Id,
            });

            await context.SaveChangesAsync();
        }

        public async Task RemoveRelatedIssue(Guid originId, Guid stateId, Guid projectId)
        {
            var checkProject = await ValidateProject(projectId);

            var getOriginIssue = await ValidateIssue(originId, projectId);
            var getStateId = await ValidateIssue(stateId, projectId);

            var relationToRemove = await context.IssueRelations.FirstOrDefaultAsync(ir =>
            ir.IssueId == getOriginIssue.Id && ir.RelatedIssueId == getStateId.Id);

            if (relationToRemove != null)
            {
                context.IssueRelations.Remove(relationToRemove);
                await context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException($"No relation found between tasks");
            }
        }

        public async Task<IEnumerable<IssueRelationDto>> GetRelatedIssues(Guid originId, Guid projectId)
        {
            var checkProject = await ValidateProject(projectId);

            var getOriginIssue = await ValidateIssue(originId, projectId);

            var retrieveRelated = await context.IssueRelations.Where(issue => issue.IssueId == getOriginIssue.Id).ToListAsync();

            var response = retrieveRelated.Select(iss => new IssueRelationDto
            {
                Id = originId,
                Relations = context.IssueRelations.Include(related => related.RelatedIssue).Where(org => org.IssueId == originId).Select(rel => new RelatedDto
                {
                    Id = rel.RelatedIssueId,
                    Name = rel.RelatedIssue.Name
                }).ToList(),
            }).ToList();

            return response;
        }

        private async Task<Project> ValidateProject(Guid projectId)
        {
            var validateProject = await context.Projects.FindAsync(projectId);
            if (validateProject == null)
            {
                throw new Exception("You need to created a project in other " +
                    "to have access to creating an issue");
            }
            return validateProject;
        }

        private int CheckDate(DateOnly startDate, DateOnly endDate)
        {
            int checkDate = endDate.DayNumber - startDate.DayNumber;

            int daysDifference = (checkDate == 0 ? 1 : checkDate) * 1440;

            return daysDifference;
        }

        private async Task<Issue> ValidateIssue(Guid issueId, Guid projectId)
        {
            var getIssue = await context.Issues.Include(user => user.AssignedTo)
                .Where(search => search.Id == issueId && search.Project.Id == projectId).SingleOrDefaultAsync();

            if (getIssue == null)
            {
                throw new Exception("Can not find this issue");
            }
            return getIssue;
        }


        private async Task<string> ValidateIssueUpdate(Guid id, UpdateGroupTaskDto dto, Guid projectId, Guid userId)
        {
            var retrieveProject = await ValidateProject(projectId);

            var getIssue = await ValidateIssue(id, retrieveProject.Id);

            if (!string.IsNullOrEmpty(dto.Name))
            {
                getIssue.Name = dto.Name;
            }

            if (!string.IsNullOrEmpty(dto.Description))
            {
                getIssue.Description = dto.Description;
            }

            if (dto.Complexity.HasValue)
            {
                getIssue.Complexity = dto.Complexity.Value;
            }

            if(dto.AssignedTo.HasValue)
            {
                var user = await config.GetUserById(dto.AssignedTo.Value.ToString());
                getIssue.AssignedTo = user;
            }

            //getIssue.TimeSpent += dto.TimeSpent != null ? (uint)dto.TimeSpent : default;

            if (dto.IssueLevel < 100 && dto.IssueLevel >= 0)
            {
                getIssue.IssueLevel = dto.IssueLevel != null ? (int)dto.IssueLevel.Value : default;
                getIssue.Progress = Progress.InProcess;
            }
            else if (dto.IssueLevel == 100)
            {
                getIssue.IssueLevel = 100;
                getIssue.Progress = Progress.Done;
            }
            else
            {
                if (getIssue.IssueLevel < 0 || getIssue.IssueLevel > 100)
                {
                    throw new InvalidOperationException("Must be within 0 to 100");
                }
            }
            var dateBetween = CheckDate(dto.StartDate, dto.EndDate);

            if (dto.StartDate > dto.EndDate)
            {
                throw new Exception("Check the date interval");
            }

            getIssue.StartDate = dto.StartDate;
            getIssue.EndDate = dto.EndDate;

            uint initializedTime = Convert.ToUInt32(dateBetween);

            if (dto.TimeSpent != null)
            {
                getIssue.TimeSpent += dto.TimeSpent <= initializedTime ? (uint)dto.TimeSpent : throw new Exception("Time spent exceed days range");
            }

            if (dto.EstimatedTimeInMinute.HasValue)
            {
                if (dto.EstimatedTimeInMinute > initializedTime)
                {
                    throw new Exception("estimated time exceeds days range");
                }
                else
                {
                    getIssue.EstimatedTimeInMinutes = dto.EstimatedTimeInMinute.Value;

                }
            }

            context.Issues.Update(getIssue);
            try
            {
                await context.IssueAnalysers.AddAsync(new IssueAnalyser
                {
                    Issue = getIssue,
                    Project = retrieveProject,
                    Note = dto.Note ?? string.Empty,
                    Comment = dto.Comment ?? string.Empty,
                    WorkComponent = dto.Workdone.HasValue ? dto.Workdone.Value : (WorkComponent?)null,
                    Id = Guid.NewGuid(),
                    User = userId, 
                    CreatedDate = DateTime.UtcNow,
                });
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            await context.SaveChangesAsync();
            return "successful";
        }

        private async Task<Issue> ParentIssue(Guid issueId, Guid projectId)
        {
            var getIssue = await ValidateIssue(issueId, projectId);

            if (getIssue.ParentIssue == getIssue)
            {
                throw new Exception("Task can be a sub task of itself");
            }
            return getIssue;
        }
    }
}
