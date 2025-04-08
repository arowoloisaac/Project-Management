using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Task_Management_System.Configurations;
using Task_Management_System.Data;
using Task_Management_System.Models;

namespace Task_Management_System.Services.Configurations
{
    public static class ConfigureIdentity
    {
        public static async Task ConfigureIdentityAsync(this WebApplication app)
        {
            using var serviceScope = app.Services.CreateScope();

            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            string[] roles = new string[]
            {
                ApplicationRoleNames.OrganizationAdministrator,
                ApplicationRoleNames.OrganizationCurator,
                ApplicationRoleNames.OrganizationMember,
                ApplicationRoleNames.GroupAdministrator,
                ApplicationRoleNames.GroupUser,
                ApplicationRoleNames.GroupModerator
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = role,
                    });
                }
            }
        }

        public static async Task ConfigureDefaultAvatar(this WebApplication app)
        {
            using var serviceScope = app.Services.CreateScope();

            var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            string avatarUrl = "https://windsorchapel.org/wp-content/uploads/2015/05/blank_avatar_male1-e1452782680848.jpg";

            var avatarExist = await dbContext.Avatars.FirstOrDefaultAsync(url => url.AvatarUrl == avatarUrl);

            if (avatarExist != null)
            {
                return;
            }

            var newAvatar = new Avatar
            {
                Id = Guid.NewGuid(),
                AvatarUrl = avatarUrl,
            };

            await dbContext.Avatars.AddAsync(newAvatar);

            await dbContext.SaveChangesAsync();
        }
    }
}
