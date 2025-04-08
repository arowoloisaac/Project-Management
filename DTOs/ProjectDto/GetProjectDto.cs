using Task_Management_System.Enums;

namespace Task_Management_System.DTOs.ProjectDto
{
    public class GetProjectDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Overview { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Complexity Complexity { get; set; }

        public Progress Progress { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
