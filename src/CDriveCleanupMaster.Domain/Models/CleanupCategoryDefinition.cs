using CDriveCleanupMaster.Domain.Enums;

namespace CDriveCleanupMaster.Domain.Models;

/// <summary>
/// 清理类别定义。
/// </summary>
public sealed class CleanupCategoryDefinition
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required string GroupName { get; init; }
    public required string IconKey { get; init; }
    public required RiskLevel RiskLevel { get; init; }
    public required bool IsExecutable { get; init; }
    public required bool IsSelectedByDefault { get; init; }
    public IReadOnlyList<string> Paths { get; init; } = Array.Empty<string>();
}
