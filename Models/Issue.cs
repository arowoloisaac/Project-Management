using System.ComponentModel.DataAnnotations;
using System;
using Task_Management_System.Enums;

namespace Task_Management_System.Models
{
    public class Issue: ObjectDateTime
    {
        public Guid Id { get; set; }
        // has sub task bool+

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public IssueType IssueType { get; set; }

        [Range(0, 100)]
        public int IssueLevel { get; set; } = 0;

        public uint TimeSpent { get; set; } = 0;

        //this works for the minute
        public uint EstimatedTimeInMinutes { get; set; }

        public Complexity Complexity { get; set; }

        public Progress Progress { get; set; }

        //user assigned for the task might be null or the user itself since it's a standalone project
        public User? AssignedTo { get; set; }

        public User? CreatedBy { get; set; }

        public User? UpdatedBy { get; set; }

        public User? User { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedTime { get; set; }

        public Issue? ParentIssue { get; set; }

        public ICollection<Issue>? SubIssues { get; set; }

        //public Guid ProjectId { get; set; }

        public required Project Project { get; set; }

        public ICollection<IssueAnalyser>? IssueAnalyser { get; set; }

        public ICollection<IssueRelation>? IssueRelations { get; set; }
    }
}
