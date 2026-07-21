using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class PhoneVerificationService(
    ApplicationDbContext db,
    ISmsService smsService,
    INotificationService notificationService,
    IConfiguration configuration,
    ILogger<PhoneVerificationService> logger) : IPhoneVerificationService
{
    public async Task SendOtpAsync(string userId, string phoneNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new InvalidOperationException("Phone number is required.");

        var otp = Random.Shared.Next(100000, 999999).ToString();
        var provider = configuration["Sms:Provider"] ?? "Twilio";
        var message = $"Your AurumWealth verification code is {otp}. It expires in 10 minutes.";

        await smsService.SendAsync(phoneNumber, message, cancellationToken);

        var verification = new PhoneVerification
        {
            UserId = userId,
            PhoneNumber = TwilioSmsService.NormalizePhoneNumber(phoneNumber),
            OtpCode = otp,
            Provider = provider,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        await db.PhoneVerifications.AddAsync(verification, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("OTP queued for user {UserId} via {Provider}", userId, provider);
    }

    public async Task<bool> VerifyOtpAsync(string userId, string code, CancellationToken cancellationToken = default)
    {
        var verification = await db.PhoneVerifications
            .Where(v => v.UserId == userId && !v.IsVerified && v.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (verification is null) return false;

        verification.AttemptCount++;
        if (verification.OtpCode != code || verification.AttemptCount > 5)
        {
            await db.SaveChangesAsync(cancellationToken);
            return false;
        }

        verification.IsVerified = true;
        var user = await db.Users.FindAsync([userId], cancellationToken);
        if (user is not null)
        {
            user.IsPhoneVerified = true;
            user.PhoneNumber = verification.PhoneNumber;
        }

        await db.SaveChangesAsync(cancellationToken);

        await notificationService.SendEventNotificationAsync(
            userId,
            NotificationEventType.Verification,
            new Dictionary<string, string>
            {
                ["Status"] = "Verified",
                ["PhoneNumber"] = verification.PhoneNumber,
                ["Description"] = "Your phone number has been verified successfully via WhatsApp/SMS OTP."
            },
            cancellationToken);

        return true;
    }
}
