using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Task_Management_System.Configurations;
using Task_Management_System.DTOs.OrganizationDto;
using Task_Management_System.Services.OrganizationService;
using Task_Management_System.Services.UserConfiguration.UserRoleConfiguration;

namespace Task_Management_System.Controllers
{
    [Route("api/organization")]
    [ApiController]
    [EnableCors]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly IUserRoleConfiguration config;

        public OrganizationController(IOrganizationService organizationService, IUserRoleConfiguration configuration)
        {
            _organizationService = organizationService;
            this.config = configuration;
        }

        [HttpPost]
        [Route("create-organization")]
        public async Task<IActionResult> CreateOrganization(CreateOrganizationDto organizationDto)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(user => user.Type == ClaimTypes.Email);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(await _organizationService.CreateOrganization(organizationDto, user.Value));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("{id}/delete")]
        public async Task<IActionResult> DeleteOrganization(Guid id)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(user => user.Type == ClaimTypes.Email);

                if (user == null)
                {
                    return NotFound("User not found");
                }
                else
                {
                    var role = await config.GetOrganizationRoleEmail(user.Value, id);

                    if (role == null || role.Name != ApplicationRoleNames.OrganizationAdministrator)
                    {
                        return Forbid("Access Denied: You are not an administrator");
                    }

                    return Ok(await _organizationService.DeleteOrganization(id, user.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Could not delete {ex.Message}");
            }
        }

        [HttpPut]
        [Route("{organizationId}/update")]
        public async Task<IActionResult> UpdateOrganization(Guid organizationId, [FromQuery] UpdateOrganizationDto dto)
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
                    var role = await config.GetOrganizationRoleEmail(user.Value, organizationId);

                    if (role == null || role.Name != ApplicationRoleNames.OrganizationAdministrator)
                    {
                        return Forbid("Access Denied: You are not an administrator");
                    }

                    return Ok(await _organizationService.UpdateOrganization(organizationId, user.Value, dto));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
