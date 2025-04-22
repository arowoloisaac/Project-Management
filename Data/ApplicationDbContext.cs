using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Task_Management_System.Models;

namespace Task_Management_System.Data
{
    public class ApplicationDbContext: IdentityDbContext<User, Role, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public override DbSet<User> Users { get; set; }

        public override DbSet<Role> Roles { get; set; }

        public DbSet<Wiki> Wikis { get; set; }

        public DbSet<Organization> Organizations { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<OrganizationUser> OrganizationUser { get; set; }

        public DbSet<GroupUser> GroupUsers { get; set; }

        public DbSet<Issue> Issues { get; set; }

        public DbSet<Avatar> Avatars { get; set; }

        public DbSet<Requests> Requests { get; set; }

        public DbSet<IssueAnalyser> IssueAnalysers { get; set; }

        public DbSet<IssueRelation> IssueRelations { get; set; }

        public DbSet<Counter> Counters { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>();

            //set projects to null when group is deleted
            builder.Entity<Group>()
                .HasMany(p => p.Projects)
                .WithOne(gp => gp.Group).HasForeignKey(fk => fk.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            //builder.Entity<Group>()
            //     .HasMany(p => p.Projects)
            //     .WithOne(gp => gp.Collaborator)
            //     .OnDelete(DeleteBehavior.SetNull);



            //builder.Entity<Project>().HasMany()

            //to delete all projects in a organization
            builder.Entity<Organization>()
                .HasMany(p => p.Projects)
                .WithOne(org => org.Organization)
                .HasForeignKey(fk => fk.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            //to delete groups in the organization
            builder.Entity<Organization>()
                .HasMany(grp => grp.Groups)
                .WithOne(org => org.Organization)
                .HasForeignKey(fk => fk.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Project>()
                .HasMany(p => p.Issues)
                .WithOne(i => i.Project)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Wiki>()
                .HasOne(cr => cr.User)
                .WithMany()
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Wiki>()
                .HasOne(w => w.UpdatedBy)
                .WithMany()
                .HasForeignKey(w => w.LastUpdateBy).OnDelete(DeleteBehavior.NoAction);



            builder.Entity<Project>()
                .HasOne(p => p.Group)
                .WithMany(g => g.Projects).HasForeignKey(fk => fk.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relationship 2: Group.ProjectCollaborated <-> Project.Collaborator
            builder.Entity<Project>()
                .HasOne(p => p.Collaborator)
                .WithMany(g => g.ProjectCollaborated)
                .HasForeignKey(p => p.CollaboratorId)  // You need to add this FK property in Project
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
