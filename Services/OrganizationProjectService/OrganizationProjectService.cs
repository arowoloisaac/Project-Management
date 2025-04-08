using Microsoft.EntityFrameworkCore;
using Task_Management_System.Configurations;
using Task_Management_System.Data;
using Task_Management_System.DTOs.OrganizationProjectDto;
using Task_Management_System.DTOs.ProjectDto;
using Task_Management_System.Enums;
using Task_Management_System.Models;
using Task_Management_System.Services.UserConfiguration;

namespace Task_Management_System.Services.OrganizationProjectService
{
    public class OrganizationProjectService : IOrganizationProjectService
    {
        private readonly ApplicationDbContext context;
        private readonly IUserConfig config;

        private const string admin = ApplicationRoleNames.OrganizationAdministrator;

        public OrganizationProjectService(ApplicationDbContext context, IUserConfig config)
        {
            this.context = context;
            this.config = config;
        }

        public async Task<string> AssignProjectToGroup(Guid organizationId, Guid groupId, Guid projectId, string userId)
        {
            var retrieveOrgGroup = await context.Groups.Where(grp => grp.Id == groupId && grp.OrganizationId == organizationId).SingleOrDefaultAsync();

            if (retrieveOrgGroup == null)
            {
                throw new Exception("Either Organization or project doesn't exist");
            }

            var retrieveGrpProject = await context.Projects.Include(grp => grp.Group)
                .Where(prt => prt.Id == projectId && prt.OrganizationId == organizationId && prt.Group == null).SingleOrDefaultAsync();

            if (retrieveGrpProject.Group != null)
            {
                return "A group is already assigned to this project";
            }

            retrieveGrpProject.Group = retrieveOrgGroup;
            await context.Projects.SingleUpdateAsync(retrieveGrpProject);

            await context.SaveChangesAsync();
            return "Assigned group to project";
        }

        public async Task<string> CreateProject(CreateDto dto, Guid organizationId, Guid? groupId, string userId)
        {

            Group? retrieveGroup = null;
            if (groupId.HasValue)
            {
                retrieveGroup = await context.Groups.FindAsync(groupId);

                if (retrieveGroup == null)
                {
                    throw new Exception("Group does not exist");
                }
            }

            var retrieveOrganization = await retrieveOrgization(organizationId);

            var user = await config.GetUserById(userId);

            try
            {
                var validateProject = await context.Projects
                    .Where(dup => dup.Name == dto.Name && dup.Creator.UserName == user.UserName && dup.OrganizationId == organizationId).SingleOrDefaultAsync();

                if (validateProject != null)
                {
                    throw new Exception($"Project with {dto.Name} already exist in this organization");
                }
                else
                {
                    var createProject = new Project
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.Name,
                        Description = dto.Description,
                        CreatedBy = user.Id,
                        Creator = user,
                        CreatedTime = DateTime.UtcNow,
                        Complexity = dto.Complexity,
                        Progress = Progress.Todo,
                        Overview = dto.Overview,
                        Group = retrieveGroup,
                        OrganizationId = organizationId,
                    };
                    context.Projects.Add(createProject);

                    await context.SaveChangesAsync();

                    return "project created";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving the project: Server Error: " + ex.Message);
            }
        }

        public async Task<string> DeleteProject(Guid projectId, Guid organizationId, string userId)
        {
            var user = await config.GetUserById(userId);

            var organization = await retrieveOrgization(organizationId);

            var findProjectById = await context.Projects
                .Where(project => project.Id == projectId && project.CreatedBy == user.Id && project.OrganizationId == organizationId).SingleOrDefaultAsync();

            if (findProjectById == null)
            {
                throw new Exception($"Project with id: {projectId} doesn not exist in this organization");
            }
            else
            {
                var removeCreatedIssues = await context.Issues.Where(issue => issue.Project == findProjectById).ToListAsync();

                if (removeCreatedIssues.Count <= 0)
                {
                    //continue
                }
                context.Issues.RemoveRange(removeCreatedIssues);
                context.Projects.Remove(findProjectById);

                await context.SaveChangesAsync();

                return "Project data removed";
            }
        }

