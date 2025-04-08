using System.ComponentModel.DataAnnotations;

namespace Task_Management_System.DTOs.UserDto
{
    public class LoginDto
    {
        [EmailAddress(ErrorMessage = "Enter the correct format of your mail")]
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
