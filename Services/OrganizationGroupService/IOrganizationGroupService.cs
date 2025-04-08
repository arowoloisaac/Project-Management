using Task_Management_System.DTOs.GroupDto.OrganizationGrpDto;

namespace Task_Management_System.Services.OrganizationGroupService
{
    public interface IOrganizationGroupService
    {
        Task<string> CreateOrganizationGroup(string groupName, Guid organizationId, string mail);

        Task<string> UpdateOrganizationGroup(Guid groupId, string? groupName, Guid organizationId, string adminEmail);

        Task<string> DeleteOrganizationGroup(Guid groupId, Guid organizationId, string adminEmail);

        Task<IEnumerable<RetrieveGroupDto>> RetrieveOrganizationGroup(string adminMail, Guid organizationId);

        Task<RetrieveGroupDto> RetrieveOrganizationGroupById(Guid groupId, Guid organizationId, string mail);
    }
}
