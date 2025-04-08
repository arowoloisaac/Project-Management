using Task_Management_System.Models;

namespace Task_Management_System.Services.Configurations.TokenGenerator
{
    public interface ITokenGenerator
    {
        string GenerateToken(User user, IList<string>? userRoles);

        string GenerateToken(User user);
    }
}
