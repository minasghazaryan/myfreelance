using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Domain.Entities;

namespace MyFreelance.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<KycProfile> KycProfiles => Set<KycProfile>();
    public DbSet<KycDocument> KycDocuments => Set<KycDocument>();
    public DbSet<UserWallet> UserWallets => Set<UserWallet>();
    public DbSet<InvestmentTier> InvestmentTiers => Set<InvestmentTier>();
    public DbSet<Investment> Investments => Set<Investment>();
    public DbSet<DepositNetwork> DepositNetworks => Set<DepositNetwork>();
    public DbSet<Deposit> Deposits => Set<Deposit>();
    public DbSet<Withdrawal> Withdrawals => Set<Withdrawal>();
    public DbSet<ReferralConfig> ReferralConfigs => Set<ReferralConfig>();
    public DbSet<ReferralCommission> ReferralCommissions => Set<ReferralCommission>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<SmartContract> SmartContracts => Set<SmartContract>();
    public DbSet<SmartContractStrategy> SmartContractStrategies => Set<SmartContractStrategy>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<CmsPage> CmsPages => Set<CmsPage>();
    public DbSet<FaqItem> FaqItems => Set<FaqItem>();
    public DbSet<LandingStatistic> LandingStatistics => Set<LandingStatistic>();
    public DbSet<LandingContent> LandingContents => Set<LandingContent>();
    public DbSet<SupportChatSettings> SupportChatSettings => Set<SupportChatSettings>();
    public DbSet<WithdrawalPenaltyConfig> WithdrawalPenaltyConfigs => Set<WithdrawalPenaltyConfig>();
    public DbSet<PhoneVerification> PhoneVerifications => Set<PhoneVerification>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(u => u.ReferralCode).IsUnique().HasFilter("[ReferralCode] IS NOT NULL");
            entity.HasOne(u => u.ReferredBy)
                .WithMany(u => u.Referrals)
                .HasForeignKey(u => u.ReferredByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<UserWallet>(entity =>
        {
            entity.HasIndex(w => w.UserId).IsUnique();
            entity.Property(w => w.AvailableBalance).HasPrecision(18, 8);
            entity.Property(w => w.InvestedCapital).HasPrecision(18, 8);
            entity.Property(w => w.ProjectedEarnings).HasPrecision(18, 8);
            entity.Property(w => w.ReferralEarnings).HasPrecision(18, 8);
            entity.Property(w => w.TotalDeposited).HasPrecision(18, 8);
            entity.Property(w => w.TotalWithdrawn).HasPrecision(18, 8);
        });

        builder.Entity<InvestmentTier>(entity =>
        {
            entity.Property(t => t.ProjectedYieldPercent).HasPrecision(8, 4);
            entity.Property(t => t.MinInvestment).HasPrecision(18, 8);
            entity.Property(t => t.MaxInvestment).HasPrecision(18, 8);
        });

        builder.Entity<Investment>(entity =>
        {
            entity.Property(i => i.Amount).HasPrecision(18, 8);
            entity.Property(i => i.ProjectedYieldPercent).HasPrecision(8, 4);
        });

        builder.Entity<DepositNetwork>(entity =>
        {
            entity.Property(n => n.MinDeposit).HasPrecision(18, 8);
        });

        builder.Entity<WithdrawalPenaltyConfig>(entity =>
        {
            entity.Property(w => w.PenaltyPercent).HasPrecision(8, 4);
        });

        builder.Entity<Deposit>(entity =>
        {
            entity.Property(d => d.Amount).HasPrecision(18, 8);
        });

        builder.Entity<Withdrawal>(entity =>
        {
            entity.Property(w => w.Amount).HasPrecision(18, 8);
            entity.Property(w => w.Fee).HasPrecision(18, 8);
            entity.Property(w => w.PenaltyAmount).HasPrecision(18, 8);
            entity.Property(w => w.NetAmount).HasPrecision(18, 8);
        });

        builder.Entity<ReferralConfig>(entity =>
        {
            entity.HasIndex(r => r.Level).IsUnique();
            entity.Property(r => r.Percentage).HasPrecision(8, 4);
        });

        builder.Entity<ReferralCommission>(entity =>
        {
            entity.Property(r => r.Percentage).HasPrecision(8, 4);
            entity.Property(r => r.SourceAmount).HasPrecision(18, 8);
            entity.Property(r => r.CommissionAmount).HasPrecision(18, 8);

            entity.HasOne(r => r.BeneficiaryUser)
                .WithMany(u => u.ReferralCommissions)
                .HasForeignKey(r => r.BeneficiaryUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.SourceUser)
                .WithMany()
                .HasForeignKey(r => r.SourceUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Transaction>(entity =>
        {
            entity.Property(t => t.Amount).HasPrecision(18, 8);
            entity.Property(t => t.BalanceAfter).HasPrecision(18, 8);
        });

        builder.Entity<SmartContract>(entity =>
        {
            entity.Property(s => s.TotalAllocatedCapital).HasPrecision(18, 8);
        });

        builder.Entity<SmartContractStrategy>(entity =>
        {
            entity.Property(s => s.AllocationPercent).HasPrecision(8, 4);
            entity.Property(s => s.HistoricalReturnPercent).HasPrecision(8, 4);
            entity.Property(s => s.RiskScore).HasPrecision(8, 4);
        });

        builder.Entity<LandingStatistic>(entity =>
        {
            entity.HasIndex(s => s.Key).IsUnique();
            entity.Property(s => s.Value).HasPrecision(18, 2);
        });

        builder.Entity<CmsPage>(entity =>
        {
            entity.HasIndex(p => p.Slug).IsUnique();
        });

        builder.Entity<SiteSettings>(entity =>
        {
            entity.HasIndex(s => s.Key).IsUnique();
        });
    }
}