        public async Task<string> EditProject(Guid projectId, UpdateProjectDto dto, Guid organizationId, string userId)
        {
            var user = await config.GetUserById(userId);
            var org = await retrieveOrgization(organizationId);

            var findProject = await context.Projects.Where(org => org.Id == projectId && org.OrganizationId == organizationId).SingleOrDefaultAsync();

            if (findProject == null)
            {
                throw new Exception("Project doesn't exist");
            }
            else
            {
                if (!string.IsNullOrEmpty(dto.Name))
                {
                    findProject.Name = dto.Name;
                }

                if (!string.IsNullOrEmpty(dto.Description))
                {
                    findProject.Description = dto.Description;
                }

                if (dto.Progress.HasValue)
                {
                    findProject.Progress = dto.Progress.Value;
                }
                if (dto.Complexity.HasValue)
                {
                    findProject.Complexity = dto.Complexity.Value;
                }

                findProject.UpdatedBy = user.Id;
                findProject.UpdatedTime = DateTime.Now;

                await context.Projects.SingleUpdateAsync(findProject);

                return "updated";
            }
        }
        //this for the group user
        public async Task<IEnumerable<GetProjectDto>> GetGroupProjects(Guid organizationId, Guid groupId, string userId)
        {
            var retrieveGroup = await retrieveOrgizationGroup(organizationId, groupId);

            var retrieveProject = await context.Projects
                .Where(pjt => pjt.OrganizationId == organizationId && pjt.Group == retrieveGroup).ToListAsync();

            if (retrieveProject == null)
            {
                return new List<GetProjectDto>();
            }

            var response = retrieveProject.Select(itm => new GetProjectDto
            {
                Id = itm.Id,
                Name = itm.Name,
                Overview = itm.Overview,
                Complexity = itm.Complexity,
            }).ToList();

            return response;
        }


        //since this will be passed from the frontend, the admin page wont require groupId while the group will (can use 2 diffeent endpoint for this)
        public async Task<GetOrganizationProjectDto> GetProjectById(Guid projectId, string userId, Guid organizationId, Guid? groupId)
        {
            var org = await retrieveOrgization(organizationId);
            var getPt = await retrieveProject(projectId);

            var query = context.Projects
                .Include(p => p.Group)
                .Where(p => p.OrganizationId == organizationId && p.Id == projectId);

            if (groupId.HasValue)
            {
                query = query.Where(p => p.Group.Id == groupId);
            }

            var project = await query.SingleOrDefaultAsync();

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} does not exist or is not accessible.");
            }

