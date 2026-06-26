using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Cms;
using MyFreelance.Application.Interfaces;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class CmsService(ApplicationDbContext db) : ICmsService
{
    public async Task<IReadOnlyList<FaqItemDto>> GetPublishedFaqsAsync(CancellationToken cancellationToken = default)
        => await db.FaqItems.Where(f => f.IsPublished).OrderBy(f => f.SortOrder)
            .Select(f => new FaqItemDto(f.Id, f.Question, f.Answer, f.Category)).ToListAsync(cancellationToken);

    public async Task<CmsPageDto?> GetPageBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => await db.CmsPages.Where(p => p.Slug == slug && p.IsPublished)
            .Select(p => new CmsPageDto(p.Slug, p.Title, p.Content)).FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<LandingStatisticDto>> GetLandingStatisticsAsync(CancellationToken cancellationToken = default)
        => await db.LandingStatistics.Where(s => s.IsActive).OrderBy(s => s.SortOrder)
            .Select(s => new LandingStatisticDto(s.Key, s.Label, s.Value, s.Prefix, s.Suffix)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LandingContentDto>> GetLandingContentAsync(CancellationToken cancellationToken = default)
        => await db.LandingContents.Where(c => c.IsActive)
            .Select(c => new LandingContentDto(c.SectionKey, c.Title, c.Subtitle, c.Content, c.ImageUrl)).ToListAsync(cancellationToken);

    public async Task<SupportChatSettingsDto?> GetSupportChatSettingsAsync(CancellationToken cancellationToken = default)
        => await db.SupportChatSettings.OrderByDescending(s => s.CreatedAt)
            .Select(s => new SupportChatSettingsDto(s.IsEnabled, s.ScriptContent, s.ShowOnLanding, s.ShowOnDashboard))
            .FirstOrDefaultAsync(cancellationToken);
}
