using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Task_Management_System.Configurations;
using Task_Management_System.Services.GroupUserService;
using Task_Management_System.Services.UserConfiguration.UserRoleConfiguration;

namespace Task_Management_System.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class GroupUserController : ControllerBase
    {
        private readonly IUserRoleConfiguration configuration;
        private readonly IGroupUserService groupUser;

        public GroupUserController(IUserRoleConfiguration configuration, IGroupUserService groupUser)
        {
            this.configuration = configuration;
            this.groupUser = groupUser;
        }

        [HttpPost]
        [Route("organization={organizationId}/group={groupId}/add-user")]
        public async Task<IActionResult> AddUserToGroup(Guid organizationId, Guid groupId, string userEmail, string roleName)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return Unauthorized("User not found");
                }
                else
                {
                    var role = await configuration.GetOrganizationRole(Guid.Parse(user.Value), organizationId);

                    if (role == null || role.Name != ApplicationRoleNames.OrganizationAdministrator)
                    {
                        return Forbid("Access Denied: You are not an administrator in the organization");
                    }

                    return Ok(await groupUser.AddUserToGroup(organizationId, groupId, userEmail, roleName));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("organization={organizationId}/group={groupId}/remove-user={userEmail}")]
        public async Task<IActionResult> RemoveUserFromGroup(Guid organizationId, Guid groupId, string userEmail)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return Unauthorized("User not found");
                }
                else
                {
                    var role = await configuration.GetUserRoles(Guid.Parse(user.Value), organizationId, groupId);

                    if (role == null || !role.Any(r => r.Name == ApplicationRoleNames.OrganizationAdministrator || r.Name == ApplicationRoleNames.GroupAdministrator))
                    {
                        return Forbid("Access Denied: You are not an administrator");
                    }

                    return Ok(await groupUser.RemoveUserFromGroup(organizationId, groupId, userEmail));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("organization/{organizationId}/group/{groupId}/user")]
        public async Task<IActionResult> RetrieveUser(Guid organizationId, Guid groupId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return Unauthorized("User not found");
                }


                var role = await configuration.GetUserRoles(Guid.Parse(user.Value), organizationId, null);

                if (role == null || role.Any(r => r.Name == ApplicationRoleNames.OrganizationMember))
                {
                    return Forbid("Access Denied: You do not belong to the organization");
                }

                return Ok(await groupUser.RetrieveUser(organizationId, groupId, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("organization={organizationId}/group={groupId}/users/admin")]
        public async Task<IActionResult> RetrieveAdminGroupUsers(Guid organizationId, Guid groupId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return Unauthorized("User not found");
                }


                var role = await configuration.GetUserRoles(Guid.Parse(user.Value), organizationId, null);

                if (role == null || role.Any(r => r.Name == ApplicationRoleNames.OrganizationMember))
                {
                    return Forbid("Access Denied: You do not belong to the organization");
                }

                return Ok(await groupUser.RetrieveGroupUsers(organizationId, groupId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("organization={organizationId}/group={groupId}/users")]
        public async Task<IActionResult> RetrieveGroupUsers(Guid organizationId, Guid groupId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return Unauthorized("User not found");
                }


                var role = await configuration.GetUserRoles(Guid.Parse(user.Value), organizationId, groupId);

                if (role == null || !role.Any(ro => ro.Name == ApplicationRoleNames.OrganizationAdministrator ||
                        ro.Name == ApplicationRoleNames.GroupAdministrator || ro.Name == ApplicationRoleNames.GroupUser))
                {
                    return Forbid("Access Denied: You do not belong to the group");
                }

                return Ok(await groupUser.RetrieveGroupUsers(organizationId, groupId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut]
        [Route("organization={organizationId}/group={groupId}/user={userToUpdate}/update")]
        public async Task<IActionResult> UpdateUserRole(Guid organizationId, Guid groupId, string roleName, string userToUpdate)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (user == null)
                {
                    return Unauthorized("User not found");
                }
                else
                {
                    var role = await configuration.GetUserRoles(Guid.Parse(user.Value), organizationId, groupId);

                    if (role == null || !role.Any(r => r.Name == ApplicationRoleNames.OrganizationAdministrator || r.Name == ApplicationRoleNames.GroupAdministrator))
                    {
                        return Forbid("Access Denied: You are not an administrator");
                    }

                    return Ok(await groupUser.UpdateUserRole(organizationId, groupId, roleName, userToUpdate));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
