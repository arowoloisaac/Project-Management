using Task_Management_System.DTOs.ProjectDto;

namespace Task_Management_System.Services.TaskAnalyserService
{
    public interface ITaskAnalyserService
    {
        Task<string> UpdateAnalysis(string? note, string? comment, Guid analysisId, string authorId);

        Task<string> DeleteAnalysis(Guid analysisId, string authorId);

        Task<IEnumerable<ProjectTimelineDto>> GetAnalyses(Guid projectId, string authorId);

        Task<IEnumerable<ProjectTimelineDto>> GetIssueAnalyses(Guid projectId, Guid issueId, string authorId);
    }
}
