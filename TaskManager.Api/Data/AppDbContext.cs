using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("organizations");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).HasColumnName("id");
            entity.Property(o => o.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.OrganizationId).HasColumnName("organization_id").IsRequired();
            entity.Property(u => u.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(u => u.Email).HasColumnName("Email").HasMaxLength(255).IsRequired();
            entity.Property(u => u.JobTitle).HasColumnName("job_title").HasMaxLength(150);
            entity.Property(u => u.PasswordHash).HasColumnName("PasswordHash").HasMaxLength(255).IsRequired();
            entity.Property(u => u.Role)
                .HasColumnName("role")
                .HasMaxLength(20)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(UserRole.Member);

            entity.HasOne(u => u.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(u => new { u.OrganizationId, u.Id }).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.OrganizationId).HasColumnName("organization_id").IsRequired();
            entity.Property(p => p.OwnerId).HasColumnName("owner_id").IsRequired();
            entity.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(p => p.Description).HasColumnName("description").HasColumnType("text");
            entity.Property(p => p.Status)
                .HasColumnName("status")
                .HasMaxLength(30)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(ProjectStatus.Active);
            entity.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd()
                .IsRequired();
            entity.Property(p => p.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAddOrUpdate()
                .IsRequired();

            entity.HasOne(p => p.Organization)
                .WithMany(o => o.Projects)
                .HasForeignKey(p => p.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Owner)
                .WithMany(u => u.OwnedProjects)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("tasks");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("id");
            entity.Property(t => t.ProjectId).HasColumnName("project_id").IsRequired();
            entity.Property(t => t.AssignedToId).HasColumnName("assigned_to_id");
            entity.Property(t => t.Description).HasColumnName("description").HasColumnType("text").IsRequired();
            entity.Property(t => t.Status)
                .HasColumnName("status")
                .HasMaxLength(30)
                .IsRequired()
                .HasConversion(
                    v => v == global::TaskManager.Models.TaskStatus.InProgress ? "In Progress" : v.ToString(),
                    v => v == "In Progress" ? global::TaskManager.Models.TaskStatus.InProgress : Enum.Parse<global::TaskManager.Models.TaskStatus>(v))
                .HasDefaultValue(global::TaskManager.Models.TaskStatus.Todo);
            entity.Property(t => t.Priority)
                .HasColumnName("priority")
                .HasMaxLength(20)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(global::TaskManager.Models.TaskPriority.Medium);
            entity.Property(t => t.DueDate).HasColumnName("due_date").HasColumnType("timestamptz");
            entity.Property(t => t.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd()
                .IsRequired();
            entity.Property(t => t.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAddOrUpdate()
                .IsRequired();

            entity.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id");
            entity.Property(c => c.TaskId).HasColumnName("task_id").IsRequired();
            entity.Property(c => c.AuthorId).HasColumnName("author_id").IsRequired();
            entity.Property(c => c.Description).HasColumnName("description").HasColumnType("text").IsRequired();
            entity.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.HasOne(c => c.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.ToTable("project_members");
            entity.HasKey(pm => new { pm.ProjectId, pm.UserId });
            entity.Property(pm => pm.ProjectId).HasColumnName("project_id").IsRequired();
            entity.Property(pm => pm.UserId).HasColumnName("user_id").IsRequired();

            entity.HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pm => pm.User)
                .WithMany(u => u.ProjectMemberships)
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
 