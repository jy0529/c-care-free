namespace CDriveCleanupMaster.Domain.Models;

/// <summary>
/// 磁盘与扫描汇总信息。
/// </summary>
public sealed class DriveScanSummary
{
    public required string DriveName { get; init; }
    public required long TotalBytes { get; init; }
    public required long UsedBytes { get; init; }
    public required long FreeBytes { get; init; }
    public required long ReclaimableBytes { get; init; }
    public required long SelectedBytes { get; init; }
}
