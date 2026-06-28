using CDriveCleanupMaster.Application.Abstractions;
using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.Application.Services;

/// <summary>
/// 垃圾清理主流程编排。
/// </summary>
public sealed class CleanupWorkspaceService
{
    private readonly ICleanupScanner _cleanupScanner;
    private readonly ICleanupExecutor _cleanupExecutor;

    public CleanupWorkspaceService(ICleanupScanner cleanupScanner, ICleanupExecutor cleanupExecutor)
    {
        _cleanupScanner = cleanupScanner;
        _cleanupExecutor = cleanupExecutor;
    }

    public Task<CleanupScanResult> ScanAsync(string driveRoot, CancellationToken cancellationToken)
    {
        return _cleanupScanner.ScanAsync(driveRoot, cancellationToken);
    }

    public Task<CleanupExecutionResult> ExecuteAsync(string driveRoot, IReadOnlyList<CleanupCategoryResult> categories, CancellationToken cancellationToken)
    {
        return _cleanupExecutor.ExecuteAsync(
            new CleanupExecutionRequest
            {
                DriveRoot = driveRoot,
                Categories = categories
            },
            cancellationToken);
    }
}
