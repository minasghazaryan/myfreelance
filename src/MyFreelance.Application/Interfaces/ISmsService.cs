namespace MyFreelance.Application.Interfaces;

public interface ISmsService
{
    Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default);
}
