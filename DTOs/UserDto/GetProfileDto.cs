namespace Task_Management_System.DTOs.UserDto
{
    public class GetProfileDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public DateOnly Birthdate { get; set; }

        public string AvatarUrl { get; set; } = string.Empty;
    }
}
