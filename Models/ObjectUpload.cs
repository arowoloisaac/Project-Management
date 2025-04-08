namespace Task_Management_System.Models
{
    public class ObjectUpload
    {
        public required IFormFile File { get; set; }

        public string FileName { get; set; } = string.Empty;
    }
}
