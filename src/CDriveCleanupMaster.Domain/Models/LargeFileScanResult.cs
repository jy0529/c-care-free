namespace CDriveCleanupMaster.Domain.Models;

/// <summary>
/// 大文件扫描结果。
/// </summary>
public sealed class LargeFileScanResult
{
    public required IReadOnlyList<LargeFileItem> Items { get; init; }
    public required long ThresholdBytes { get; init; }
    public int MaxResultCount { get; init; } = 500;
    public required DateTimeOffset ScannedAt { get; init; }
}
