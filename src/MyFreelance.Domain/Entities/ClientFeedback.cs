using MyFreelance.Domain.Common;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Domain.Entities;

public class ClientFeedback : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public string? DisplayName { get; set; }
    public string? AuthorSubtitle { get; set; }
    public string? Location { get; set; }
    public TestimonialMediaType MediaType { get; set; }
    public string? MediaPath { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
