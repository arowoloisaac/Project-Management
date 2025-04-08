using System.Threading.Tasks;
using Task_Management_System.DTOs.UserDto;
using Task_Management_System.Models;

namespace Task_Management_System.Services.UserService
{
    public interface IUserService
    {
        Task<TokenResponse> RegisterUser(RegisterDto registerDto);

        Task<TokenResponse> LoginUser(LoginDto loginDto);

        Task<GetProfileDto> UserProfile(string userId);

        Task<GetProfileDto> UpdateProfile(UpdateDto updateDto, Guid? avatar, string userId);
    }
}
