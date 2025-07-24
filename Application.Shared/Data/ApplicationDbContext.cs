using Application.Shared.Models;
using Application.Shared.Models.User;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Application.Shared.Data
{
    //public delegate ApplicationDbContext DbContextFactory(string companyId);

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("application_user");
            });
            modelBuilder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable("user_claim");
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.ToTable("user_login");
            });

            modelBuilder.Entity<IdentityUserToken<string>>(b =>
            {
                b.ToTable("token");
            });

            modelBuilder.Entity<IdentityRole>(b =>
            {
                b.ToTable("role");
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.ToTable("role_claim");
            });

            modelBuilder.Entity<IdentityUserRole<string>>(b =>
            {
                b.ToTable("user_role");
            });

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Configure Entity relationships
            modelBuilder.Entity<EntityDependency>()
                .HasOne(ed => ed.Entity)
                .WithMany(e => e.Dependencies)
                .HasForeignKey(ed => ed.EntityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EntityDependency>()
                .HasOne(ed => ed.DependsOnEntity)
                .WithMany(e => e.DependentOn)
                .HasForeignKey(ed => ed.DependsOnEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Incident relationships
            modelBuilder.Entity<Incident>()
                .HasOne(i => i.Entity)
                .WithMany(e => e.Incidents)
                .HasForeignKey(i => i.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IncidentUpdate>()
                .HasOne(iu => iu.Incident)
                .WithMany(i => i.Updates)
                .HasForeignKey(iu => iu.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Job relationships
            modelBuilder.Entity<Job>()
                .HasOne(j => j.Entity)
                .WithMany(e => e.Jobs)
                .HasForeignKey(j => j.EntityId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<JobExecution>()
                .HasOne(je => je.Job)
                .WithMany(j => j.JobExecutions)
                .HasForeignKey(je => je.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Alert relationships
            modelBuilder.Entity<AlertRule>()
                .HasOne(ar => ar.Entity)
                .WithMany()
                .HasForeignKey(ar => ar.EntityId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AlertInstance>()
                .HasOne(ai => ai.AlertRule)
                .WithMany(ar => ar.AlertInstances)
                .HasForeignKey(ai => ai.AlertRuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure StatusPage relationships
            modelBuilder.Entity<StatusPageEntity>()
                .HasOne(spe => spe.StatusPage)
                .WithMany(sp => sp.StatusPageEntities)
                .HasForeignKey(spe => spe.StatusPageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StatusPageEntity>()
                .HasOne(spe => spe.Entity)
                .WithMany()
                .HasForeignKey(spe => spe.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entity.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                {
                    entity.SetTableName(ToSnakeCase(tableName));
                }

                foreach (var property in entity.GetProperties())
                {
                    var attributes = property.PropertyInfo?.GetCustomAttributesData();

                    if (attributes == null || !attributes.Any(a => a.AttributeType.Name == "ColumnAttribute"))
                    {
                        property.SetColumnName(ToSnakeCase(property.Name));
                    }
                }
            }
        }

        public DbSet<Workspace> Workspace { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMember { get; set; }
        public DbSet<WorkspaceDomain> WorkspaceDomain { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        
        // Status Page Models
        public DbSet<Entity> Entity { get; set; }
        public DbSet<EntityDependency> EntityDependency { get; set; }
        public DbSet<EntityStatusHistory> EntityStatusHistory { get; set; }
        public DbSet<Incident> Incident { get; set; }
        public DbSet<IncidentUpdate> IncidentUpdate { get; set; }
        public DbSet<Job> Job { get; set; }
        public DbSet<JobExecution> JobExecution { get; set; }
        public DbSet<AlertRule> AlertRule { get; set; }
        public DbSet<AlertInstance> AlertInstance { get; set; }
        public DbSet<StatusPage> StatusPage { get; set; }
        public DbSet<StatusPageEntity> StatusPageEntity { get; set; }



        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var stringBuilder = new StringBuilder();
            var previousCharWasUpper = false;

            foreach (var character in input)
            {
                if (char.IsUpper(character))
                {
                    if (stringBuilder.Length != 0 && !previousCharWasUpper)
                    {
                        stringBuilder.Append('_');
                    }
                    stringBuilder.Append(char.ToLowerInvariant(character));
                    previousCharWasUpper = true;
                }
                else
                {
                    stringBuilder.Append(character);
                    previousCharWasUpper = false;
                }
            }

            return stringBuilder.ToString();
        }
    }
}
