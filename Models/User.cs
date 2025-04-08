using Microsoft.AspNetCore.Identity;

namespace Task_Management_System.Models
{
    public class User: IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateOnly Birthdate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string? AvatarUrl { get; set; }

        public Avatar? Avatar { get; set; }

        //public ICollection<Organization>? Organizations { get; set; }

        public ICollection<OrganizationUser>? Organization { get; set; }

        public ICollection<Project>? Projects { get; set; }

        public ICollection<Wiki>? CreatedByWiki { get; set; }

        public ICollection<Wiki>? UpdatedByWiki { get; set; }

        public ICollection<GroupUser>? GroupUsers { get; set; }

        public ICollection<Requests>? Requests { get; set; }
    }
}
