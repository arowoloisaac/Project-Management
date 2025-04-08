using Task_Management_System.Enums;

namespace Task_Management_System.DTOs.IssueDto
{
    public class RetrieveIssue
    {
        public Guid id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Complexity Complexity { get; set; }

        public IssueType IssueType { get; set; }

        public Progress Progress { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public uint EstimatedTimeInMinute { get; set; }

        public uint TimeSpent { get; set; }

        public int IssueLevel { get; set; }

        public string AssignedTo { get; set; } = string.Empty;
    }
}
