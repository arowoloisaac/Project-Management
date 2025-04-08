using Task_Management_System.DTOs.OrganizationDto;
using Task_Management_System.DTOs.RequestDto;
using Task_Management_System.Enums;

namespace Task_Management_System.Services.OrganizationUserService
{
    public interface IOrganizationUserService
    {
        //send request to the specific user by email
        Task<string> SendOrganizationRequest(Guid organization, string receiver, string adminId);

        Task<string> RevokeOrganizationRequest(Guid organization, string receiver, string adminId);

        Task<string> RemoveUserFromOrganization(Guid organization, string receiver, string adminId);

        //to get a specific organization
        Task<GetOrganizationDto> GetOrganization(Guid organizationId, string mail);

        Task<IEnumerable<GetOrganizationDto>> GetOrganizations(OrganizationFilter? filter, string userMail);

        Task<OrganizationResponse> GetPaginatedOrganizations(OrganizationFilter? filter, int? page, int itemPerPage, string userMail);

        Task<IEnumerable<OrganizationUserDto>> OrganizationUsers(Guid organizationId, string userEmail);

        Task<OrganizationUserDto> RetrieveOrganizationUser(Guid organizationId, string userEmail);

        Task<string> AcceptOrganizationRequest(Guid organizationId, string userEmail);

        Task<string> RejectOrganizationRequest(Guid organizationId, string userEmail);

        Task<IEnumerable<InvitationRequestDto>> InvitationList(string mail);

        Task<IEnumerable<SentRequestDto>> SentRequests(Guid organizationId);
    }
}
