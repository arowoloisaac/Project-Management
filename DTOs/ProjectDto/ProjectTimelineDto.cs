namespace Task_Management_System.DTOs.ProjectDto
{
    public class ProjectTimelineDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProjectId { get; set; }

        public string IssueName { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
