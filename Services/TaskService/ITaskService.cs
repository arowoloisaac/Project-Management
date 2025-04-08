using Task_Management_System.DTOs.IssueDto;
using Task_Management_System.Enums;

namespace Task_Management_System.Services.TaskService
{
    public interface ITaskService
    {
        //to be able to filter with task assigned to the actor
        Task<IEnumerable<RetrieveIssue>> GetIssues(IssueType? issueType, Complexity? complexity, Progress? progress, Guid projectId, string mail);

        Task<IssueResponse> GetIssuesPaginated(IssueType? issueType, Complexity? complexity, Progress? progress, int? page, int itemPerPage, Guid projectId, string mail);

        Task<string> CreateIssues(Guid projectId, CreateIssue issueDto, string mail);

        Task<string> UpdateIssues(Guid issueId, UpdateIssueDto dto, Guid projectId, string mail);

        Task<string> DeleteIssues(Guid issueId, Guid projectId, bool isDeleteChildren, string mail);

        Task<string> CreateSubIssue(Guid projectId, CreateIssue subIssueDto, Guid parentIssueId, string mail);

        Task<List<RetrieveIssue>> GetIssue(Guid projectId, string userMail);

        Task<IEnumerable<RetrieveIssue>> GetSubIssues(Guid parentId, Guid projectId, string userId);

        Task<RetrieveIssue> GetIssueById(Guid projectId, Guid issueId, string userId);

        Task AddRelatedIssue(Guid originId, Guid stateId, Guid projectId, string userId);

        Task RemoveRelatedIssue(Guid originId, Guid issueId, Guid projectId, string userId);

        Task<IEnumerable<IssueRelationDto>> GetRelatedIssues(Guid issueId, Guid projectId, string userId);

        Task<IEnumerable<IssueAndChild>> GetIssueAndChild(Guid projectId, string userMail);

        Task<IEnumerable<DeadlineListDto>> IssueDeadlineList();
    }
}
