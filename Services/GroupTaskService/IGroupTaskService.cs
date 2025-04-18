using Task_Management_System.DTOs.GroupIssueDto;
using Task_Management_System.DTOs.IssueDto;
using Task_Management_System.Enums;

namespace Task_Management_System.Services.GroupIssueService
{
    public interface IGroupTaskService
    {
        Task<IEnumerable<RetrieveIssue>> GetIssues(IssueType? issueType, Complexity? complexity, Progress? progress, Guid projectId);

        Task<IssueResponse> GetIssuesPaginated(IssueType? issueType, Complexity? complexity, Progress? progress, int? page, int itemPerPage, Guid projectId);

        Task<string> CreateIssues(Guid projectId, CreateGroupIssueDto issueDto, string mail);

        Task<string> UpdateIssues(Guid issueId, UpdateGroupTaskDto dto, Guid projectId, string mail);

        Task<string> CreateChildTask(Guid projectId, CreateGroupIssueDto subIssueDto, Guid parentIssueId, string mail);

        Task<string> DeleteIssues(Guid issueId, Guid projectId, bool isDeleteChildren);

        Task<List<RetrieveIssue>> GetIssue(Guid projectId);

        Task<IEnumerable<RetrieveIssue>> GetSubIssues(Guid parentId, Guid projectId);

        Task<RetrieveGroupIssueDto> GetIssueById(Guid projectId, Guid issueId);

        Task AssignIssue(Guid taskId, Guid assignTo, Guid projectId);

        Task UnassignIssue(Guid taskId, Guid projectId);

        Task AddRelatedIssue(Guid originId, Guid stateId, Guid projectId);

        Task RemoveRelatedIssue(Guid originId, Guid issueId, Guid projectId);

        Task<IEnumerable<IssueRelationDto>> GetRelatedIssues(Guid issueId, Guid projectId);

        Task<IEnumerable<IssueAndChild>> GetIssueAndChild(Guid projectId);
    }
}
