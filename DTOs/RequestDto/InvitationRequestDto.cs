namespace Task_Management_System.DTOs.RequestDto
{
    public class InvitationRequestDto
    {
        public Guid RequestId { get; set; }

        public Guid OrganizationId { get; set; }

        public string OrganizationName { get; set; } = string.Empty;
    }
}
