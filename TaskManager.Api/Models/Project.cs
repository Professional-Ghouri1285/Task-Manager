using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public class Project
{
    public Guid Id { get; set; }

    [Required]
    public Guid OrganizationId { get; set; }

    [Required]
    public Guid OwnerId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Organization? Organization { get; set; }
    public User? Owner { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
}
