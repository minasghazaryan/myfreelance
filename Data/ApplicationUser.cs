using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using MyFreelance.Models;

namespace MyFreelance.Data;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? JobTitle { get; set; }

    public bool IsHired { get; set; }

    public DateTime? HiredAt { get; set; }

    public ICollection<WorkTask> AssignedTasks { get; set; } = [];

    public ICollection<WorkTask> CreatedTasks { get; set; } = [];
}
