using Microsoft.AspNetCore.Identity;
using Task_Management_System.Models;

namespace Task_Management_System.Services.UserConfiguration
{
    public interface IUserConfig
    {
        Task<User> GetUser(string mail);

        Task<User> GetUserById(string id);

        Task<OrganizationUser> ValidateOrganizationUser(string mail, Guid organizationId, string roleName);

        Task<Role> GetRole(string roleName);

        Task<bool> UserInRole(string roleName, User user);

        Task<IdentityResult> AddUserToRole(string roleName, User user);
    }
}
