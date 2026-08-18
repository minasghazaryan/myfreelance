using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Feedback;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class FeedbackService(ApplicationDbContext db, IFileStorageService fileStorage) : IFeedbackService
{
    private static readonly HashSet<string> AllowedImageTypes =
        ["image/jpeg", "image/png", "image/webp", "image/gif"];

    private static readonly HashSet<string> AllowedVideoTypes =
        ["video/mp4", "video/webm", "video/quicktime"];

    public async Task SubmitFeedbackAsync(string userId, string content, CancellationToken cancellationToken = default)
    {
        var trimmed = content.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new InvalidOperationException("Feedback cannot be empty.");

        await db.ClientFeedbacks.AddAsync(new ClientFeedback
        {
            UserId = userId,
            Content = trimmed,
            IsPublished = false,
            MediaType = TestimonialMediaType.None
        }, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClientFeedbackItemDto>> GetUserFeedbackAsync(string userId, CancellationToken cancellationToken = default)
        => await db.ClientFeedbacks
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new ClientFeedbackItemDto(f.Id, f.Content, f.IsPublished, f.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PublishedFeedbackDto>> GetPublishedFeedbackAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.ClientFeedbacks
            .Include(f => f.User)
            .Where(f => f.IsPublished)
            .OrderByDescending(f => f.CreatedAt)
            .Take(12)
            .ToListAsync(cancellationToken);

        return items.Select(MapPublished).ToList();
    }

    public async Task<IReadOnlyList<ClientFeedbackDto>> GetAllFeedbackAsync(CancellationToken cancellationToken = default)
        => await db.ClientFeedbacks
            .Include(f => f.User)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new ClientFeedbackDto(
                f.Id,
                f.Content,
                f.IsPublished,
                f.User.FirstName + " " + f.User.LastName,
                f.User.Email!,
                f.AuthorSubtitle,
                f.Location,
                f.MediaType,
                f.MediaPath == null ? null : "/uploads/" + f.MediaPath,
                f.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task PublishFeedbackAsync(
        Guid feedbackId,
        string? displayName,
        string? authorSubtitle,
        string? location,
        Stream? mediaStream,
        string? mediaFileName,
        string? mediaContentType,
        CancellationToken cancellationToken = default)
    {
        var feedback = await db.ClientFeedbacks.FirstOrDefaultAsync(f => f.Id == feedbackId, cancellationToken)
            ?? throw new InvalidOperationException("Feedback not found.");

        feedback.IsPublished = true;
        feedback.DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        feedback.AuthorSubtitle = string.IsNullOrWhiteSpace(authorSubtitle) ? null : authorSubtitle.Trim();
        feedback.Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();

        if (mediaStream is not null && !string.IsNullOrWhiteSpace(mediaFileName))
        {
            await ApplyMediaAsync(feedback, mediaStream, mediaFileName, mediaContentType, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateFeaturedReviewAsync(
        string adminUserId,
        string content,
        string displayName,
        string? authorSubtitle,
        string? location,
        Stream? mediaStream,
        string? mediaFileName,
        string? mediaContentType,
        CancellationToken cancellationToken = default)
    {
        var trimmedContent = content.Trim();
        var trimmedName = displayName.Trim();
        if (string.IsNullOrWhiteSpace(trimmedContent) || string.IsNullOrWhiteSpace(trimmedName))
            throw new InvalidOperationException("Review text and display name are required.");

        var feedback = new ClientFeedback
        {
            UserId = adminUserId,
            Content = trimmedContent,
            IsPublished = true,
            DisplayName = trimmedName,
            AuthorSubtitle = string.IsNullOrWhiteSpace(authorSubtitle) ? "Verified Investor" : authorSubtitle.Trim(),
            Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
            MediaType = TestimonialMediaType.None
        };

        if (mediaStream is not null && !string.IsNullOrWhiteSpace(mediaFileName))
        {
            await ApplyMediaAsync(feedback, mediaStream, mediaFileName, mediaContentType, cancellationToken);
        }

        await db.ClientFeedbacks.AddAsync(feedback, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UnpublishFeedbackAsync(Guid feedbackId, CancellationToken cancellationToken = default)
    {
        var feedback = await db.ClientFeedbacks.FirstOrDefaultAsync(f => f.Id == feedbackId, cancellationToken)
            ?? throw new InvalidOperationException("Feedback not found.");

        feedback.IsPublished = false;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteFeedbackAsync(Guid feedbackId, CancellationToken cancellationToken = default)
    {
        var feedback = await db.ClientFeedbacks.FirstOrDefaultAsync(f => f.Id == feedbackId, cancellationToken)
            ?? throw new InvalidOperationException("Feedback not found.");

        if (!string.IsNullOrWhiteSpace(feedback.MediaPath))
            await fileStorage.DeleteFileAsync(feedback.MediaPath, cancellationToken);

        db.ClientFeedbacks.Remove(feedback);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyMediaAsync(
        ClientFeedback feedback,
        Stream mediaStream,
        string mediaFileName,
        string? mediaContentType,
        CancellationToken cancellationToken)
    {
        var contentType = mediaContentType?.ToLowerInvariant() ?? "application/octet-stream";
        TestimonialMediaType mediaType;
        if (AllowedImageTypes.Contains(contentType))
            mediaType = TestimonialMediaType.Image;
        else if (AllowedVideoTypes.Contains(contentType))
            mediaType = TestimonialMediaType.Video;
        else
            throw new InvalidOperationException("Only image (JPG, PNG, WEBP, GIF) or video (MP4, WEBM) files are allowed.");

        if (!string.IsNullOrWhiteSpace(feedback.MediaPath))
            await fileStorage.DeleteFileAsync(feedback.MediaPath, cancellationToken);

        feedback.MediaPath = await fileStorage.SaveFileAsync(mediaStream, mediaFileName, "testimonials", cancellationToken);
        feedback.MediaType = mediaType;
    }

    private static PublishedFeedbackDto MapPublished(ClientFeedback feedback)
    {
        var authorName = !string.IsNullOrWhiteSpace(feedback.DisplayName)
            ? feedback.DisplayName
            : FormatPrivateName(feedback.User);

        var subtitle = !string.IsNullOrWhiteSpace(feedback.AuthorSubtitle)
            ? feedback.AuthorSubtitle
            : "Verified Investor";

        return new PublishedFeedbackDto(
            feedback.Id,
            feedback.Content,
            authorName,
            subtitle,
            feedback.Location,
            feedback.MediaType,
            feedback.MediaPath is null ? null : "/uploads/" + feedback.MediaPath);
    }

    private static string FormatPrivateName(ApplicationUser user)
    {
        if (string.IsNullOrWhiteSpace(user.LastName))
            return user.FirstName;

        return $"{user.FirstName} {user.LastName[0]}.";
    }
}
