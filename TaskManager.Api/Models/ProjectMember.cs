using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public class ProjectMember
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public Project? Project { get; set; }
    public User? User { get; set; }
}
