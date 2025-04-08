using Task_Management_System.Models;

namespace Task_Management_System.DTOs.OrganizationDto
{
    public class OrganizationResponse
    {
        public IEnumerable<GetOrganizationDto> Org { get; set; } = new List<GetOrganizationDto>();

        public Pagination Pagination { get; set; }

        public OrganizationResponse(List<GetOrganizationDto> org, int page, int total, int count, int start, int end, int totalItem)
        {
            Org = org;

            Pagination = new Pagination
            {
                Count = count,
                Current = page,
                Size = total,
                Start = start,
                End = end,
                TotalItems = totalItem
            };
        }
    }
}
