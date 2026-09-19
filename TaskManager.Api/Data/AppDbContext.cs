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
            entity.Property(o => o.Name)
                .HasMaxLength(150)
                .IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.OrganizationId).IsRequired();
            entity.Property(u => u.Name).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
            entity.Property(u => u.JobTitle).HasMaxLength(150);
            entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(u => u.Role)
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
            entity.Property(p => p.OrganizationId).IsRequired();
            entity.Property(p => p.OwnerId).IsRequired();
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Description).HasColumnType("text");
            entity.Property(p => p.Status)
                .HasMaxLength(30)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(ProjectStatus.Active);
            entity.Property(p => p.CreatedAt)
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd()
                .IsRequired();
            entity.Property(p => p.UpdatedAt)
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
            entity.Property(t => t.ProjectId).IsRequired();
            entity.Property(t => t.AssignedToId);
            entity.Property(t => t.Description).HasColumnType("text").IsRequired();
            entity.Property(t => t.Status)
                .HasMaxLength(30)
                .IsRequired()
                .HasConversion(
                    v => v == global::TaskManager.Models.TaskStatus.InProgress ? "In Progress" : v.ToString(),
                    v => v == "In Progress" ? global::TaskManager.Models.TaskStatus.InProgress : Enum.Parse<global::TaskManager.Models.TaskStatus>(v))
                .HasDefaultValue(global::TaskManager.Models.TaskStatus.Todo);
            entity.Property(t => t.Priority)
                .HasMaxLength(20)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(global::TaskManager.Models.TaskPriority.Medium);
            entity.Property(t => t.DueDate).HasColumnType("timestamptz");
            entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd()
                .IsRequired();
            entity.Property(t => t.UpdatedAt)
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
            entity.Property(c => c.TaskId).IsRequired();
            entity.Property(c => c.AuthorId).IsRequired();
            entity.Property(c => c.Description).HasColumnType("text").IsRequired();
            entity.Property(c => c.CreatedAt)
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
            entity.Property(pm => pm.ProjectId).IsRequired();
            entity.Property(pm => pm.UserId).IsRequired();

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
 