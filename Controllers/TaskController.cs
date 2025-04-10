using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Task_Management_System.DTOs.IssueDto;
using Task_Management_System.Enums;
using Task_Management_System.Services.TaskService;

namespace Task_Management_System.Controllers
{
    [Route("api/")]
    [ApiController]
    [EnableCors]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService service;

        public TaskController(ITaskService taskService)
        {
            this.service = taskService;
        }


        [HttpPost]
        [Route("project={projectId}/create-issue")]
        public async Task<IActionResult> CreateIssue([Required] Guid projectId, CreateIssue createIssue)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                else
                {
                    return Ok(await service.CreateIssues(projectId, createIssue, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("project={projectId}/issue={parentIssueId}/create-subIssue")]
        public async Task<IActionResult> CreateSubIssue(Guid projectId, CreateIssue issueDto, Guid parentIssueId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                return Ok(await service.CreateSubIssue(projectId, issueDto, parentIssueId, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //add the project id later
        [HttpDelete]
        [Route("project={projectId}/issue={issueId}/delete")]
        public async Task<IActionResult> DeleteIssue(Guid issueId, Guid projectId, [Required] bool isDeleteChildren)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                return Ok(await service.DeleteIssues(issueId, projectId, isDeleteChildren, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //same here
        [HttpPut]
        [Route("project={projectId}/issue={issueId}/update")]
        public async Task<IActionResult> UpdateIssue(UpdateIssueDto dto, Guid projectId, Guid issueId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                return Ok(await service.UpdateIssues(issueId, dto, projectId, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("projectId={projectId}/issues")]
        public async Task<IActionResult> GetProjectIssues(Guid projectId, [FromQuery] IssueType? issueType, [FromQuery] Complexity? complexity, [FromQuery] Progress? progress)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
                if (user == null)
                {
                    return NotFound("user not found");
                }
                return Ok(await service.GetIssues(issueType, complexity, progress, projectId, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("projectId={projectId}/issues/page")]
        public async Task<IActionResult> GetProjectIssuesPaginated(Guid projectId, [FromQuery] IssueType? issueType, [FromQuery] Complexity? complexity, [FromQuery] Progress? progress, int? page, int itemPerPage)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
                if (user == null)
                {
                    return NotFound("user not found");
                }
                return Ok(await service.GetIssuesPaginated(issueType, complexity, progress, page, itemPerPage, projectId, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("project={projectId}/default")]
        public async Task<IActionResult> GetProjectIssue(Guid projectId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);

                if (user == null)
                {
                    throw new Exception("User does not exist");
                }

                return Ok(await service.GetIssue(projectId, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("project={projectId}/issue={issueId}")]
        public async Task<IActionResult> GetIssueById(Guid projectId, Guid issueId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                return Ok(await service.GetIssueById(projectId, issueId, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("parentId={parentIssueId}")]
        public async Task<IActionResult> GetSubIssues(Guid projectId, Guid parentIssueId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(await service.GetSubIssues(parentIssueId, projectId, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("project={projectId}/issues")]
        public async Task<IActionResult> GetIssueAndChild(Guid projectId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(await service.GetIssueAndChild(projectId, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("project={projectId}/origin={originId}/related={stateId}")]
        public async Task<IActionResult> AddRelatedIssue(Guid originId, Guid stateId, Guid projectId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                await service.AddRelatedIssue(originId, stateId, projectId, user.Value);

                return Ok("Successful");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("project={projectId}/origin={originId}/related")]
        public async Task<IActionResult> GetRelatedIssues(Guid originId, Guid projectId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(await service.GetRelatedIssues(originId, projectId, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("project={projectId}/origin={originId}/remove")]
        public async Task<IActionResult> RemoveRelatedIssue(Guid originId, Guid stateId, Guid projectId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                await service.RemoveRelatedIssue(originId, stateId, projectId, user.Value);
                return Ok("successfully removed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
