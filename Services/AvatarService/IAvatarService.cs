using Task_Management_System.DTOs.AvatarDto;

namespace Task_Management_System.Services.AvatarService
{
    public interface IAvatarService
    {
        Task AddAvatar(string avatarUrl);

        Task<GetAvatarDto> GetAvatar(Guid Id);

        Task<IEnumerable<GetAvatarDto>> GetAvatars();

        Task DeleteAvatar(Guid Id);
    }
}
