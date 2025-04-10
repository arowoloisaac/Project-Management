using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Task_Management_System.Configurations;
using Task_Management_System.DTOs.ProjectDto;
using Task_Management_System.Enums;
using Task_Management_System.Services.OrganizationProjectService;
using Task_Management_System.Services.UserConfiguration.UserRoleConfiguration;

namespace Task_Management_System.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class OrganizationProjectController : ControllerBase
    {
        private readonly IOrganizationProjectService service;
        private readonly IUserRoleConfiguration userRole;

        public OrganizationProjectController(IOrganizationProjectService service, IUserRoleConfiguration userRole)
        {
            this.service = service;
            this.userRole = userRole;
        }

        [HttpPost]
        [Route("organization/{organizationId}/create-project")]
        public async Task<IActionResult> CreateProject(CreateDto dto, Guid organizationId, Guid? groupId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return NotFound("User not found");
                }
                else
                {
                    var role = await userRole.GetOrganizationRole(Guid.Parse(user.Value), organizationId);

                    if (role == null || role.Name != ApplicationRoleNames.OrganizationAdministrator)
                    {
                        return Forbid("Access Denied: You are not an administrator in the organization");
                    }

                    return Ok(await service.CreateProject(dto, organizationId, groupId, user.Value));
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("organization={organizationId}/group={groupId}/assign/project={projectId}")]
        public async Task<IActionResult> AssignProjectToGroup(Guid organizationId, Guid groupId, Guid projectId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return NotFound("User not found");
                }
                else
                {
                    var role = await userRole.GetOrganizationRole(Guid.Parse(user.Value), organizationId);

                    if (role == null || role.Name != ApplicationRoleNames.OrganizationAdministrator)
                    {
                        return Forbid("Access Denied: You are not an administrator in the organization");
                    }

                    return Ok(await service.AssignProjectToGroup(organizationId, groupId, projectId, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("organization={organizationId}/group={groupId}/unassign/project={projectId}")]
        public async Task<IActionResult> UnassignProjectToGroup(Guid organizationId, Guid groupId, Guid projectId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return NotFound("User not found");
                }
                else
                {
                    var role = await userRole.GetOrganizationRole(Guid.Parse(user.Value), organizationId);

                    if (role == null || role.Name != ApplicationRoleNames.OrganizationAdministrator)
                    {
                        return Forbid("Access Denied: You are not an administrator in the organization");
                    }

                    return Ok(await service.UnassignProjectToGroup(organizationId, groupId, projectId));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("organization={organizationId}/update/project={projectId}")]
        public async Task<IActionResult> UpdateProject(Guid projectId, string? name, string? description, Progress? progress, Complexity? complexity, Guid organizationId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return NotFound("User not found");
                }
                else
                {
                    var role = await userRole.GetOrganizationRole(Guid.Parse(user.Value), organizationId);

                    if (role == null || role.Name != ApplicationRoleNames.OrganizationAdministrator)
                    {
                        return Forbid("Access Denied: You are not an administrator in the organization");
                    }

                    return Ok(await service.UpdateProject(projectId, name, description, progress, complexity, organizationId, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("organization={organizationId}/delete/project={projectId}")]
        public async Task<IActionResult> DeleteProject(Guid projectId, Guid organizationId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return NotFound("User not found");
                }
                else
                {
                    var role = await userRole.GetOrganizationRole(Guid.Parse(user.Value), organizationId);

                    if (role == null || role.Name != ApplicationRoleNames.OrganizationAdministrator)
                    {
                        return Forbid("Access Denied: You are not an administrator in the organization");
                    }

                    return Ok(await service.DeleteProject(projectId, organizationId, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("organization={organizationId}/edit/project={projectId}")]
        public async Task<IActionResult> EditProject(Guid projectId, UpdateProjectDto dto, Guid organizationId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return NotFound("User not found");
                }
                else
                {
                    var role = await userRole.GetOrganizationRole(Guid.Parse(user.Value), organizationId);

                    if (role == null || role.Name != ApplicationRoleNames.OrganizationAdministrator)
                    {
                        return Forbid("Access Denied: You are not an administrator in the organization");
                    }

                    return Ok(await service.EditProject(projectId, dto, organizationId, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("organization={organizationId}/get/project={projectId}")]
        public async Task<IActionResult> GetProjectById(Guid projectId, Guid organizationId, Guid? groupId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return NotFound("User not found");
                }
                else
                {
                    var role = await userRole.GetUserRoles(Guid.Parse(user.Value), organizationId, groupId);

                    if (role == null ||
                        !role.Any(ro => ro.Name == ApplicationRoleNames.OrganizationAdministrator ||
                        ro.Name == ApplicationRoleNames.GroupAdministrator || ro.Name == ApplicationRoleNames.GroupUser))
                    {
                        return Forbid("Access Denied: You are not an administrator in the organization");
                    }

                    return Ok(await service.GetProjectById(projectId, user.Value, organizationId, groupId));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //for the admin page
        [HttpGet]
        [Route("organization={organizationId}/get/projects")]
        public async Task<IActionResult> GetProjects([FromQuery] Progress? progress, Complexity? complexity, bool? isAssigned, Guid organizationId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return NotFound("User not found");
                }
                else
                {
                    var role = await userRole.GetOrganizationRole(Guid.Parse(user.Value), organizationId);

                    if (role == null || role.Name != ApplicationRoleNames.OrganizationAdministrator)
                    {
                        return Forbid("Access Denied: You are not an administrator in the organization");
                    }

                    return Ok(await service.GetProjects(progress, complexity, isAssigned, organizationId, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
