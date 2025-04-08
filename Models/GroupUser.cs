namespace Task_Management_System.Models
{
    public class GroupUser
    {
        public Guid Id { get; set; }

        public required User User { get; set; }

        public required Group Group { get; set; }

        public required Role Role { get; set; }
    }
}
