using System.ComponentModel.DataAnnotations;
using System;
using Task_Management_System.Enums;

namespace Task_Management_System.Models
{
    public class Project: StatusDateTime
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Overview { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Guid CreatedBy { get; set; }

        public Guid UpdatedBy { get; set; }

        public Progress Progress { get; set; }

        public Complexity Complexity { get; set; }

        //foreign key for personal projects
        public User? Creator { get; set; }

        public ICollection<Issue>? Issues { get; set; }

        public ICollection<Wiki>? Wiki { get; set; }

        public ICollection<IssueAnalyser>? IssueAnalyser { get; set; }

        //public Guid? GroupId { get; set; }   
        //assign to
        public Guid? GroupId { get; set; }
        public Group? Group { get; set; }

        public Guid? CollaboratorId {get; set; }

        public Group? Collaborator { get; set; }

        public Guid? OrganizationId { get; set; }

        public Organization? Organization { get; set; }
    }
}
