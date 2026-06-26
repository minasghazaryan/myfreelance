using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Cms;
using MyFreelance.Application.DTOs.Investments;
using MyFreelance.Application.DTOs.Referrals;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Interfaces;

namespace MyFreelance.Web.Pages;

public class IndexModel(
    ICmsService cmsService,
    IInvestmentService investmentService,
    IReferralService referralService,
    IUnitOfWork unitOfWork) : PageModel
{
    public IReadOnlyList<LandingStatisticDto> Statistics { get; set; } = [];
    public IReadOnlyList<InvestmentTierDto> Tiers { get; set; } = [];
    public IReadOnlyList<ReferralConfigDto> ReferralLevels { get; set; } = [];
    public IReadOnlyList<FaqItemDto> Faqs { get; set; } = [];
    public SupportChatSettingsDto? ChatSettings { get; set; }

    public async Task OnGetAsync()
    {
        Statistics = await cmsService.GetLandingStatisticsAsync();
        Tiers = await investmentService.GetActiveTiersAsync();
        ReferralLevels = await referralService.GetReferralConfigAsync();
        Faqs = await cmsService.GetPublishedFaqsAsync();
        ChatSettings = await cmsService.GetSupportChatSettingsAsync();
    }

    public async Task<IActionResult> OnPostContactAsync(string name, string email, string subject, string message)
    {
        await unitOfWork.Repository<ContactMessage>().AddAsync(new ContactMessage
        {
            Name = name,
            Email = email,
            Subject = subject,
            Message = message
        });
        await unitOfWork.SaveChangesAsync();
        TempData["ContactSuccess"] = true;
        return RedirectToPage();
    }
}
