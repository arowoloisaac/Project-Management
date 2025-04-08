using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Management_System.Models
{
    public class Wiki
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedTime { get; set; }

        //this works with the last update
        public DateTime UpdatedTime { get; set; }

        public DateTime DeletedTime { get; set; }

        public Wiki? ParentWiki { get; set; }

        public ICollection<Wiki>? WikiChildren { get; set; }

        //reference to the 
        public Guid? LastUpdateBy { get; set; }

        public Project? Project { get; set; }

        [ForeignKey("LastUpdateBy")]
        public virtual User UpdatedBy { get; set; }

        public Guid CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual User User { get; set; }
    }
}
