using System.ComponentModel.DataAnnotations;
using MyFreelance.Data;

namespace MyFreelance.Models;

public class WorkTask
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Pending;

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string AssignedToUserId { get; set; } = string.Empty;

    public ApplicationUser AssignedTo { get; set; } = null!;

    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    public ApplicationUser CreatedBy { get; set; } = null!;
}
