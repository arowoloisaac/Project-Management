using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Task_Management_System.Data;
using Task_Management_System.Models;

namespace Task_Management_System.Services.UserConfiguration
{
    public class UserConfig : IUserConfig
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserConfig(UserManager<User> userManager, RoleManager<Role> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<Role> GetRole(string roleName)
        {
            var getRole = await _roleManager.FindByNameAsync(roleName);

            if (getRole == null)
            {
                throw new InvalidOperationException("Role not valid");
            }

            return getRole;
        }

        public async Task<bool> UserInRole(string roleName, User user)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }


        public async Task<IdentityResult> AddUserToRole(string roleName, User user)
        {
            try
            {
                var addToRole = await _userManager.AddToRoleAsync(user, roleName);
                return addToRole;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to add user to role");
            }
        }


        public async Task<User> GetUser(string mail)
        {
            var user = await _userManager.FindByEmailAsync(mail);

            if (user == null)
            {
                throw new UnauthorizedAccessException("User does not exist");
            }

            return user;
        }

        public async Task<User> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new UnauthorizedAccessException("User does not exist");
            }
            return user;
        }

        public async Task<OrganizationUser> ValidateOrganizationUser(string mail, Guid organizationId, string roleName)
        {
            var getRole = await _roleManager.FindByNameAsync(roleName);

            if (getRole == null)
            {
                throw new ArgumentNullException();
            }

            var user = await GetUser(mail);

            var organization = await _context.Organizations
                .SingleOrDefaultAsync(org => org.Id == organizationId);

            if (organization == null)
            {
                throw new Exception("Organization does not exist");
            }
            else
            {
                var role = await _context.OrganizationUser
                    .Where(u => u.Role == getRole && u.User == user && u.Organization == organization)
                    .SingleOrDefaultAsync();

                if (role == null)
                {
                    throw new Exception("You don't have the ability to perform this action");
                }

                return role;
            }
        }
    }
}
