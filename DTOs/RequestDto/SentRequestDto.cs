namespace Task_Management_System.DTOs.RequestDto
{
    public class SentRequestDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
