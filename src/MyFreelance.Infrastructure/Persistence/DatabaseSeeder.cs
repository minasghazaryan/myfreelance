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
        await SyncWalletYieldEarnedAsync(db);

        foreach (var role in new[] { AppRoles.Admin, AppRoles.AdminReadOnly, AppRoles.Investor, AppRoles.Compliance, AppRoles.Support })
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
            db.InvestmentTiers.AddRange(CreateDefaultInvestmentTiers());
        }
        else if (!await db.SiteSettings.AnyAsync(s => s.Key == "Tiers.Defaults.v2"))
        {
            await SyncInvestmentTiersAsync(db);
            db.SiteSettings.Add(new SiteSettings { Key = "Tiers.Defaults.v2", Value = "true", Category = "System" });
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
            db.DepositNetworks.AddRange(CreateDefaultDepositNetworks());
        }

        if (!await db.SiteSettings.AnyAsync(s => s.Key == "DepositNetworks.Wallets.v2"))
        {
            await SyncDepositNetworksAsync(db);
            db.SiteSettings.Add(new SiteSettings { Key = "DepositNetworks.Wallets.v2", Value = "true", Category = "System" });
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
                new FaqItem { Question = "Are returns guaranteed?", Answer = "No. Investment Fund U.S.Africa displays projected yields based on historical performance and algorithmic models. All investments carry risk and past performance does not guarantee future results.", SortOrder = 1 },
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
                new NotificationTemplate { EventType = NotificationEventType.Registration, Channel = NotificationChannel.InApp, Subject = "Welcome to Investment Fund U.S.Africa", BodyTemplate = "Your account has been created successfully." },
                new NotificationTemplate { EventType = NotificationEventType.Deposit, Channel = NotificationChannel.InApp, Subject = "Deposit Received", BodyTemplate = "Your deposit of {Amount} is {Status}." },
                new NotificationTemplate { EventType = NotificationEventType.Withdrawal, Channel = NotificationChannel.InApp, Subject = "Withdrawal Requested", BodyTemplate = "Your withdrawal of {Amount} is {Status}." },
                new NotificationTemplate { EventType = NotificationEventType.ReferralReward, Channel = NotificationChannel.InApp, Subject = "Referral Reward", BodyTemplate = "You earned {Amount} from {ReferralName} (Level {Level})." },
                new NotificationTemplate { EventType = NotificationEventType.KycStatusChange, Channel = NotificationChannel.InApp, Subject = "KYC Status Updated", BodyTemplate = "Your KYC status is now {Status}." },
                new NotificationTemplate { EventType = NotificationEventType.Verification, Channel = NotificationChannel.InApp, Subject = "Verification Update", BodyTemplate = "Verification status: {Status}." },
                new NotificationTemplate { EventType = NotificationEventType.TierUpgrade, Channel = NotificationChannel.InApp, Subject = "Investment Active", BodyTemplate = "Your {TierName} investment of {Amount} is now active." }
            );
        }

        if (!await db.NotificationTemplates.AnyAsync(t => t.EventType == NotificationEventType.BonusAward))
        {
            db.NotificationTemplates.Add(new NotificationTemplate
            {
                EventType = NotificationEventType.BonusAward,
                Channel = NotificationChannel.InApp,
                Subject = "Bonus Credited",
                BodyTemplate = "You received a bonus of {Amount}. {Description}"
            });
        }

        if (!await db.ClientFeedbacks.AnyAsync())
        {
            var seedUser = await userManager.FindByEmailAsync("admin@aurumwealth.gh");
            if (seedUser is not null)
            {
                db.ClientFeedbacks.AddRange(
                    new ClientFeedback
                    {
                        UserId = seedUser.Id,
                        Content = "The dashboard gave me clarity I never had with informal crypto deals. I know my tier, my balance, and my withdrawal rules.",
                        IsPublished = true,
                        DisplayName = "Kwame A.",
                        AuthorSubtitle = "Silver Tier Investor",
                        Location = "Accra, Ghana"
                    },
                    new ClientFeedback
                    {
                        UserId = seedUser.Id,
                        Content = "Verification felt thorough — that actually increased my confidence. Support responded the same day when I had deposit questions.",
                        IsPublished = true,
                        DisplayName = "Adwoa M.",
                        AuthorSubtitle = "Bronze Tier Investor",
                        Location = "Kumasi, Ghana"
                    },
                    new ClientFeedback
                    {
                        UserId = seedUser.Id,
                        Content = "I appreciate that projected yields are labeled as projections. The platform feels professional, not like a get-rich-quick scheme.",
                        IsPublished = true,
                        DisplayName = "Emmanuel O.",
                        AuthorSubtitle = "Gold Tier Investor",
                        Location = "Lagos, Nigeria"
                    });
            }
        }

        if (!await db.SiteSettings.AnyAsync())
        {
            db.SiteSettings.AddRange(
                new SiteSettings { Key = "Contact.Email", Value = "support@aurumwealth.gh", Category = "Contact" },
                new SiteSettings { Key = "Contact.WhatsApp", Value = "+233201234567", Category = "Contact" },
                new SiteSettings { Key = "Contact.Telegram", Value = "@AurumWealthGH", Category = "Contact" },
                new SiteSettings { Key = "Brand.Name", Value = BrandConstants.Name, Category = "Brand" },
                new SiteSettings { Key = "Brand.HeroBadge", Value = "Africa's First Investment Fund", Category = "Brand" },
                new SiteSettings { Key = "Insurance.GlobalBanner", Value = "All deposits are insured by the African Insurance Organisation — AIO. Your capital is fully protected — zero risk to investors.", Category = "Insurance" }
            );
        }
        else
        {
            await EnsureSiteSettingAsync(db, "Brand.HeroBadge", "Africa's First Investment Fund", "Brand");
            await EnsureSiteSettingAsync(db, "Insurance.GlobalBanner", "All deposits are insured by the African Insurance Organisation — AIO. Your capital is fully protected — zero risk to investors.", "Insurance");
        }

        if (!await db.SiteSettings.AnyAsync(s => s.Key == "Branding.Insurance.v1"))
        {
            const string insuranceNotice = "Insured by the African Insurance Organisation — AIO — zero risk.";
            foreach (var tier in await db.InvestmentTiers.ToListAsync())
            {
                if (string.IsNullOrWhiteSpace(tier.InsuranceNotice))
                    tier.InsuranceNotice = insuranceNotice;
            }

            db.SiteSettings.Add(new SiteSettings { Key = "Branding.Insurance.v1", Value = "true", Category = "System" });
        }

        if (!await db.SiteSettings.AnyAsync(s => s.Key == "Branding.Insurance.AIO.v1"))
        {
            const string insuranceNotice = "Insured by the African Insurance Organisation — AIO — zero risk.";
            const string globalBanner = "All deposits are insured by the African Insurance Organisation — AIO. Your capital is fully protected — zero risk to investors.";
            var globalSetting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "Insurance.GlobalBanner");
            if (globalSetting is not null)
                globalSetting.Value = globalBanner;

            foreach (var tier in await db.InvestmentTiers.ToListAsync())
            {
                if (string.IsNullOrWhiteSpace(tier.InsuranceNotice) ||
                    tier.InsuranceNotice.Contains("African Association of Insurers", StringComparison.OrdinalIgnoreCase))
                    tier.InsuranceNotice = insuranceNotice;
            }

            db.SiteSettings.Add(new SiteSettings { Key = "Branding.Insurance.AIO.v1", Value = "true", Category = "System" });
        }

        if (!await db.SiteSettings.AnyAsync(s => s.Key == "Branding.Name.v1"))
        {
            var brandSetting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "Brand.Name");
            if (brandSetting is not null)
                brandSetting.Value = BrandConstants.Name;

            db.SiteSettings.Add(new SiteSettings { Key = "Branding.Name.v1", Value = "true", Category = "System" });
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Database seed completed.");
    }

    private static async Task SyncWalletYieldEarnedAsync(ApplicationDbContext db)
    {
        var wallets = await db.UserWallets.ToListAsync();
        foreach (var wallet in wallets)
        {
            var actualYield = await db.Transactions
                .Where(t => t.UserId == wallet.UserId
                    && t.Type == TransactionType.YieldCredit
                    && t.Status == TransactionStatus.Completed)
                .SumAsync(t => t.Amount);

            if (wallet.ProjectedEarnings != actualYield)
                wallet.ProjectedEarnings = actualYield;
        }

        await db.SaveChangesAsync();
    }

    private static InvestmentTier[] CreateDefaultInvestmentTiers() =>
    [
        new InvestmentTier
        {
            Name = "Bronze",
            Description = "Entry level tier with conservative allocation and lower projected yield.",
            PackageDetails = "Conservative DeFi and stablecoin allocation mix\n30-day investment cycle with daily yield accrual\nIdeal starting point for new investors\nWithdrawal terms and penalties apply after cycle ends\nFully insured by the African Insurance Organisation — AIO",
            RiskLevel = RiskLevel.Low,
            ProjectedYieldPercent = 10m,
            MinInvestment = 100,
            MaxInvestment = 500,
            SortOrder = 1,
            AccentColor = "#CD7F32",
            IconClass = "bi-award"
        },
        new InvestmentTier
        {
            Name = "Silver",
            Description = "Balanced portfolio with moderate projected yield and diversified strategies.",
            PackageDetails = "Balanced mix of yield farming, LP, and arbitrage strategies\n30-day cycle with higher projected returns than Bronze\nDiversified on-chain allocation with risk scoring\nPriority support for Silver-tier investors\nFully insured by the African Insurance Organisation — AIO",
            RiskLevel = RiskLevel.Moderate,
            ProjectedYieldPercent = 15m,
            MinInvestment = 500,
            MaxInvestment = 5000,
            SortOrder = 2,
            AccentColor = "#C0C0C0",
            IconClass = "bi-gem"
        },
        new InvestmentTier
        {
            Name = "Gold",
            Description = "Advanced allocation with higher projected yield and dynamic rebalancing.",
            PackageDetails = "Advanced multi-strategy deployment with dynamic rebalancing\nHighest projected yield tier for experienced investors\nReal-time risk monitoring and automated adjustments\nDedicated account manager for Gold-tier members\nFully insured by the African Insurance Organisation — AIO",
            RiskLevel = RiskLevel.High,
            ProjectedYieldPercent = 25m,
            MinInvestment = 5000,
            MaxInvestment = 20000,
            SortOrder = 3,
            AccentColor = "#D4AF37",
            IconClass = "bi-trophy"
        }
    ];

    private static async Task SyncInvestmentTiersAsync(ApplicationDbContext db)
    {
        foreach (var defaults in CreateDefaultInvestmentTiers())
        {
            var tier = await db.InvestmentTiers.FirstOrDefaultAsync(t => t.Name == defaults.Name);
            if (tier is null)
            {
                db.InvestmentTiers.Add(defaults);
                continue;
            }

            tier.ProjectedYieldPercent = defaults.ProjectedYieldPercent;
            tier.MinInvestment = defaults.MinInvestment;
            tier.MaxInvestment = defaults.MaxInvestment;
            tier.RiskLevel = defaults.RiskLevel;
            tier.SortOrder = defaults.SortOrder;
            tier.Description = defaults.Description;
            tier.PackageDetails = defaults.PackageDetails;
            tier.AccentColor = defaults.AccentColor;
            tier.IconClass = defaults.IconClass;
            tier.IsActive = true;
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureSiteSettingAsync(ApplicationDbContext db, string key, string value, string category)
    {
        if (await db.SiteSettings.AnyAsync(s => s.Key == key))
            return;

        db.SiteSettings.Add(new SiteSettings { Key = key, Value = value, Category = category });
        await db.SaveChangesAsync();
    }

    private static DepositNetwork[] CreateDefaultDepositNetworks() =>
        GetDepositNetworkDefinitions()
            .Select(n => new DepositNetwork
            {
                Name = n.Name,
                Code = n.Code,
                Currency = n.Currency,
                WalletAddress = n.WalletAddress,
                MinDeposit = n.MinDeposit,
                RequiredConfirmations = n.RequiredConfirmations,
                SortOrder = n.SortOrder
            })
            .ToArray();

    private static async Task SyncDepositNetworksAsync(ApplicationDbContext db)
    {
        foreach (var definition in GetDepositNetworkDefinitions())
        {
            var network = await db.DepositNetworks.FirstOrDefaultAsync(n => n.Code == definition.Code);
            if (network is null)
            {
                db.DepositNetworks.Add(new DepositNetwork
                {
                    Name = definition.Name,
                    Code = definition.Code,
                    Currency = definition.Currency,
                    WalletAddress = definition.WalletAddress,
                    MinDeposit = definition.MinDeposit,
                    RequiredConfirmations = definition.RequiredConfirmations,
                    SortOrder = definition.SortOrder
                });
                continue;
            }

            network.Name = definition.Name;
            network.Currency = definition.Currency;
            network.WalletAddress = definition.WalletAddress;
            network.MinDeposit = definition.MinDeposit;
            network.RequiredConfirmations = definition.RequiredConfirmations;
            network.SortOrder = definition.SortOrder;
            network.IsActive = true;
        }
    }

    private static IReadOnlyList<(string Name, string Code, string Currency, string WalletAddress, decimal MinDeposit, int RequiredConfirmations, int SortOrder)> GetDepositNetworkDefinitions() =>
    [
        ("Bitcoin", "BTC", "BTC", "bc1q2v8c2amhxr4v7x7d6ys5xatdvd7vn7ra0y8frm", 50m, 3, 1),
        ("Ethereum", "ETH", "ETH", "0x616D1cc3b2d4F9745F90915E9bB0DAd95031C577", 50m, 12, 2),
        ("Solana", "SOL", "SOL", "DN5inRSx2wvDEWwr127P417ch4r2eyVC66n1vNSAn8bA", 50m, 32, 3),
        ("USDT TRC20", "TRC20", "USDT", "TD2rSZ735G9jsoA718t7E9mrvg9okxC2Sb", 50m, 20, 4),
        ("USDT ERC20", "ERC20", "USDT", "0x616D1cc3b2d4F9745F90915E9bB0DAd95031C577", 100m, 12, 5),
        ("USDT BEP20", "BEP20", "USDT", "0x616D1cc3b2d4F9745F90915E9bB0DAd95031C577", 50m, 15, 6)
    ];
}
