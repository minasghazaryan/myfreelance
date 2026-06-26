using MyFreelance.Application.DTOs.Cms;

namespace MyFreelance.Application.Interfaces;

public interface ICmsService
{
    Task<IReadOnlyList<FaqItemDto>> GetPublishedFaqsAsync(CancellationToken cancellationToken = default);
    Task<CmsPageDto?> GetPageBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LandingStatisticDto>> GetLandingStatisticsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LandingContentDto>> GetLandingContentAsync(CancellationToken cancellationToken = default);
    Task<SupportChatSettingsDto?> GetSupportChatSettingsAsync(CancellationToken cancellationToken = default);
}
