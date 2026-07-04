using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyFreelance.Application.Interfaces;
using MyFreelance.Infrastructure.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace MyFreelance.Infrastructure.Services;

public class TwilioSmsService(
    IOptions<SmsOptions> options,
    ILogger<TwilioSmsService> logger,
    IHostEnvironment environment) : ISmsService
{
    public async Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default)
    {
        var twilio = options.Value.Twilio;
        var to = ToTwilioAddress(toPhoneNumber, twilio);
        var from = ToTwilioAddress(twilio.FromNumber, twilio, isFromNumber: true);

        if (string.Equals(to.ToString(), from.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Twilio FromNumber cannot be the same as the recipient. For WhatsApp sandbox, set FromNumber to whatsapp:+14155238886.");
        }

        if (!IsConfigured(twilio))
        {
            if (environment.IsDevelopment())
            {
                logger.LogWarning(
                    "Twilio is not configured. Simulated {Channel} message to {Phone}: {Message}",
                    twilio.Channel,
                    to,
                    message);
                return;
            }

            throw new InvalidOperationException("Twilio is not configured. Add credentials in appsettings or user secrets.");
        }

        TwilioClient.Init(twilio.AccountSid, twilio.AuthToken);

        try
        {
            var result = await MessageResource.CreateAsync(
                body: message,
                from: from,
                to: to);

            if (result.ErrorCode is not null)
            {
                logger.LogError(
                    "Twilio {Channel} failed for {Phone}: {Code} {Message}",
                    twilio.Channel,
                    to,
                    result.ErrorCode,
                    result.ErrorMessage);
                throw new InvalidOperationException(result.ErrorMessage ?? "Failed to send message.");
            }

            logger.LogInformation("Twilio {Channel} sent to {Phone}. MessageSid: {Sid}", twilio.Channel, to, result.Sid);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            logger.LogError(ex, "Twilio {Channel} send failed for {Phone}", twilio.Channel, to);
            throw new InvalidOperationException(MapTwilioError(ex), ex);
        }
    }

    private static string MapTwilioError(Exception ex)
    {
        if (ex.Message.Contains("Authenticate", StringComparison.OrdinalIgnoreCase))
            return "Twilio authentication failed. Check AccountSid and AuthToken in configuration.";

        if (ex.Message.Contains("same To and From", StringComparison.OrdinalIgnoreCase))
            return "Twilio FromNumber cannot be your own phone number. For sandbox use whatsapp:+14155238886 as FromNumber.";

        return string.IsNullOrWhiteSpace(ex.Message) ? "Failed to send message via Twilio." : ex.Message;
    }

    private static bool IsConfigured(TwilioOptions twilio) =>
        !string.IsNullOrWhiteSpace(twilio.AccountSid) &&
        !string.IsNullOrWhiteSpace(twilio.AuthToken) &&
        !string.IsNullOrWhiteSpace(twilio.FromNumber);

    internal static string NormalizePhoneNumber(string phoneNumber)
    {
        var trimmed = phoneNumber.Trim();
        if (trimmed.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
            trimmed = trimmed["whatsapp:".Length..].Trim();

        if (string.IsNullOrWhiteSpace(trimmed))
            throw new InvalidOperationException("Phone number is required.");

        var digits = new string(trimmed.Where(c => char.IsDigit(c) || c == '+').ToArray());
        if (digits.StartsWith('+'))
            return digits;

        if (digits.StartsWith("00"))
            return "+" + digits[2..];

        return "+" + digits;
    }

    private static PhoneNumber ToTwilioAddress(string phoneNumber, TwilioOptions twilio, bool isFromNumber = false)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new InvalidOperationException(isFromNumber ? "Twilio FromNumber is required." : "Phone number is required.");

        if (phoneNumber.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
            return new PhoneNumber(phoneNumber);

        var normalized = NormalizePhoneNumber(phoneNumber);
        return twilio.UseWhatsApp
            ? new PhoneNumber($"whatsapp:{normalized}")
            : new PhoneNumber(normalized);
    }
}
