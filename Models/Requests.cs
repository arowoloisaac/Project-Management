using Task_Management_System.Enums;

namespace Task_Management_System.Models
{
    public class Requests
    {
        public Guid Id { get; set; }

        public string InviteeEmail { get; set; } = string.Empty;

        public Status Status { get; set; }

        public Guid OrganizationId { get; set; }

        public Organization? Organization { get; set; }

        public Guid UserId { get; set; }

        public User? User { get; set; }
    }
}
