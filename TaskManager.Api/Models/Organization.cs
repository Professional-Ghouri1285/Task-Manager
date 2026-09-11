using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public class Organization
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
