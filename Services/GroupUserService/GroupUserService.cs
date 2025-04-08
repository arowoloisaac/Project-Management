using Microsoft.EntityFrameworkCore;
using Task_Management_System.Data;
using Task_Management_System.DTOs.OrganizationDto;
using Task_Management_System.Models;
using Task_Management_System.Services.UserConfiguration;

namespace Task_Management_System.Services.GroupUserService
{
    public class GroupUserService : IGroupUserService
    {
        private readonly ApplicationDbContext context;
        private readonly IUserConfig config;

        public GroupUserService(ApplicationDbContext context, IUserConfig config)
        {
            this.context = context;
            this.config = config;
        }

        public async Task<string> AddUserToGroup(Guid organizationId, Guid groupId, string userEmail, string roleName)
        {
            var getOrgGroup = await context.Groups
                .Where(grp => grp.Id == groupId && grp.OrganizationId == organizationId)
                .SingleOrDefaultAsync();

            if (getOrgGroup == null)
            {
                throw new Exception("Group does not exist");
            }

            // Fetch user first
            var getUser = await config.GetUser(userEmail);
            if (getUser == null)
            {
                throw new Exception("User does not exist");
            }

            // Ensure the user exists in the organization
            var checkIfUserExistInOrg = await context.OrganizationUser
                .Where(ou => ou.User.Id == getUser.Id && ou.Organization.Id == organizationId)
                .SingleOrDefaultAsync();

            if (checkIfUserExistInOrg == null)
            {
                throw new Exception("User does not exist in organization");
            }

            // Check if user already exists in the group
            var getIfUserExistInGroup = await context.GroupUsers
                .Where(gu => gu.Group.Id == groupId && gu.User.Id == getUser.Id)
                .SingleOrDefaultAsync();

            if (getIfUserExistInGroup != null)
            {
                throw new Exception("User already exists in the group");
            }

            // Fetch the role
            var getRole = await config.GetRole(roleName);
            if (getRole == null)
            {
                throw new Exception("Role does not exist");
            }

            // Check if user is already in role
            var checkIfUserExistInRole = await config.UserInRole(roleName, getUser);

            if (!checkIfUserExistInRole)
            {
                var addToRole = await config.AddUserToRole(roleName, getUser);
                if (!addToRole.Succeeded)
                {
                    throw new Exception($"Unable to add user to role {roleName}");
                }
            }

            // Add user to group
            var addUserToGroup = new GroupUser
            {
                Id = Guid.NewGuid(),
                User = getUser,
                Group = getOrgGroup,
                Role = getRole
            };

            await context.GroupUsers.AddAsync(addUserToGroup);
            await context.SaveChangesAsync();

            return "User added to group";
        }

        /*public async Task<string> AddUserToGroup(Guid organizationId, Guid groupId, string userEmail, string roleName)
        {
            var getOrgGroup = await context.Groups
                .Where(grp => grp.Id == groupId && grp.OrganizationId == organizationId).SingleOrDefaultAsync();

            var getRole = await config.GetRole(roleName);

            if (getOrgGroup == null)
            {
                throw new Exception("Group does not exist");
            }
            else
            {
                var getUser = await config.GetUser(userEmail);

                var checkIfUserExistInOrg = await context.OrganizationUser
                    .Where(ou => ou.User.Email == userEmail && ou.Organization.Id == organizationId).SingleOrDefaultAsync();

                if (checkIfUserExistInOrg == null)
                {
                    throw new Exception("User does not exist in organization");
                }

                else
                {
                    var getIfUserExistInGroup = await context.GroupUsers
                    .Where(gu => gu.Group.Id == groupId && gu.User.Email == userEmail).SingleOrDefaultAsync();

                    if (getIfUserExistInGroup != null)
                    {
                        throw new Exception("User already exist in the group");
                    }
                    else
                    {
                        var checkIfUserExistInRole = await config.UserInRole(roleName, getUser);

                        GroupUser addUserToGroup;

                        if (checkIfUserExistInRole == true)
                        {
                            addUserToGroup = new GroupUser
                            {
                                Group = getOrgGroup,
                                Id = Guid.NewGuid(),
                                User = getUser,
                                Role = getRole
                            };

                            await context.GroupUsers.AddAsync(addUserToGroup);
                        }
                        else
                        {
                            var addToRole = await config.AddUserToRole(roleName, getUser);

                            if (addToRole.Succeeded)
                            {
                                addUserToGroup = new GroupUser
                                {
                                    Id = Guid.NewGuid(),
                                    User = getUser,
                                    Group = getOrgGroup,
                                    Role = getRole,
                                };
                                await context.GroupUsers.AddAsync(addUserToGroup);
                            }
                            else
                            {
                                throw new Exception("Unable to add user to the role" + roleName);
                            }
                            
                        }
                        await context.SaveChangesAsync();

                        return "User added to group";
                    }
                }
                
            }
        }*/

