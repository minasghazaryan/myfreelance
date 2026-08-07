namespace MyFreelance.Application.DTOs.Feedback;

public record PublishedFeedbackDto(Guid Id, string Content, string AuthorName, string AuthorSubtitle, string? Location);

public record ClientFeedbackDto(
    Guid Id,
    string Content,
    bool IsPublished,
    string AuthorName,
    string AuthorEmail,
    string? AuthorSubtitle,
    string? Location,
    DateTime CreatedAt);

public record ClientFeedbackItemDto(Guid Id, string Content, bool IsPublished, DateTime CreatedAt);
