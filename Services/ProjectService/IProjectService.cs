using Task_Management_System.DTOs.ProjectDto;
using Task_Management_System.Enums;

namespace Task_Management_System.Services.ProjectService
{
    public interface IProjectService
    {
        Task<string> CreateProject(CreateDto dto, string mail);

        Task<string> UpdateProject(Guid projectId, string? name, string? description, Progress? progress, Complexity? complexity, string mail);

        Task<string> DeleteProject(Guid projectId, string mail);

        Task<string> EditProject(Guid projectId, UpdateProjectDto dto, string mail);

        Task<GetProjectDto> GetProjectById(Guid projectId, string mail);

        Task<IEnumerable<GetProjectDto>> GetProjects(Progress? progress, Complexity? complexity, string mail);

        Task<ProjectResponse> GetProjectPaginated(Progress? progress, Complexity? complexity, int? page, int itemPerPage, string mail);
    }
}
