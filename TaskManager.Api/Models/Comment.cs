using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public class Comment
{
    public Guid Id { get; set; }

    [Required]
    public Guid TaskId { get; set; }

    [Required]
    public Guid AuthorId { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TaskItem? Task { get; set; }
    public User? Author { get; set; }
}
