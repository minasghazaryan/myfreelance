namespace MyFreelance.Application.Interfaces;

public interface IPhoneVerificationService
{
    Task SendOtpAsync(string userId, string phoneNumber, CancellationToken cancellationToken = default);
    Task<bool> VerifyOtpAsync(string userId, string code, CancellationToken cancellationToken = default);
}
