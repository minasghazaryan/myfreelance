using MyFreelance.Domain.Common;

namespace MyFreelance.Domain.Entities;

public class FaqItem : BaseEntity
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public string Category { get; set; } = "General";
}
