namespace Task_Management_System.DTOs.IssueDto
{
    public class DeadlineListDto
    {
        public string User { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public DateOnly EndDate { get; set; }
    }
}
