using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Task_Management_System.Services.TaskAnalyserService;

namespace Task_Management_System.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    //[EnableCors]
    public class TimelineController : ControllerBase
    {
        private readonly ITaskAnalyserService analyser;

        public TimelineController(ITaskAnalyserService service)
        {
            this.analyser = service;
        }

        [HttpGet]
        [Route("project/{projectId}/timeline")]
        public async Task<IActionResult> ProjectTimeline(Guid projectId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(await analyser.GetAnalyses(projectId, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("project={projectId}/issue={issueId}/timeline")]
        public async Task<IActionResult> IssueTimeline(Guid projectId, Guid issueId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(await analyser.GetIssueAnalyses(projectId, issueId, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
