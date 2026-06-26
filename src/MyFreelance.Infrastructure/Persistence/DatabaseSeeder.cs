using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyFreelance.Domain.Constants;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        await db.Database.MigrateAsync();

        foreach (var role in new[] { AppRoles.Admin, AppRoles.Investor, AppRoles.Compliance, AppRoles.Support })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (await userManager.FindByEmailAsync("admin@aurumwealth.gh") is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@aurumwealth.gh",
                Email = "admin@aurumwealth.gh",
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                IsKycApproved = true,
                IsPhoneVerified = true,
                ReferralCode = "AWADMIN001"
            };
            await userManager.CreateAsync(admin, "Admin@123!");
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            await db.UserWallets.AddAsync(new UserWallet { UserId = admin.Id });
        }

        if (!await db.InvestmentTiers.AnyAsync())
        {
            db.InvestmentTiers.AddRange(
                new InvestmentTier { Name = "Bronze", Description = "Entry level tier with conservative allocation and lower projected yield.", RiskLevel = RiskLevel.Low, ProjectedYieldPercent = 8.5m, MinInvestment = 100, MaxInvestment = 5000, SortOrder = 1, AccentColor = "#CD7F32", IconClass = "bi-award" },
                new InvestmentTier { Name = "Silver", Description = "Balanced portfolio with moderate projected yield and diversified strategies.", RiskLevel = RiskLevel.Moderate, ProjectedYieldPercent = 14.2m, MinInvestment = 5000, MaxInvestment = 50000, SortOrder = 2, AccentColor = "#C0C0C0", IconClass = "bi-gem" },
                new InvestmentTier { Name = "Gold", Description = "Advanced allocation with higher projected yield and dynamic rebalancing.", RiskLevel = RiskLevel.High, ProjectedYieldPercent = 22.8m, MinInvestment = 50000, MaxInvestment = 500000, SortOrder = 3, AccentColor = "#D4AF37", IconClass = "bi-trophy" }
            );
        }

        if (!await db.ReferralConfigs.AnyAsync())
        {
            db.ReferralConfigs.AddRange(
                new ReferralConfig { Level = 1, Percentage = 10m, Description = "Direct referral commission" },
                new ReferralConfig { Level = 2, Percentage = 5m, Description = "Second-level referral commission" },
                new ReferralConfig { Level = 3, Percentage = 2m, Description = "Third-level referral commission" }
            );
        }

        if (!await db.DepositNetworks.AnyAsync())
        {
            db.DepositNetworks.AddRange(
                new DepositNetwork { Name = "USDT TRC20", Code = "TRC20", WalletAddress = "TPlaceholderTRC20WalletAddress123456789", MinDeposit = 50, RequiredConfirmations = 20, SortOrder = 1 },
                new DepositNetwork { Name = "USDT ERC20", Code = "ERC20", WalletAddress = "0xPlaceholderERC20WalletAddress1234567890", MinDeposit = 100, RequiredConfirmations = 12, SortOrder = 2 },
                new DepositNetwork { Name = "USDT BEP20", Code = "BEP20", WalletAddress = "0xPlaceholderBEP20WalletAddress1234567890", MinDeposit = 50, RequiredConfirmations = 15, SortOrder = 3 }
            );
        }

        if (!await db.LandingStatistics.AnyAsync())
        {
            db.LandingStatistics.AddRange(
                new LandingStatistic { Key = "investors", Label = "Active Investors", Value = 12847, Suffix = "+", SortOrder = 1 },
                new LandingStatistic { Key = "aum", Label = "Assets Under Management", Value = 48.5m, Prefix = "$", Suffix = "M", SortOrder = 2 },
                new LandingStatistic { Key = "rewards", Label = "Total Distributed Rewards", Value = 12.3m, Prefix = "$", Suffix = "M", SortOrder = 3 },
                new LandingStatistic { Key = "countries", Label = "Countries Served", Value = 4, SortOrder = 4 }
            );
        }

        if (!await db.FaqItems.AnyAsync())
        {
            db.FaqItems.AddRange(
                new FaqItem { Question = "Are returns guaranteed?", Answer = "No. AurumWealth displays projected yields based on historical performance and algorithmic models. All investments carry risk and past performance does not guarantee future results.", SortOrder = 1 },
                new FaqItem { Question = "How does the smart contract engine work?", Answer = "Our capital allocation engine deploys funds across multiple on-chain strategies and automatically rebalances based on risk-adjusted performance metrics.", SortOrder = 2 },
                new FaqItem { Question = "What is required before I can invest?", Answer = "You must complete identity verification (KYC), verify your phone number, and deposit funds via supported USDT networks.", SortOrder = 3 },
                new FaqItem { Question = "Which countries are supported?", Answer = "We primarily serve Ghana with expanding support for Nigeria, Kenya, and South Africa.", SortOrder = 4 }
            );
        }

        if (!await db.CmsPages.AnyAsync())
        {
            db.CmsPages.AddRange(
                new CmsPage { Slug = "terms", Title = "Terms of Service", PageType = CmsPageType.Terms, Content = "<p>Terms of service content managed via Admin CMS.</p>" },
                new CmsPage { Slug = "privacy", Title = "Privacy Policy", PageType = CmsPageType.Privacy, Content = "<p>Privacy policy content managed via Admin CMS.</p>" }
            );
        }

        if (!await db.WithdrawalPenaltyConfigs.AnyAsync())
        {
            db.WithdrawalPenaltyConfigs.Add(new WithdrawalPenaltyConfig { Name = "Immediate Withdrawal", PenaltyPercent = 5m, MinDaysHeld = 0, AppliesToImmediate = true });
        }

        if (!await db.SmartContracts.AnyAsync())
        {
            var contract = new SmartContract { Name = "Aurum Allocation Engine v1", Network = "Ethereum", ContractAddress = "0xAurumEnginePlaceholder", TotalAllocatedCapital = 48500000m, Status = SmartContractStatus.Active };
            db.SmartContracts.Add(contract);
            await db.SaveChangesAsync();

            db.SmartContractStrategies.AddRange(
                new SmartContractStrategy { SmartContractId = contract.Id, Name = "DeFi Yield Optimizer", AllocationPercent = 35m, HistoricalReturnPercent = 18.5m, RiskScore = 4.2m },
                new SmartContractStrategy { SmartContractId = contract.Id, Name = "Stablecoin Liquidity Pool", AllocationPercent = 25m, HistoricalReturnPercent = 12.1m, RiskScore = 2.8m },
                new SmartContractStrategy { SmartContractId = contract.Id, Name = "Cross-Chain Arbitrage", AllocationPercent = 20m, HistoricalReturnPercent = 24.3m, RiskScore = 6.5m },
                new SmartContractStrategy { SmartContractId = contract.Id, Name = "Governance Token Staking", AllocationPercent = 20m, HistoricalReturnPercent = 15.7m, RiskScore = 5.1m }
            );
        }

        if (!await db.SupportChatSettings.AnyAsync())
        {
            db.SupportChatSettings.Add(new SupportChatSettings
            {
                IsEnabled = false,
                ScriptContent = "<!-- Tawk.to script - configure in Admin Panel -->",
                ShowOnLanding = true,
                ShowOnDashboard = true
            });
        }

        if (!await db.NotificationTemplates.AnyAsync())
        {
            db.NotificationTemplates.AddRange(
                new NotificationTemplate { EventType = NotificationEventType.Registration, Channel = NotificationChannel.InApp, Subject = "Welcome to AurumWealth", BodyTemplate = "Your account has been created successfully." },
                new NotificationTemplate { EventType = NotificationEventType.Deposit, Channel = NotificationChannel.InApp, Subject = "Deposit Received", BodyTemplate = "Your deposit is being processed." },
                new NotificationTemplate { EventType = NotificationEventType.Withdrawal, Channel = NotificationChannel.InApp, Subject = "Withdrawal Requested", BodyTemplate = "Your withdrawal request has been submitted." },
                new NotificationTemplate { EventType = NotificationEventType.ReferralReward, Channel = NotificationChannel.InApp, Subject = "Referral Reward", BodyTemplate = "You earned a referral commission!" }
            );
        }

        if (!await db.SiteSettings.AnyAsync())
        {
            db.SiteSettings.AddRange(
                new SiteSettings { Key = "Contact.Email", Value = "support@aurumwealth.gh", Category = "Contact" },
                new SiteSettings { Key = "Contact.WhatsApp", Value = "+233201234567", Category = "Contact" },
                new SiteSettings { Key = "Contact.Telegram", Value = "@AurumWealthGH", Category = "Contact" },
                new SiteSettings { Key = "Brand.Name", Value = "AurumWealth", Category = "Brand" }
            );
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Database seed completed.");
    }
}
