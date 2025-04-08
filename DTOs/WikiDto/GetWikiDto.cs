namespace Task_Management_System.DTOs.WikiDto
{
    public class GetWikiDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;

        public string UpdatedBy { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }

        public List<GetWikiDto> Wiki { get; set; } = new List<GetWikiDto>();
    }
}
