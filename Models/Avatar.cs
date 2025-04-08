namespace Task_Management_System.Models
{
    public class Avatar
    {
        public Guid Id { get; set; }

        public string AvatarUrl { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; }
    }
}
