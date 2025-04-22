namespace Task_Management_System.Models
{
    public class Group: UserSection
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public Guid OrganizationId { get; set; }

        public required Organization Organization { get; set; }

        public ICollection<Project>? Projects { get; set; }

        public ICollection<GroupUser>? GroupUsers { get; set; }

        public ICollection<Project>? ProjectCollaborated {  get; set; }
    }
}
