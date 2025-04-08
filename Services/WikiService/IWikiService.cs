using Task_Management_System.DTOs.WikiDto;

namespace Task_Management_System.Services.WikiService
{
    public interface IWikiService
    {
        Task<string> CreateWiki(Guid projectId, WikiDto wikiDto, string userId);

        Task<GetWikiDto> GetWiki(Guid projectId, Guid wikiId, string userId);

        Task<string> UpdateWiki(Guid projectId, Guid wikiId, WikiDto wikiDto, string userId);

        Task<string> DeleteWiki(Guid projectId, bool isDeleteChild, Guid wikiId, string userId);

        Task<string> CreateWikiChild(Guid projectId, WikiDto wikiDto, Guid parentWikiId, string userId);

        Task<IEnumerable<GetWikiDto>> GetWikis(Guid projectId, string userId);
    }
}
