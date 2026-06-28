using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.Application.Abstractions;

/// <summary>
/// 大文件扫描接口。
/// </summary>
public interface ILargeFileScanner
{
    Task<LargeFileScanResult> ScanAsync(
        IReadOnlyList<string> roots,
        LargeFileScanOptions options,
        IProgress<ScanProgress>? progress,
        CancellationToken cancellationToken);
}
