using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public class TaskItem
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    public Guid? AssignedToId { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TaskStatus Status { get; set; } = TaskStatus.Todo;

    [Required]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDate { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
    public User? AssignedTo { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
