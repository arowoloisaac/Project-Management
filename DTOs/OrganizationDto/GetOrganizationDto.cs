namespace Task_Management_System.DTOs.OrganizationDto
{
    public class GetOrganizationDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Creator { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }

        public DateTime DateJoined { get; set; }
    }
}
