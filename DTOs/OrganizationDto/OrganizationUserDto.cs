namespace Task_Management_System.DTOs.OrganizationDto
{
    public class OrganizationUserDto
    {
        public Guid UserId { get; set; }

        //public Guid OrganizationId { get; set; }
        public string UserImage { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string UserRole { get; set; } = string.Empty;
    }


    public class GroupUserDto
    {
        public Guid Id { get; set; }

        //public Guid GroupId { get; set; }
        public string UserImage { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
