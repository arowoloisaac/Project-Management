using Task_Management_System.Enums;

namespace Task_Management_System.Models
{
    public class Organization: StatusDateTime
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Guid CreatedBy { get; set; }

        public OrganizationFilter Filter { get; set; }

        //public ICollection<User>? Users { get; set; }

        public ICollection<OrganizationUser>? Users { get; set; }

        public ICollection<Group>? Groups { get; set; }

        public ICollection<Project>? Projects { get; set; }

        public ICollection<Requests>? Requests { get; set; }
    }
}
