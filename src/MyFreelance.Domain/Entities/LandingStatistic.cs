using MyFreelance.Domain.Common;

namespace MyFreelance.Domain.Entities;

public class LandingStatistic : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Suffix { get; set; }
    public string? Prefix { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
