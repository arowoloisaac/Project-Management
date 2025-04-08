using System.ComponentModel.DataAnnotations;

namespace Task_Management_System.DTOs.UserDto
{
    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "You have to input the correct email format")]
        public string Email { get; set; } = string.Empty;

        public DateOnly BirthDate { get; set; }

        [Phone]
        [RegularExpression(@"^\+?[1-9]\d{1,14}$")]
        public string PhoneNumber { get; set; } = string.Empty;

        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
