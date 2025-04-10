using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Task_Management_System.DTOs.ProjectDto;
using Task_Management_System.Enums;
using Task_Management_System.Models;
using Task_Management_System.Services.ProjectService;

namespace Task_Management_System.Controllers
{
    [Route("api/project")]
    [ApiController]
    [EnableCors]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateProject(CreateDto projectDto)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);

                if (user == null)
                {
                    return NotFound($"User not found");
                }

                return Ok(await _projectService.CreateProject(projectDto, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete]
        [Route("delete/{projectId}")]
        public async Task<IActionResult> DeleteProject(Guid projectId)
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
                    return Ok(await _projectService.DeleteProject(projectId, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("{projectId}")]
        public async Task<IActionResult> GetProjectById(Guid projectId)
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
                    return Ok(await _projectService.GetProjectById(projectId, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetUserProjects([FromQuery] Progress? progress, [FromQuery] Complexity? complexity)
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
                    return Ok(await _projectService.GetProjects(progress, complexity, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("get")]
        [SwaggerOperation(Summary = "Get the list of projects")]
        public async Task<IActionResult> GetPaginated([FromQuery] Progress? progress, [FromQuery] Complexity? complexity, [FromQuery] int? page, [FromQuery] int itemPerPage)
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
                    var projects = await _projectService.GetProjectPaginated(progress, complexity, page, itemPerPage, user.Value);

                    return Ok(projects);
                }

            }

            catch (BadHttpRequestException ex)
            {
                return BadRequest(ex.Message);
            }

            catch (Exception ex)
            {
                var response = new ResponseBody
                {
                    Status = "Error" + ex.Source,
                    Message = ex.Message,
                };

                return StatusCode(500, response);
            }

        }


        [HttpPut]
        [Route("update/{projectId}")]// user parameters instead
        public async Task<IActionResult> UpdateProject(Guid projectId, string? name, string? description, Progress? progress, Complexity? complexity)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);

                if (user == null)
                {
                    return NotFound("user not found");
                }
                else
                {
                    return Ok(await _projectService.UpdateProject(projectId, name, description, progress, complexity, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("edit/{projectId}")]// user parameters instead
        public async Task<IActionResult> EditProject(Guid projectId, UpdateProjectDto dto)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);

                if (user == null)
                {
                    return NotFound("user not found");
                }
                else
                {
                    return Ok(await _projectService.EditProject(projectId, dto, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
