namespace Task_Management_System.Models
{
    public class CollaborationRequest
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public Guid SenderId { get; set; }

        public Guid ReceiverId { get; set; }
    }
}
