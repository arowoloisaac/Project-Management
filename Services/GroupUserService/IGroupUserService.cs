using Task_Management_System.DTOs.OrganizationDto;

namespace Task_Management_System.Services.GroupUserService
{
    public interface IGroupUserService
    {
        Task<string> AddUserToGroup(Guid organizationId, Guid groupId, string userEmail, string roleName);

        Task<string> RemoveUserFromGroup(Guid organizationId, Guid groupId, string userEmail);

        Task<IEnumerable<GroupUserDto>> RetrieveGroupUsers(Guid organizationId, Guid groupId);

        Task<string> UpdateUserRole(Guid organizationId, Guid groupId, string roleName, string userToUpdate);

        Task<GroupUserDto> RetrieveUser(Guid organizationId, Guid groupId, string userId);
    }
}
