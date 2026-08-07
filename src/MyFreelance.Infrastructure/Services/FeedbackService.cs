using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Feedback;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class FeedbackService(ApplicationDbContext db) : IFeedbackService
{
    public async Task SubmitFeedbackAsync(string userId, string content, CancellationToken cancellationToken = default)
    {
        var trimmed = content.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new InvalidOperationException("Feedback cannot be empty.");

        await db.ClientFeedbacks.AddAsync(new ClientFeedback
        {
            UserId = userId,
            Content = trimmed,
            IsPublished = false
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
                f.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task PublishFeedbackAsync(
        Guid feedbackId,
        string? displayName,
        string? authorSubtitle,
        string? location,
        CancellationToken cancellationToken = default)
    {
        var feedback = await db.ClientFeedbacks.FirstOrDefaultAsync(f => f.Id == feedbackId, cancellationToken)
            ?? throw new InvalidOperationException("Feedback not found.");

        feedback.IsPublished = true;
        feedback.DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        feedback.AuthorSubtitle = string.IsNullOrWhiteSpace(authorSubtitle) ? null : authorSubtitle.Trim();
        feedback.Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();

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

        db.ClientFeedbacks.Remove(feedback);
        await db.SaveChangesAsync(cancellationToken);
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
            feedback.Location);
    }

    private static string FormatPrivateName(ApplicationUser user)
    {
        if (string.IsNullOrWhiteSpace(user.LastName))
            return user.FirstName;

        return $"{user.FirstName} {user.LastName[0]}.";
    }
}
