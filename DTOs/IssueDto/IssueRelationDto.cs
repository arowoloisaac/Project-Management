namespace Task_Management_System.DTOs.IssueDto
{
    public class IssueRelationDto
    {
        public Guid Id { get; set; }

        public List<RelatedDto> Relations { get; set; } = new List<RelatedDto>();
    }

    public class RelatedDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
