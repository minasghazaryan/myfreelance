namespace MyFreelance.Infrastructure.Options;

public class SmsOptions
{
    public const string SectionName = "Sms";

    public string Provider { get; set; } = "Twilio";
    public TwilioOptions Twilio { get; set; } = new();
}

public class TwilioOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
    public string Channel { get; set; } = "Sms";

    public bool UseWhatsApp => Channel.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase);
}
