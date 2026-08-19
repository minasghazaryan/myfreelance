using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Cms;
using MyFreelance.Application.DTOs.Feedback;
using MyFreelance.Application.DTOs.Investments;
using MyFreelance.Application.DTOs.Legal;
using MyFreelance.Application.DTOs.Referrals;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Interfaces;

namespace MyFreelance.Web.Pages;

public class IndexModel(
    ICmsService cmsService,
    IFeedbackService feedbackService,
    ILegalDocumentService legalDocumentService,
    IInvestmentService investmentService,
    IReferralService referralService,
    IUnitOfWork unitOfWork) : PageModel
{
    public IReadOnlyList<LandingStatisticDto> Statistics { get; set; } = [];
    public IReadOnlyList<InvestmentTierDto> Tiers { get; set; } = [];
    public IReadOnlyList<ReferralConfigDto> ReferralLevels { get; set; } = [];
    public IReadOnlyList<FaqItemDto> Faqs { get; set; } = [];
    public IReadOnlyList<PublishedFeedbackDto> Testimonials { get; set; } = [];
    public IReadOnlyList<LegalDocumentDto> LegalDocuments { get; set; } = [];
    public string HeroBadge { get; set; } = "Africa's First Investment Fund";
    public string InsuranceBanner { get; set; } = "All deposits are insured by the African Insurance Organisation — AIO. Your capital is fully protected — zero risk to investors.";
    public string ContactEmail { get; set; } = "support@aurumwealth.gh";
    public string ContactTelegram { get; set; } = "@africausainvest";

    public async Task OnGetAsync()
    {
        Statistics = await cmsService.GetLandingStatisticsAsync();
        Tiers = await investmentService.GetActiveTiersAsync();
        ReferralLevels = await referralService.GetReferralConfigAsync();
        Faqs = await cmsService.GetPublishedFaqsAsync();
        Testimonials = await feedbackService.GetPublishedFeedbackAsync();
        LegalDocuments = await legalDocumentService.GetActiveDocumentsAsync();
        HeroBadge = await cmsService.GetSiteSettingAsync("Brand.HeroBadge") ?? HeroBadge;
        InsuranceBanner = await cmsService.GetSiteSettingAsync("Insurance.GlobalBanner") ?? InsuranceBanner;
        ContactEmail = await cmsService.GetSiteSettingAsync("Contact.Email") ?? ContactEmail;
        ContactTelegram = await cmsService.GetSiteSettingAsync("Contact.Telegram") ?? ContactTelegram;
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
