using MyFreelance.Domain.Common;

namespace MyFreelance.Domain.Entities;

public class SiteSettings : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string? Description { get; set; }
}
