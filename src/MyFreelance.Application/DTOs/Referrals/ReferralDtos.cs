namespace MyFreelance.Application.DTOs.Referrals;

public record ReferralConfigDto(int Level, decimal Percentage, string Description);
public record ReferralStatsDto(int TeamSize, decimal TotalRewards, int Level1Count, int Level2Count, int Level3Count);
public record ReferralTreeNodeDto(string UserId, string Name, int Level, int DirectReferrals, decimal TotalCommissions, IReadOnlyList<ReferralTreeNodeDto> Children);
public record ReferralTreeDto(string ReferralCode, string ReferralLink, ReferralStatsDto Stats, IReadOnlyList<ReferralTreeNodeDto> Tree);
