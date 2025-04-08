using Microsoft.EntityFrameworkCore;
using Task_Management_System.Data;
using Task_Management_System.Models;

namespace Task_Management_System.Services.UserConfiguration.UserRoleConfiguration
{
    public class UserRoleConfiguration : IUserRoleConfiguration
    {
        private readonly ApplicationDbContext context;

        public UserRoleConfiguration(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Role?> GetOrganizationRole(Guid userId, Guid organizationId)
        {
            var retrieveOrg = await context.Organizations.FindAsync(organizationId);

            if (retrieveOrg == null)
            {
                throw new Exception("organization does not exist");
            }
            var role = await context.OrganizationUser
                         .Include(uor => uor.Role)
                         .Where(uor => uor.User.Id == userId && uor.Organization.Id == organizationId)
                         .Select(uor => uor.Role)
                         .FirstOrDefaultAsync();

            return role;
        }

        public async Task<Role> GetGroupRole(Guid userId, Guid organizationId, Guid groupId)
        {
            var retrieveOrg = await context.Organizations.FindAsync(organizationId);

            if (retrieveOrg == null)
            {
                throw new Exception("organization does not exist");
            }

            var role = await context.GroupUsers
                         .Include(uor => uor.Role)
                         .Where(uor => uor.User.Id == userId && uor.Group.OrganizationId == organizationId && uor.Group.Id == groupId)
                         .Select(uor => uor.Role)
                         .FirstOrDefaultAsync();

            return role;
        }


        public async Task<IEnumerable<Role>> GetUserRoles(Guid userId, Guid organizationId, Guid? groupId)
        {
            var roles = new List<Role>();

            if (groupId.HasValue)
            {
                var groupRole = await GetGroupRole(userId, organizationId, groupId.Value);
                if (groupRole != null) roles.Add(groupRole);
            }
            var orgRole = await GetOrganizationRole(userId, organizationId);
            if (orgRole != null) roles.Add(orgRole);

            return roles;
        }

        public async Task<Role?> GetOrganizationRoleEmail(string userMail, Guid organizationId)
        {
            var retrieveOrg = await context.Organizations.FindAsync(organizationId);

            if (retrieveOrg == null)
            {
                throw new Exception("organization does not exist");
            }
            var role = await context.OrganizationUser
                         .Include(uor => uor.Role)
                         .Where(uor => uor.User.Email == userMail && uor.Organization.Id == organizationId)
                         .Select(uor => uor.Role)
                         .FirstOrDefaultAsync();

            return role;
        }

        public async Task<Role?> GetGroupRoleByEmail(string userMail, Guid organizationId, Guid groupId)
        {
            var retrieveOrg = await context.Organizations.FindAsync(organizationId);

            if (retrieveOrg == null)
            {
                throw new Exception("organization does not exist");
            }

            var role = await context.GroupUsers
                         .Include(uor => uor.Role)
                         .Where(uor => uor.User.Email == userMail && uor.Group.OrganizationId == organizationId && uor.Group.Id == groupId)
                         .Select(uor => uor.Role)
                         .FirstOrDefaultAsync();

            return role;
        }
    }
}
