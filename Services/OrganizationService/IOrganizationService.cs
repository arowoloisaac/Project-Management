using Task_Management_System.DTOs.OrganizationDto;

namespace Task_Management_System.Services.OrganizationService
{
    public interface IOrganizationService
    {
        Task<string> CreateOrganization(CreateOrganizationDto dto, string mail);

        Task<string> DeleteOrganization(Guid organizationId, string mail);

        Task<string> UpdateOrganization(Guid organizationId, string mail, UpdateOrganizationDto dto);
    }
}
