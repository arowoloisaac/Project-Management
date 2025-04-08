using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Task_Management_System.Configurations;
using Task_Management_System.Data;
using Task_Management_System.DTOs.GroupDto.OrganizationGrpDto;
using Task_Management_System.Models;
using Task_Management_System.Services.UserConfiguration;

namespace Task_Management_System.Services.OrganizationGroupService
{
    public class OrganizationGroupService : IOrganizationGroupService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserConfig _userConfig;

        public const string admin = ApplicationRoleNames.OrganizationAdministrator;

        public OrganizationGroupService(ApplicationDbContext context, UserManager<User> userManager,
            RoleManager<Role> roleManager, IUserConfig userConfig)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _userConfig = userConfig;
        }


        public async Task<string> CreateOrganizationGroup(string groupName, Guid organizationId, string mail)
        {
            var organizationAdmin = await _userConfig.ValidateOrganizationUser(mail, organizationId, admin);

            var groupExists = await _context.Groups
                .AnyAsync(grp => grp.Name == groupName && grp.OrganizationId == organizationAdmin.Organization.Id);

            if (groupExists)
            {
                throw new Exception("Group with this name already exists");
            }

            var newGroup = new Group
            {
                Id = Guid.NewGuid(),
                Name = groupName,
                OrganizationId = organizationAdmin.Organization.Id,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = organizationAdmin.User
            };

            await _context.Groups.AddAsync(newGroup);

            await _context.SaveChangesAsync();

            return "Group created";
        }

        public async Task<string> DeleteOrganizationGroup(Guid groupId, Guid organizationId, string mail)
        {
            var organizationAdmin = await _userConfig.ValidateOrganizationUser(mail, organizationId, admin);

            var getGroup = await _context.Groups.Where(grp => grp.Id == groupId).SingleOrDefaultAsync();

            if (getGroup != null)
            {
                var getGroupUser = await _context.GroupUsers
                    .Include(ur => ur.User)
                    .Include(gp => gp.Group)
                    .Include(ro => ro.Role)
                    .Where(grp => grp.Group.Id == getGroup.Id).ToListAsync();

                if (getGroupUser == null)
                {
                    _context.Groups.Remove(getGroup);
                }
                else
                {
                    _context.GroupUsers.RemoveRange(getGroupUser);
                    _context.Groups.Remove(getGroup);
                }
                await _context.SaveChangesAsync();
                return "Organization deleted successfully";
            }

            throw new Exception("group does not exist");
        }

        public async Task<RetrieveGroupDto> RetrieveOrganizationGroupById(Guid groupId, Guid organizationId, string mail)
        {
            var organizationAdmin = await _userConfig.ValidateOrganizationUser(mail, organizationId, admin);

            var getOrganization = await _context.Groups.SingleOrDefaultAsync(grp => grp.Id == groupId && grp.OrganizationId == organizationId);

            if (getOrganization != null)
            {
                var response = new RetrieveGroupDto
                {
                    Id = getOrganization.Id,
                    Name = getOrganization.Name,
                    Description = getOrganization.Description,
                };
                return response;
            }

            throw new ArgumentNullException($"Group with Id - {groupId} not found");
        }


        public async Task<IEnumerable<RetrieveGroupDto>> RetrieveOrganizationGroup(string adminMail, Guid organizationId)
        {
            var organizationAdmin = await _userConfig.ValidateOrganizationUser(adminMail, organizationId, admin);

            var validateGroup = await _context.Groups
                .Include(grpProject => grpProject.Projects)
                .Where(grp => grp.OrganizationId == organizationId).ToListAsync();

            if (validateGroup == null)
                return new List<RetrieveGroupDto>();

            else
            {
                var response = validateGroup.Select(grp => new RetrieveGroupDto
                {
                    Id = grp.Id,
                    Name = grp.Name,
                    Description = grp.Description,
                    ProjectCount = grp.Projects.Count()
                }).ToList();

                return response;
            }
        }


        public async Task<string> UpdateOrganizationGroup(Guid groupId, string? groupName, Guid organizationId, string adminEmail)
        {
            var organizationAdmin = await _userConfig.ValidateOrganizationUser(adminEmail, organizationId, admin);

            var validateGroup = await _context.Groups
                .Where(grp => grp.OrganizationId == organizationId && grp.Id == groupId)
                .SingleOrDefaultAsync();

            if (validateGroup == null)
            {
                throw new Exception("Group does not exist");
            }
            else
            {
                if (!string.IsNullOrEmpty(groupName))
                {
                    validateGroup.Name = groupName;
                }
                validateGroup.UpdatedDate = DateTime.UtcNow;
                validateGroup.UpdatedBy = organizationAdmin.User;

                _context.Groups.Update(validateGroup);
                await _context.SaveChangesAsync();
                return "Group updated";
            }
        }
    }
}
