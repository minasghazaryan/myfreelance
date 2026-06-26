using MyFreelance.Domain.Common;

namespace MyFreelance.Domain.Entities;

public class LandingContent : BaseEntity
{
    public string SectionKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
