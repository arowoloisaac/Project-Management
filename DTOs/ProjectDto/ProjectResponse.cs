using Task_Management_System.Models;

namespace Task_Management_System.DTOs.ProjectDto
{
    public class ProjectResponse
    {
        public IEnumerable<GetProjectDto> Projects { get; set; } = new List<GetProjectDto>();

        public Pagination Pagination { get; set; }

        public ProjectResponse(List<GetProjectDto> project, int page, int total, int count, int start, int end, int totalItem)
        {
            Projects = project;

            Pagination = new Pagination
            {
                Count = count,
                Current = page,
                Size = total,
                Start = start,
                End = end,
                TotalItems = totalItem
            };
        }
    }
}
