namespace CDriveCleanupMaster.Domain.Models;

/// <summary>
/// 清理类别扫描结果。
/// </summary>
public sealed class CleanupCategoryResult
{
    public required CleanupCategoryDefinition Definition { get; init; }
    public required long TotalBytes { get; init; }
    public required long FileCount { get; init; }
    public required IReadOnlyList<string> ResolvedPaths { get; init; }
    public required bool IsSelected { get; init; }
}
