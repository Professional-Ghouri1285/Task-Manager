using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public class User
{
    public Guid Id { get; set; }

    [Required]
    public Guid OrganizationId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? JobTitle { get; set; }

    [Required]
    [MaxLength(20)]
    public UserRole Role { get; set; } = UserRole.Member;

    public Organization? Organization { get; set; }
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
}
