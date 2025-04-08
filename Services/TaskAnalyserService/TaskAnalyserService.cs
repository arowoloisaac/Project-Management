using Microsoft.EntityFrameworkCore;
using Task_Management_System.Data;
using Task_Management_System.DTOs.ProjectDto;
using Task_Management_System.Services.UserConfiguration;

namespace Task_Management_System.Services.TaskAnalyserService
{
    public class TaskAnalyserService : ITaskAnalyserService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IUserConfig userConfig;
        public TaskAnalyserService(ApplicationDbContext dbContext, IUserConfig userConfig)
        {
            _dbcontext = dbContext;
            this.userConfig = userConfig;
        }

        public Task<string> DeleteAnalysis(Guid analysisId, string authorId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ProjectTimelineDto>> GetAnalyses(Guid projectId, string authorId)
        {
            var retrieveUser = await userConfig.GetUserById(authorId);

            var retrieveProject = await _dbcontext.Projects.Where(itm => itm.Id == projectId && itm.CreatedBy == retrieveUser.Id).SingleOrDefaultAsync();

            if (retrieveProject == null)
            {
                throw new Exception("Project does not belong to user");
            }
            else
            {
                var retrieveTimeline = await _dbcontext.IssueAnalysers
                    .Include(prt => prt.Project).Include(iss => iss.Issue)
                    .Where(itm => itm.Project.Id == projectId && itm.User == retrieveUser.Id).ToListAsync();

                if (retrieveTimeline.Count < 1)
                {
                    return Enumerable.Empty<ProjectTimelineDto>();
                }

                var obj = retrieveTimeline.Select(itm => new ProjectTimelineDto
                {
                    Id = itm.Id,
                    IssueName = itm.Issue.Name,
                    Comment = itm.Comment,
                    Note = itm.Note,
                    //CreatedDate = itm.,
                    //UpdatedDate = itm.UpdatedDate,
                }).ToList();

                return obj;
            }
        }


        public async Task<IEnumerable<ProjectTimelineDto>> GetIssueAnalyses(Guid projectId, Guid issueId, string authorId)
        {
            var retrieveUser = await userConfig.GetUserById(authorId);

            var retrieveIssue = await _dbcontext.Issues
                .Include(prt => prt.Project)
                .Where(itm => itm.Id == issueId && itm.Project.Id == projectId && itm.User == retrieveUser)
                .ToListAsync();

            if (retrieveIssue == null)
            {
                throw new Exception("Issue does not belong to user");
            }
            else
            {
                var retrieveTimeline = await _dbcontext.IssueAnalysers
                    .Include(prt => prt.Project).Include(iss => iss.Issue)
                    .Where(itm => itm.Project.Id == projectId && itm.User == retrieveUser.Id).ToListAsync();

                if (retrieveTimeline.Count < 1)
                {
                    return Enumerable.Empty<ProjectTimelineDto>();
                }

                var obj = retrieveTimeline.Select(itm => new ProjectTimelineDto
                {
                    Id = itm.Id,
                    IssueName = itm.Issue.Name,
                    Comment = itm.Comment,
                    Note = itm.Note,
                    CreatedDate = itm.CreatedDate,
                    UpdatedDate = itm.ModifiedDate,
                }).ToList();

                return obj;
            }
        }

        public async Task<string> UpdateAnalysis(string? note, string? comment, Guid analysisId, string authorId)
        {
            var retrieveUser = await userConfig.GetUserById(authorId);
            var retrieveAnalysis = await _dbcontext.IssueAnalysers.Where(itm => itm.Id == analysisId && itm.User == retrieveUser.Id).SingleOrDefaultAsync();
            if (retrieveAnalysis == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(comment)) { retrieveAnalysis.Comment = comment; }
            if (!string.IsNullOrEmpty(note)) { retrieveAnalysis.Note = note; }

            _dbcontext.IssueAnalysers.Update(retrieveAnalysis);
            await _dbcontext.SaveChangesAsync();

            return "Updated";
        }

    }
}
