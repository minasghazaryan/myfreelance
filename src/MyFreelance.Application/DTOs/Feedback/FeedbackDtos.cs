using MyFreelance.Domain.Enums;

namespace MyFreelance.Application.DTOs.Feedback;

public record PublishedFeedbackDto(
    Guid Id,
    string Content,
    string AuthorName,
    string AuthorSubtitle,
    string? Location,
    TestimonialMediaType MediaType,
    string? MediaUrl);

public record ClientFeedbackDto(
    Guid Id,
    string Content,
    bool IsPublished,
    string AuthorName,
    string AuthorEmail,
    string? AuthorSubtitle,
    string? Location,
    TestimonialMediaType MediaType,
    string? MediaUrl,
    DateTime CreatedAt);

public record ClientFeedbackItemDto(Guid Id, string Content, bool IsPublished, DateTime CreatedAt);
