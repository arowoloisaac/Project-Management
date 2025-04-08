namespace Task_Management_System.DTOs.GroupDto.OrganizationGrpDto
{
    public class RetrieveGroupDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int ProjectCount { get; set; }
    }
}
