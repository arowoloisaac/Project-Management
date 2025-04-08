using Task_Management_System.Enums;

namespace Task_Management_System.DTOs.ProjectDto
{
    public class UpdateProjectDto
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Overview { get; set; }

        public Complexity? Complexity { get; set; }

        public Progress? Progress { get; set; }
    }
}
