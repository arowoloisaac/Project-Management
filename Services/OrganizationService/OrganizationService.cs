using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Task_Management_System.Configurations;
using Task_Management_System.Data;
using Task_Management_System.DTOs.OrganizationDto;
using Task_Management_System.Enums;
using Task_Management_System.Models;
using Task_Management_System.Services.UserConfiguration;

namespace Task_Management_System.Services.OrganizationService
{
    public class OrganizationService : IOrganizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IUserConfig _userConfig;
        private readonly RoleManager<Role> _roleManager;

        public string role = ApplicationRoleNames.OrganizationAdministrator;

        public OrganizationService(ApplicationDbContext context, UserManager<User> userManager, IUserConfig userConfig, RoleManager<Role> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _userConfig = userConfig;
            _roleManager = roleManager;
        }

        public async Task<string> CreateOrganization(CreateOrganizationDto dto, string mail)
        {
            var user = await _userConfig.GetUser(mail);

            string organizationName = dto.Name;

            if (string.IsNullOrEmpty(organizationName))
            {
                throw new Exception("The name field needs a value");
            }

            var organization = await _context.Organizations.Where(org => org.Name == organizationName && org.CreatedBy == user.Id).SingleOrDefaultAsync();

            if (organization != null)
            {
                throw new Exception("This organization already exist");
            }
            else
            {
                var addOrganization = new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = organizationName,
                    Description = dto.Description,
                    CreatedTime = DateTime.UtcNow,
                    CreatedBy = user.Id,
                    Filter = OrganizationFilter.Owned
                };

                var response = await _context.Organizations.AddAsync(addOrganization);

                var getRole = await _roleManager.FindByNameAsync(role);
                if (getRole != null)
                {
                    var checkRole = await _userManager.IsInRoleAsync(user, role);
                    if (checkRole == false)
                    {
                        var addToRole = await _userManager.AddToRoleAsync(user, role);
                        if (!addToRole.Succeeded)
                        {
                            throw new Exception("Unable to add user to role");
                        }

                        await _context.OrganizationUser.AddAsync(new OrganizationUser
                        {
                            Id = Guid.NewGuid(),
                            Role = getRole,
                            User = user,
                            Organization = addOrganization
                        });
                    }
                    else
                    {
                        await _context.OrganizationUser.AddAsync(new OrganizationUser
                        {
                            Id = Guid.NewGuid(),
                            Role = getRole,
                            User = user,
                            Organization = addOrganization
                        });
                    }
                }
                await _context.SaveChangesAsync();

                return addOrganization.Id.ToString();
            }
        }

        public async Task<string> DeleteOrganization(Guid organizationId, string mail)
        {
            var user = await _userConfig.GetUser(mail);
            var orgAdmin = await _userConfig.ValidateOrganizationUser(mail, organizationId, role);

            var valToRemoveOrg = await _context.OrganizationUser
                .Include(org => org.Organization)
                .Include(org => org.User)
                .Include(org => org.Role)
                .Where(org => org.Organization.Id == organizationId).ToListAsync();

            if (valToRemoveOrg == null)
            {
                throw new Exception("Either permssion or invalid Inputs");
            }
            else
            {
                var getOrg = await _context.Organizations.
                    Include(grp => grp.Groups)
                        .ThenInclude(gp => gp.Projects)
                    .Include(prj => prj.Projects)
                    .Include(ou => ou.Users)
                    .Include(req => req.Requests)
                    .SingleOrDefaultAsync(o => o.Id == organizationId);

                if (getOrg == null)
                {
                    throw new Exception("Doesn't exist in the database");
                }

                if (getOrg.Projects != null) _context.Projects.RemoveRange(getOrg.Projects);

                if (getOrg.Groups != null)
                {
                    foreach (var group in getOrg.Groups) {
                        if (group.Projects != null) _context.Projects.RemoveRange(group.Projects);
                        _context.Groups.Remove(group);
                    }
                }

                _context.OrganizationUser.RemoveRange(valToRemoveOrg);

                if (getOrg.Users != null)
                    _context.OrganizationUser.RemoveRange(getOrg.Users);

                // Delete related Requests
                if (getOrg.Requests != null)
                    _context.Requests.RemoveRange(getOrg.Requests);

                // Finally, delete the organization itself
                _context.Organizations.Remove(getOrg);

                await _context.SaveChangesAsync();
                return "Delete successfully";
            }
        }

        public async Task<string> UpdateOrganization(Guid organizationId, string mail, UpdateOrganizationDto dto)
        {
            var admin = await _userConfig.GetUser(mail);

            var validateOrg = await _context.Organizations.FindAsync(organizationId);

            if (validateOrg == null)
            {
                throw new Exception("Organzaition doesn't exist");
            }
            else
            {
                if (!string.IsNullOrEmpty(dto.Name))
                {
                    validateOrg.Name = dto.Name;
                }

                if (!string.IsNullOrEmpty(dto.Description))
                {
                    validateOrg.Description = dto.Description;
                }

                var saveUpdate = _context.Organizations.Update(validateOrg);
                await _context.SaveChangesAsync();

                return "Organization updated";
            }
        }
    }
}
