using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Task_Management_System.Configurations;
using Task_Management_System.Enums;
using Task_Management_System.Services.OrganizationProjectService;
using Task_Management_System.Services.UserConfiguration.UserRoleConfiguration;

namespace Task_Management_System.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class GroupProjectController : ControllerBase
    {
        //this controller doesn't really have much with it but an extension of organization controller and service
        private readonly IOrganizationProjectService service;
        private readonly IUserRoleConfiguration userRole;
        public GroupProjectController(IOrganizationProjectService service, IUserRoleConfiguration userRole)
        {
            this.service = service;
            this.userRole = userRole;
        }


        [HttpGet]
        [Route("organization={organizationId}/group={groupId}/get/projects")]
        public async Task<IActionResult> GetProjectPaginated(Progress? progress, Complexity? complexity, int? page, int itemPerPage, Guid organizationId, Guid groupId)
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

                    return Ok(await service.GetProjectPaginated(progress, complexity, page, itemPerPage, organizationId, groupId, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("organization={organizationId}/group={groupId}/projects")]
        public async Task<IActionResult> GetGroupProjects(Guid organizationId, Guid groupId)
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

                    return Ok(await service.GetGroupProjects(organizationId, groupId, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
