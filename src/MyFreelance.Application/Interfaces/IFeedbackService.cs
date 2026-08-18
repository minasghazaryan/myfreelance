using MyFreelance.Application.DTOs.Feedback;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Application.Interfaces;

public interface IFeedbackService
{
    Task SubmitFeedbackAsync(string userId, string content, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClientFeedbackItemDto>> GetUserFeedbackAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PublishedFeedbackDto>> GetPublishedFeedbackAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClientFeedbackDto>> GetAllFeedbackAsync(CancellationToken cancellationToken = default);
    Task PublishFeedbackAsync(
        Guid feedbackId,
        string? displayName,
        string? authorSubtitle,
        string? location,
        Stream? mediaStream,
        string? mediaFileName,
        string? mediaContentType,
        CancellationToken cancellationToken = default);
    Task CreateFeaturedReviewAsync(
        string adminUserId,
        string content,
        string displayName,
        string? authorSubtitle,
        string? location,
        Stream? mediaStream,
        string? mediaFileName,
        string? mediaContentType,
        CancellationToken cancellationToken = default);
    Task UnpublishFeedbackAsync(Guid feedbackId, CancellationToken cancellationToken = default);
    Task DeleteFeedbackAsync(Guid feedbackId, CancellationToken cancellationToken = default);
}
