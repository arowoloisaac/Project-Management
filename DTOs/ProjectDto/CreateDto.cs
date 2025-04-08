using System.ComponentModel.DataAnnotations;
using Task_Management_System.Enums;

namespace Task_Management_System.DTOs.ProjectDto
{
    public class CreateDto
    {
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Overview { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Complexity Complexity { get; set; }
    }
}
