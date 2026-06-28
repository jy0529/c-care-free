using CDriveCleanupMaster.Domain.Enums;

namespace CDriveCleanupMaster.Domain.Services;

/// <summary>
/// 风险到默认选择行为的映射。
/// </summary>
public static class RiskSelectionPolicy
{
    public static bool CanSelectByDefault(RiskLevel riskLevel)
    {
        return riskLevel == RiskLevel.Low;
    }
}
