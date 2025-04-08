using Task_Management_System.Models;

namespace Task_Management_System.DTOs.IssueDto
{
    public class IssueResponse
    {
        public IEnumerable<RetrieveIssue> Issues { get; set; } = new List<RetrieveIssue>();

        public Pagination Pagination { get; set; }

        public IssueResponse(List<RetrieveIssue> issue, int page, int total, int count, int start, int end, int totalItem)
        {
            Issues = issue;

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