        public async Task<string> RemoveUserFromGroup(Guid organizationId, Guid groupId, string userEmail)
        {
            var getOrgGroup = await context.Groups
                .Where(grp => grp.Id == groupId && grp.OrganizationId == organizationId).SingleOrDefaultAsync();

            if (getOrgGroup == null)
            {
                throw new Exception("Group does not exist");
            }
            else
            {
                var getUser = await config.GetUser(userEmail);

                var getIfUserExistInGroup = await context.GroupUsers
                    .Where(gu => gu.Group.Id == groupId && gu.User.Email == userEmail).SingleOrDefaultAsync();

                if (getIfUserExistInGroup == null)
                {
                    throw new Exception("User does not exist in the group");
                }
                else
                {
                    context.GroupUsers.Remove(getIfUserExistInGroup);

                    await context.SaveChangesAsync();
                    return (getUser.LastName + " Removed from group");
                }
            }
        }

        public async Task<IEnumerable<GroupUserDto>> RetrieveGroupUsers(Guid organizationId, Guid groupId)
        {
            var getOrgGroup = await context.Groups
                 .Where(grp => grp.Id == groupId && grp.OrganizationId == organizationId).SingleOrDefaultAsync();

            if (getOrgGroup == null)
            {
                throw new Exception("Group does not exist");
            }
            else
            {
                var getGroupUsers = await context.GroupUsers
                    .Include(user => user.User)
                    .Include(role => role.Role)
                    .Where(gu => gu.Group == getOrgGroup).ToListAsync();

                if (getGroupUsers == null)
                {
                    return new List<GroupUserDto>();
                }
                else
                {
                    var response = getGroupUsers.Select(user => new GroupUserDto
                    {
                        Id = user.User.Id,
                        Name = user.User.FirstName,
                        Email = user.User.Email,
                        Role = user.Role.Name
                    }).ToList();

                    return response;
                }
            }
        }

        public async Task<GroupUserDto> RetrieveUser(Guid organizationId, Guid groupId, string userId)
        {
            var getUser = await config.GetUserById(userId);

            var getOrgGroup = await context.Groups
                 .Where(grp => grp.Id == groupId && grp.OrganizationId == organizationId).SingleOrDefaultAsync();

            if (getOrgGroup == null)
            {
                throw new Exception("Group does not exist");
            }
            else
            {
                var getGroupUsers = await context.GroupUsers
                    .Include(user => user.User)
                    .Include(role => role.Role)
                    .Where(gu => gu.Group == getOrgGroup && gu.User == getUser).SingleOrDefaultAsync();
                if (getGroupUsers == null)
                {
                    return new GroupUserDto();
                }
                else
                {
                    var response = new GroupUserDto
                    {
                        Id = getGroupUsers.User.Id,
                        Name = getGroupUsers.User.FirstName,
                        Email = getGroupUsers.User.Email,
                        Role = getGroupUsers.Role.Name
                    };

                    return response;
                }
            }
        }

        public async Task<string> UpdateUserRole(Guid organizationId, Guid groupId, string roleName, string userToUpdate)
        {
            var getOrgGroup = await context.Groups
                .Where(grp => grp.Id == groupId && grp.OrganizationId == organizationId).SingleOrDefaultAsync();

            var getRole = await config.GetRole(roleName);


            if (getOrgGroup == null)
            {
                throw new Exception("Group does not exist");
            }
            else
            {
                var getUser = await config.GetUser(userToUpdate);

                var checkIfUserExistInOrg = await context.OrganizationUser
                    .Where(ou => ou.User == getUser && ou.Organization.Id == organizationId).SingleOrDefaultAsync();

                if (checkIfUserExistInOrg == null)
                {
                    throw new Exception("User does not exist in organization");
                }

                else
                {
                    var getIfUserExistInGroup = await context.GroupUsers
                    .Where(gu => gu.Group.Id == groupId && gu.User == getUser).SingleOrDefaultAsync();

                    if (getIfUserExistInGroup == null)
                    {
                        throw new Exception("User does not exist in the group");
                    }
                    else
                    {
                        var checkIfUserExistInRole = await config.UserInRole(roleName, getUser);

                        getIfUserExistInGroup.Role = getRole;

                        context.GroupUsers.Update(getIfUserExistInGroup);

                        await context.SaveChangesAsync();

                        return "User added to group";
                    }
                }
            }

        }
    }
}
