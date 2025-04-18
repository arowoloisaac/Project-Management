using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Task_Management_System.Data;
using Task_Management_System.DTOs.ProjectDto;
using Task_Management_System.Enums;
using Task_Management_System.Models;
using Task_Management_System.Services.UserConfiguration;

namespace Task_Management_System.Services.ProjectService
{
    public class ProjectService : IProjectService
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserConfig _userConfig;
        public int paginationSize = 12;

        public ProjectService(UserManager<User> userManager, ApplicationDbContext context, IMapper mapper, IUserConfig userConfig)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
            _userConfig = userConfig;
        }


        public async Task<string> CreateProject(CreateDto dto, string mail)
        {
            var user = await _userConfig.GetUser(mail);

            try
            {

                var validateProject = await _context.Projects.Where(duplicateName => duplicateName.Name == dto.Name && duplicateName.Creator.UserName == user.UserName).SingleOrDefaultAsync();

                if (validateProject != null)
                {
                    throw new Exception($"Project with {dto.Name} already exist");
                }
                else
                {
                    //make sure start date can't be yesterday and also end date can't be less than start date
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
                        Overview = dto.Overview
                    };
                    _context.Projects.Add(createProject);

                    await _context.SaveChangesAsync();

                    return "project created";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }


        public async Task<string> DeleteProject(Guid projectId, string mail)
        {
            var user = await _userConfig.GetUser(mail);

            var findProjectById = await _context.Projects.Where(project => project.Id == projectId && project.CreatedBy == user.Id).SingleOrDefaultAsync();

            if (findProjectById == null)
            {
                throw new Exception($"Project with id: {projectId} not found");
            }
            else
            {
                var removeCreatedIssues = await _context.Issues.Where(issue => issue.Project == findProjectById).ToListAsync();

                if (removeCreatedIssues.Count <= 0)
                {
                    //continue
                }
                _context.Issues.RemoveRange(removeCreatedIssues);
                _context.Projects.Remove(findProjectById);

                await _context.SaveChangesAsync();

                return "Project infos removed";
            }
        }


        public async Task<GetProjectDto> GetProjectById(Guid projectId, string mail)
        {
            var user = await _userConfig.GetUser(mail);

            var project = await _context.Projects.FindAsync(projectId);

            if (project == null)
            {
                throw new Exception($"Project with id {projectId} doesn't exist");
            }

            else
            {
                return new GetProjectDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    Progress = project.Progress,
                    Overview = project.Overview,
                    DateCreated = project.CreatedTime,
                    Complexity = project.Complexity,
                };
            }
        }


        //to add pagination later on, in which there will be a default value of 10-12 and also the user can also set a default value
        public async Task<IEnumerable<GetProjectDto>> GetProjects(Progress? progress, Complexity? complexity, string mail)
        {
            var user = await _userConfig.GetUser(mail);

            IQueryable<Project> query = _context.Projects.Where(findProjects => findProjects.CreatedBy == user.Id);

            if (progress.HasValue)
            {
                query = query.Where(project => project.Progress == progress);
            }

            if (complexity.HasValue)
            {
                query = query.Where(project => project.Complexity == complexity);
            }

            var projectList = await query.ToListAsync();

            if (projectList.Count == 0)
            {
                return new List<GetProjectDto>();
            }

            else
            {
                var projects = projectList.Select(project => new GetProjectDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    Progress = project.Progress,
                    Complexity = project.Complexity,
                    Overview = project.Overview
                }).ToList();

                return projects;
            }
        }

        //to be completed later on using the paginationSize field, here to give the user choice 
        public async Task<ProjectResponse> GetProjectPaginated(Progress? progress, Complexity? complexity, int? page, int itemPerPage, string mail)
        {
            var user = await _userConfig.GetUser(mail);

            int defaultItemsPerPage = 10;

            var items = itemPerPage == 0 ? defaultItemsPerPage : itemPerPage;

            IQueryable<Project> query = _context.Projects.Where(findProjects => findProjects.CreatedBy == user.Id && findProjects.OrganizationId == null);

            if (progress.HasValue)
            {
                query = query.Where(project => project.Progress == progress);
            }

            if (complexity.HasValue)
            {
                query = query.Where(project => project.Complexity == complexity);
            }

            var projectList = await query.ToListAsync();

            if (projectList.Count == 0)
            {
                return new ProjectResponse(new List<GetProjectDto>(), 0, 0, 0, 0, 0, 0);
                //throw new Exception("page doesn't exist");
            }

            else
            {
                int pageResult = items;
                int currentPage = page.HasValue && page > 0 ? page.Value : 1;

                int totalItems = await query.CountAsync();
                int pageCount = (int)Math.Ceiling((double)totalItems / pageResult);

                var projects = await query.Skip((currentPage - 1) * pageResult)
                                        .Take(pageResult)
                                        .ToListAsync();

                totalItems = projects.Count;

                if (totalItems < 1)
                {
                    throw new Exception("Page doesn't exist");
                }

                int itemStart = (currentPage - 1) * pageResult + 1; ;

                int itemEnd = Math.Min(currentPage * pageResult, totalItems) + (itemStart - 1);

                var mappedProjects = projects.Select(project => new GetProjectDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    Progress = project.Progress,
                    Complexity = project.Complexity,
                    Overview = project.Overview,
                });
                var response = new ProjectResponse(mappedProjects.ToList(), currentPage, totalItems, pageCount, itemStart, itemEnd, projectList.Count);
                return response;
            }
        }


        public async Task<string> UpdateProject(Guid projectId, string? Name, string? Description, Progress? progress, Complexity? complexity, string mail)
        {
            var user = await _userConfig.GetUser(mail);

            var findProject = await _context.Projects.FindAsync(projectId);

            if (findProject == null)
            {
                throw new Exception("Project doesn't exist");
            }
            else
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    findProject.Name = Name;
                }

                if (!string.IsNullOrEmpty(Description))
                {
                    findProject.Description = Description;
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

                var updateResponse = _context.Projects.Update(findProject);

                if (updateResponse is null)
                {
                    throw new InvalidOperationException("invalid");
                }

                await _context.SaveChangesAsync();

                return "updated";
            }
        }

        public async Task<string> EditProject(Guid projectId, UpdateProjectDto dto, string mail)
        {
            var user = await _userConfig.GetUser(mail);

            var findProject = await _context.Projects.FindAsync(projectId);

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

                var updateResponse = _context.Projects.Update(findProject);

                if (updateResponse is null)
                {
                    throw new InvalidOperationException("invalid");
                }

                await _context.SaveChangesAsync();

                return "updated";
            }
        }

    }
}
