using MyFreelance.Domain.Common;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Domain.Entities;

public class CmsPage : BaseEntity
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public CmsPageType PageType { get; set; }
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }
}
