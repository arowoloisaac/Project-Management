using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Task_Management_System.Data;
using Task_Management_System.DTOs.IssueDto;
using Task_Management_System.Enums;
using Task_Management_System.Models;
using Task_Management_System.Services.UserConfiguration;

namespace Task_Management_System.Services.TaskService
{
    public class TaskService: ITaskService
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserConfig _userConfig;

        public TaskService(UserManager<User> userManager, ApplicationDbContext context, IMapper mapper, IUserConfig userConfig)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
            _userConfig = userConfig;
        }

        public async Task<string> CreateIssues(Guid projectId, CreateIssue issueDto, string mail)
        {
            var user = await _userConfig.GetUser(mail);

            var checkProject = await ValidateProject(projectId);

            var validateIssueName = await _context.Issues
                .Where(issue => issue.Name == issueDto.Name && issue.Project.Id == projectId).SingleOrDefaultAsync();

            if (validateIssueName == null)
            {
                if (issueDto.StartDate <= issueDto.EndDate)
                {
                    int date = CheckDate(issueDto.StartDate, issueDto.EndDate);

                    if (date >= issueDto.EstimatedTimeInMinutes)
                    {
                        var newIssue = await _context.Issues.AddAsync(new Issue
                        {
                            Id = Guid.NewGuid(),
                            Name = issueDto.Name,
                            Description = issueDto.Description,
                            //AssignedUserTo = issueDto.AssignedTo ?? null,//String.IsNullOrEmpty(userrole.RoleId) ? (Guid?)null : new Guid(userrole.RoleId)
                            EstimatedTimeInMinutes = issueDto.EstimatedTimeInMinutes,
                            CreatedBy = user,
                            CreatedDate = DateTime.Now,
                            StartDate = issueDto.StartDate,
                            EndDate = issueDto.EndDate,
                            Complexity = issueDto.Complexity,
                            Progress = Progress.Todo,
                            IssueType = issueDto.IssueType,
                            Project = checkProject,
                            AssignedTo = user,
                            User = user,

                        });
                        await _context.SaveChangesAsync();
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

        public async Task<IssueResponse> GetIssuesPaginated(IssueType? issueType, Complexity? complexity, Progress? progress, int? page, int itemPerPage, Guid projectId, string mail)
        {
            var user = await _userConfig.GetUser(mail);
            IQueryable<Issue> query = _context.Issues;

            int defaultItemsPerPage = 7;

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

            var getIssues = await query.Where(find => find.Project.Id == projectId && find.CreatedBy == user).ToListAsync();

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


                var retrievedIssues = getIssues.Select(find => new RetrieveIssue
                {
                    id = find.Id,
                    Name = find.Name,
                    Progress = find.Progress,
                    Complexity = find.Complexity,
                    IssueType = find.IssueType,
                }).ToList();

                var reponse = new IssueResponse(retrievedIssues, currentPage, totalItems, pageCount, itemStart, itemEnd, getIssues.Count);
                return reponse;
            }
        }

        public async Task<string> CreateSubIssue(Guid projectId, CreateIssue issueDto, Guid parentIssueId, string mail)
        {
            var user = await _userConfig.GetUser(mail);

            var checkProject = await ValidateProject(projectId);

            var getParentIssue = await ValidateIssue(parentIssueId, checkProject.Id, user.Id);

            var validateIssueName = await _context.Issues
                .Where(issue => issue.Name == issueDto.Name && issue.Project.Id == projectId).SingleOrDefaultAsync();

            if (validateIssueName != null)
            {
                throw new Exception("Killer being");
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
                            var newIssue = await _context.Issues.AddAsync(new Issue
                            {
                                Id = Guid.NewGuid(),
                                Name = issueDto.Name,
                                Description = issueDto.Description,
                                //AssignedTo = issueDto.AssignedTo,
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
                            _context.Issues.Update(getParentIssue);

                            await _context.SaveChangesAsync();
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

        public async Task<string> DeleteIssues(Guid issueId, Guid projectId, bool isDeleteChildren, string mail)
        {
            try
            {
                var user = await _userConfig.GetUser(mail);

                var checkProject = await ValidateProject(projectId);

                var issue = await ValidateIssue(issueId, checkProject.Id, user.Id);

                var issueChildren = await _context.Issues
                        .Where(filter => filter.ParentIssue.Id == issueId)
                        .ToListAsync();

                if (isDeleteChildren)
                {
                    _context.Issues.RemoveRange(issueChildren);
                }
                else
                {
                    foreach (var child in issueChildren)
                    {
                        child.ParentIssue = null;
                    }
                }

                _context.Issues.Remove(issue);

                await _context.SaveChangesAsync();

                return "deleted successfully";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<RetrieveIssue>> GetIssues(IssueType? issueType, Complexity? complexity, Progress? progress, Guid projectId, string mail)
        {
            var user = await _userConfig.GetUser(mail);
            IQueryable<Issue> query = _context.Issues;

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

            var getAllIssues = await query.Where(find => find.Project.Id == projectId && find.CreatedBy == user).ToListAsync();

            if (getAllIssues.Count <= 0)
            {
                return new List<RetrieveIssue>();
            }

            var response = getAllIssues.Select(find => new RetrieveIssue
            {
                id = find.Id,
                Name = find.Name,
                Progress = find.Progress,
                Complexity = find.Complexity,
                IssueType = find.IssueType,
            }).ToList();
            return response;
        }

        public async Task<string> UpdateIssues(Guid issueId, UpdateIssueDto dto, Guid projectId, string mail)
        {
            var user = await _userConfig.GetUser(mail);

            await ValidateIssueUpdate(issueId, dto, projectId, user.Id);

            return "Task successful";
        }

        public async Task<RetrieveIssue> GetIssueById(Guid projectId, Guid issueId, string userId)
        {
            var exactIssue = await ValidateIssue(issueId, projectId, Guid.Parse(userId));

            return new RetrieveIssue
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
            };
        }

        public async Task<IEnumerable<IssueAndChild>> GetIssueAndChild(Guid projectId, string userMail)
        {
            var user = await _userConfig.GetUser(userMail);
            var issues = await _context.Issues
               .Where(issue => issue.Project.Id == projectId && issue.User.Id == user.Id) // Get only parent issues
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
                SubIssue = _context.Issues
                    .Where(sub => sub.ParentIssue.Id == issue.Id)
                    .Select(sub => new IssueAndChild
                    {
                        Id = sub.Id,
                        Name = sub.Name,
                        IssueType = sub.IssueType
                    })
                    .ToList(),
            }).ToList();

            return response;
        }

        public async Task<List<RetrieveIssue>> GetIssue(Guid projectId, string userMail)
        {
            var user = await _userConfig.GetUser(userMail);

            var list = await _context.Issues.Where(issues => issues.Project.Id == projectId && issues.User.Id == user.Id).ToListAsync();

            if (list.Count == 0)
            {
                return new List<RetrieveIssue>();
            }

            else
            {
                var response = list.Select(issue => new RetrieveIssue
                {
                    id = issue.Id,
                    Name = issue.Name,
                    Progress = issue.Progress,
                    Complexity = issue.Complexity,
                    IssueType = issue.IssueType,
                    EndDate = issue.EndDate,
                    StartDate = issue.StartDate,
                }).ToList();

                return response;
            }
        }

        public async Task<IEnumerable<RetrieveIssue>> GetSubIssues(Guid parentId, Guid projectId, string userId)
        {
            var user = await _userConfig.GetUserById(userId);

            var checkProject = await ValidateProject(projectId);

            var getParentIssue = await ParentIssue(parentId, checkProject.Id, user.Id);

            var getSubIssues = await _context.Issues.Where(iss => iss.ParentIssue.Id == getParentIssue.Id).ToListAsync();

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

        public async Task<IEnumerable<DeadlineListDto>> IssueDeadlineList()
        {
            DateOnly dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            DateOnly threshold = dateNow.AddDays(1);

            var deadlineList = await _context.Issues
                .Where(iss => iss.EndDate <= threshold && iss.EndDate > dateNow &&
                (iss.Progress != Progress.Canceled || iss.Progress != Progress.Done)
                && iss.Project.OrganizationId == null && iss.User != null)
                .Include(user => user.User)
                .ToListAsync();

            var selectedList = deadlineList.Select(iss => new DeadlineListDto
            {
                EndDate = iss.EndDate,
                Name = iss.Name,
                User = iss.User.UserName
            }).ToList();

            return selectedList;
        }


        public async Task AddRelatedIssue(Guid originId, Guid stateId, Guid projectId, string userId)
        {
            var user = await _userConfig.GetUserById(userId);
            var checkProject = await ValidateProject(projectId);

            var getOriginIssue = await ValidateIssue(originId, projectId, user.Id);
            var getStateId = await ValidateIssue(stateId, projectId, user.Id);

            var addToDb = _context.IssueRelations.Add(new IssueRelation
            {
                Id = Guid.NewGuid(),
                IssueId = getOriginIssue.Id,
                RelatedIssueId = getStateId.Id,
            });

            await _context.SaveChangesAsync();
        }

        public async Task RemoveRelatedIssue(Guid originId, Guid stateId, Guid projectId, string userId)
        {
            var user = await _userConfig.GetUserById(userId);
            var checkProject = await ValidateProject(projectId);

            var getOriginIssue = await ValidateIssue(originId, projectId, user.Id);
            var getStateId = await ValidateIssue(stateId, projectId, user.Id);

            var relationToRemove = await _context.IssueRelations.FirstOrDefaultAsync(ir =>
            ir.IssueId == getOriginIssue.Id && ir.RelatedIssueId == getStateId.Id);

            if (relationToRemove != null)
            {
                _context.IssueRelations.Remove(relationToRemove);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException($"No relation found between tasks");
            }
        }

        public async Task<IEnumerable<IssueRelationDto>> GetRelatedIssues(Guid originId, Guid projectId, string userId)
        {
            var user = await _userConfig.GetUserById(userId);
            var checkProject = await ValidateProject(projectId);

            var getOriginIssue = await ValidateIssue(originId, projectId, user.Id);

            var retrieveRelated = await _context.IssueRelations.Where(issue => issue.IssueId == getOriginIssue.Id).ToListAsync();

            var response = retrieveRelated.Select(iss => new IssueRelationDto
            {
                Id = originId,
                Relations = _context.IssueRelations.Include(related => related.RelatedIssue).Where(org => org.IssueId == originId).Select(rel => new RelatedDto
                {
                    Id = rel.RelatedIssueId,
                    Name = rel.RelatedIssue.Name
                }).ToList(),
            }).ToList();

            return response;
        }


        private async Task<string> ValidateIssueUpdate(Guid id, UpdateIssueDto dto, Guid projectId, Guid userId)
        {
            var retrieveProject = await ValidateProject(projectId);

            var getIssue = await ValidateIssue(id, retrieveProject.Id, userId);

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

            //uint initializedTime = getIssue.TimeSpent + timeSpent;

            getIssue.TimeSpent += dto.TimeSpent != null ? (uint)dto.TimeSpent : default;

            uint initializedTime = getIssue.TimeSpent;
            if (dto.EstimatedTimeInMinute.HasValue)
            {
                getIssue.EstimatedTimeInMinutes = dto.EstimatedTimeInMinute.Value;

                if (initializedTime > dto.EstimatedTimeInMinute)
                {
                    throw new Exception("Time spent can't be greater than estimated time");
                }
                else
                {
                    uint v = initializedTime > dto.EstimatedTimeInMinute ? throw new Exception("reduce time spent") : getIssue.TimeSpent = initializedTime;

                    if (dto.IssueLevel < 100 && dto.IssueLevel > 0 || v > 0)
                    {
                        getIssue.IssueLevel = dto.IssueLevel != null ? (int)dto.IssueLevel.Value : default;
                        getIssue.Progress = Progress.InProcess;
                    }
                    else if (dto.IssueLevel == 100 || v == dto.EstimatedTimeInMinute)
                    {
                        getIssue.IssueLevel = 100;
                        getIssue.Progress = Progress.Done;
                    }
                    else
                    {
                        if (getIssue.IssueLevel == 0)
                        {
                            //continue
                        }
                        else if (getIssue.IssueLevel < 0)
                        {
                            throw new InvalidOperationException("can't be less than zero");
                        }
                        else
                        {
                            throw new Exception("can't validate action");
                        }
                    }
                }
            }
            else
            {
                if (initializedTime > getIssue.EstimatedTimeInMinutes)
                {
                    throw new Exception(" can't be greater than the time spent");
                }
                else
                {

                    uint v = initializedTime > dto.EstimatedTimeInMinute ? throw new Exception("reduce time spent") : getIssue.TimeSpent = initializedTime;
                    if (dto.IssueLevel < 100 && dto.IssueLevel > 0)
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
                        if (getIssue.IssueLevel == 0)
                        {
                            //continue
                        }
                        else if (getIssue.IssueLevel < 0)
                        {
                            throw new InvalidOperationException("can't be less than zero");
                        }
                        else
                        {
                            throw new Exception("can't validate action");
                        }
                    }
                }
            }

            _context.Issues.Update(getIssue);
            try
            {
                await _context.IssueAnalysers.AddAsync(new IssueAnalyser
                {
                    Issue = getIssue,
                    Project = retrieveProject,
                    Note = dto.Note ?? string.Empty,
                    Comment = dto.Comment ?? string.Empty,
                    WorkComponent = dto.Workdone.HasValue ? dto.Workdone.Value : (WorkComponent?)null,
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    User = userId
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            await _context.SaveChangesAsync();
            return "successful";
        }

        private int CheckDate(DateOnly startDate, DateOnly endDate)
        {
            int checkDate = endDate.DayNumber - startDate.DayNumber;

            int daysDifference = (checkDate == 0 ? 1 : checkDate) * 1440;

            return daysDifference;
        }

        private async Task<Project> ValidateProject(Guid projectId)
        {
            var validateProject = await _context.Projects.FindAsync(projectId);

            if (validateProject == null)
            {
                throw new Exception("You need to created a project in other " +
                    "to have access to creating an issue");
            }
            return validateProject;
        }

        private async Task<Issue> ValidateIssue(Guid issueId, Guid projectId, Guid userId)
        {
            var user = await _userConfig.GetUserById(userId.ToString());
            var getIssue = await _context.Issues.Where(search => search.Id == issueId && search.CreatedBy == user && search.Project.Id == projectId).SingleOrDefaultAsync();

            if (getIssue == null)
            {
                throw new Exception("Can not find this issue");
            }
            return getIssue;
        }

        //to check if the parent issue is the sub issue of itself
        private async Task<Issue> ParentIssue(Guid issueId, Guid projectId, Guid userId)
        {
            var getIssue = await ValidateIssue(issueId, projectId, userId);

            if (getIssue.ParentIssue == getIssue)
            {
                throw new Exception("Issue can be a sub task of itself");
            }
            return getIssue;
        }
    }
}
