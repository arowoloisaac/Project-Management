using Task_Management_System.Enums;

namespace Task_Management_System.DTOs.IssueDto
{
    public class UpdateIssueDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Complexity? Complexity { get; set; }
        public uint? EstimatedTimeInMinute { get; set; }
        public uint? TimeSpent { get; set; }
        public int? IssueLevel { get; set; }
        public string? Comment { get; set; }
        public string? Note { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }
        public WorkComponent? Workdone { get; set; }
    }
}
