using Microsoft.AspNetCore.Identity;

namespace Task_Management_System.Models
{
    public class Role : IdentityRole<Guid>
    {
        public ICollection<OrganizationUser>? OrganizationUser { get; set; }

        public ICollection<GroupUser>? GroupUser { get; set; }
    }
}
