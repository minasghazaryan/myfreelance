# Database Schema — AurumWealth

See `ApplicationDbContext` for full EF Core configuration. Key tables:

- **AspNetUsers** (extended): Identity, referral code, KYC flags
- **UserWallets**: AvailableBalance, InvestedCapital, ProjectedEarnings
- **KycProfiles / KycDocuments**: Verification workflow
- **InvestmentTiers / Investments**: Tier-based investing
- **DepositNetworks / Deposits**: USDT TRC20, ERC20, BEP20
- **Withdrawals / WithdrawalPenaltyConfigs**: Payout management
- **ReferralConfigs / ReferralCommissions**: 3-level MLM
- **SmartContracts / SmartContractStrategies**: On-chain registry
- **Notifications / NotificationTemplates**: Multi-channel alerts
- **AuditLogs**: Security and financial audit trail
- **CmsPages / FaqItems / LandingStatistics**: CMS content

```bash
dotnet ef database update --project src/MyFreelance.Infrastructure --startup-project src/MyFreelance.Web
```
