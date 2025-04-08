using System.ComponentModel.DataAnnotations;

namespace Task_Management_System.DTOs.UserDto
{
    public class UpdateDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        [RegularExpression(@"^\+?[1-9]\d{1,14}$")]
        public string? PhoneNumber { get; set; }

        public DateOnly BirthDate { get; set; }
    }
}
