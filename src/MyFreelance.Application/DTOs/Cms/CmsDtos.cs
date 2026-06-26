namespace MyFreelance.Application.DTOs.Cms;

public record FaqItemDto(Guid Id, string Question, string Answer, string Category);
public record CmsPageDto(string Slug, string Title, string Content);
public record LandingStatisticDto(string Key, string Label, decimal Value, string? Prefix, string? Suffix);
public record LandingContentDto(string SectionKey, string Title, string Subtitle, string Content, string? ImageUrl);
public record SupportChatSettingsDto(bool IsEnabled, string ScriptContent, bool ShowOnLanding, bool ShowOnDashboard);
