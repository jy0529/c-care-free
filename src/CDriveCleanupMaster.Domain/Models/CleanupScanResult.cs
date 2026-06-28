namespace CDriveCleanupMaster.Domain.Models;

/// <summary>
/// 垃圾清理扫描结果。
/// </summary>
public sealed class CleanupScanResult
{
    public required DriveScanSummary Summary { get; init; }
    public required IReadOnlyList<CleanupCategoryResult> Categories { get; init; }
    public required DateTimeOffset ScannedAt { get; init; }
}