            return new GetOrganizationProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                AssignedTo = project.Group?.Name,
                Overview = project.Overview,
                DateCreated = project.CreatedTime,
                Complexity = project.Complexity,
                AssignedGroupId = project.Group.Id
            };

        }

        //this is for the members of the organization
        public async Task<OrganizationProjectResponse> GetProjectPaginated(Progress? progress, Complexity? complexity, int? page, int itemPerPage, Guid organizationId, Guid groupId, string userId)
        {
            var user = await config.GetUserById(userId);

            var retrieveGroup = await retrieveOrgizationGroup(organizationId, groupId);

            int defaultItemsPerPage = 7;

            var items = itemPerPage == 0 ? defaultItemsPerPage : itemPerPage;
            int currentPage = page.HasValue && page > 0 ? page.Value : 1;

            var query = context.Projects
                .Include(user => user.Group)
                .Where(prj => prj.Group == retrieveGroup && prj.OrganizationId == organizationId);

            if (progress != null)
            {
                query = query.Where(org => org.Progress == progress);
            }

            if (complexity != null)
            {
                query = query.Where(org => org.Complexity == complexity);
            }
            int totalItems = await query.CountAsync();

            if (totalItems == 0)
            {
                return new OrganizationProjectResponse(new List<GetOrganizationProjectDto>(), 0, 0, 0, 0, 0, 0);
            }

            int pageCount = (int)Math.Ceiling((double)totalItems / items);
            int itemStart = (currentPage - 1) * items + 1;
            int itemEnd = Math.Min(currentPage * items, totalItems);

            var projectList = await query
                .Skip((currentPage - 1) * items)
                .Take(items)
                .ToListAsync();

            var mappedProject = projectList.Select(project => new GetOrganizationProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                AssignedTo = project.Group.Name,
                Overview = project.Overview,
                Complexity = project.Complexity,
                Progress = project.Progress,
            }).ToList();

            return new OrganizationProjectResponse(mappedProject, currentPage, totalItems, pageCount, itemStart, itemEnd, projectList.Count);
        }

        //for the admin
        public async Task<IEnumerable<GetOrganizationProjectDto>> GetProjects(Progress? progress, Complexity? complexity,
            bool? isAssigned, Guid organizationId, string userId)
        {
            var org = await retrieveOrgization(organizationId);

            IQueryable<Project> query = context.Projects.Include(group => group.Group).Where(findProjects => findProjects.OrganizationId == organizationId);

            if (progress.HasValue)
            {
                query = query.Where(project => project.Progress == progress);
            }

            if (complexity.HasValue)
            {
                query = query.Where(project => project.Complexity == complexity);
            }

            if (isAssigned.HasValue)
            {
                if (isAssigned == true)
                {
                    query = query.Where(project => project.Group != null);
                }

                else
                {
                    query = query.Where(project => project.Group == null);
                }
            }


            var projectList = await query.ToListAsync();

            if (projectList.Count == 0)
            {
                return new List<GetOrganizationProjectDto>();
            }

            else
            {
                var projects = projectList.Select(project => new GetOrganizationProjectDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    Progress = project.Progress,
                    Complexity = project.Complexity,
                    Overview = project.Overview,
                    AssignedTo = project.Group == null ? null : project.Group.Name,
                    AssignedGroupId = project.Group == null ? null : project.Group.Id
                }).ToList();

                return projects;
            }
        }


        public async Task<string> UpdateProject(Guid projectId, string? name, string? description, Progress? progress, Complexity? complexity, Guid organizationId, string userId)
        {
            var user = await config.GetUserById(userId);

            var findProject = await context.Projects.Where(org => org.Id == projectId && org.OrganizationId == organizationId).SingleOrDefaultAsync();

            if (findProject == null)
            {
                throw new Exception("Project doesn't exist");
            }
            else
            {
                if (!string.IsNullOrEmpty(name))
                {
                    findProject.Name = name;
                }

                if (!string.IsNullOrEmpty(description))
                {
                    findProject.Description = description;
                }

                if (progress.HasValue)
                {
                    findProject.Progress = progress.Value;
                }
                if (complexity.HasValue)
                {
                    findProject.Complexity = complexity.Value;
                }

                findProject.UpdatedBy = user.Id;
                findProject.UpdatedTime = DateTime.Now;

                var updateResponse = context.Projects.Update(findProject);

                if (updateResponse is null)
                {
                    throw new InvalidOperationException("unable to update changes");
                }

                await context.SaveChangesAsync();

                return "updated";
            }
        }

        public async Task<string> UnassignProjectToGroup(Guid organizationId, Guid groupId, Guid projectId)
        {
            var retrieveOrgGroup = await context.Groups.Where(grp => grp.Id == groupId && grp.OrganizationId == organizationId).SingleOrDefaultAsync();

            if (retrieveOrgGroup == null)
            {
                throw new Exception("Either Organization or project doesn't exist");
            }

            var retrieveGrpProject = await context.Projects
                .Where(prt => prt.Id == projectId && prt.Group == retrieveOrgGroup && prt.OrganizationId == organizationId).SingleOrDefaultAsync();

            if (retrieveGrpProject == null)
            {
                return "An error here, these group doesn't have the project assigned to them";
            }

            retrieveGrpProject.Group = null;

            await context.Projects.SingleUpdateAsync(retrieveGrpProject);

            return "Group unassigned to project";
        }

        private async Task<Organization> retrieveOrgization(Guid organizationId)
        {
            var org = await context.Organizations.FindAsync(organizationId);

            if (org == null)
            {
                throw new Exception("Organization does not exist");
            }
            return org;
        }

        private async Task<Group> retrieveOrgizationGroup(Guid organizationId, Guid groupId)
        {
            var org = await retrieveOrgization(organizationId);

            var retrieveGroup = await context.Groups.Where(og => og.Id == groupId && og.OrganizationId == org.Id).FirstOrDefaultAsync();

            if (retrieveGroup == null)
            {
                throw new Exception("Group does not exist in the organization");
            }

            return retrieveGroup;
        }

        private async Task<Project> retrieveProject(Guid projectId)
        {
            var project = await context.Projects.FindAsync(projectId);

            if (project == null)
            {
                throw new Exception("Project does not exist");
            }
            return project;
        }


    }
}
