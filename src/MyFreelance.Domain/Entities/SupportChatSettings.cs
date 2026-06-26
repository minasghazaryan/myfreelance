using MyFreelance.Domain.Common;

namespace MyFreelance.Domain.Entities;

public class SupportChatSettings : BaseEntity
{
    public string Provider { get; set; } = "Tawk.to";
    public string ScriptContent { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool ShowOnLanding { get; set; } = true;
    public bool ShowOnDashboard { get; set; } = true;
}
