namespace Task_Management_System.Models
{
    public class IssueRelation
    {
        public Guid Id { get; set; }

        public Guid IssueId { get; set; }

        public Guid RelatedIssueId { get; set; }

        public Issue? RelatedIssue { get; set; }
    }
}
