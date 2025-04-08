using Task_Management_System.Enums;

namespace Task_Management_System.DTOs.IssueDto
{
    public class IssueAndChild
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Complexity Complexity { get; set; }

        public IssueType IssueType { get; set; }

        public Progress Progress { get; set; }

        public DateOnly StartDate { get; set; }

        //this serves as progress 
        public int IssueLevel { get; set; } = 0;

        //this works for the minute
        //public uint EstimatedTimeInMinutes { get; set; }

        public string AssignedTo { get; set; } = string.Empty;

        public DateOnly EndDate { get; set; }

        public List<IssueAndChild> SubIssue { get; set; } = new List<IssueAndChild>();
    }
}
