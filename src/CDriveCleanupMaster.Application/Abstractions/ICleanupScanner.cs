using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.Application.Abstractions;

/// <summary>
/// 垃圾清理扫描接口。
/// </summary>
public interface ICleanupScanner
{
    Task<CleanupScanResult> ScanAsync(string driveRoot, CancellationToken cancellationToken);
}
