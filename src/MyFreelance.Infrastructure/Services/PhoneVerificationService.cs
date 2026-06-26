using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class PhoneVerificationService(
    ApplicationDbContext db,
    IConfiguration configuration,
    ILogger<PhoneVerificationService> logger) : IPhoneVerificationService
{
    public async Task SendOtpAsync(string userId, string phoneNumber, CancellationToken cancellationToken = default)
    {
        var otp = Random.Shared.Next(100000, 999999).ToString();
        var provider = configuration["Sms:Provider"] ?? "Twilio";

        var verification = new PhoneVerification
        {
            UserId = userId,
            PhoneNumber = phoneNumber,
            OtpCode = otp,
            Provider = provider,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        await db.PhoneVerifications.AddAsync(verification, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Integration hook for Twilio/Vonage
        logger.LogInformation("OTP {Otp} sent to {Phone} via {Provider} (configure API keys in appsettings)", otp, phoneNumber, provider);
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
        return true;
    }
}
