using CDriveCleanupMaster.Application.Abstractions;
using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.Application.Services;

/// <summary>
/// 大文件主流程编排。
/// </summary>
public sealed class LargeFileWorkspaceService
{
    private readonly ILargeFileScanner _largeFileScanner;
    private readonly IFileMover _fileMover;

    public LargeFileWorkspaceService(ILargeFileScanner largeFileScanner, IFileMover fileMover)
    {
        _largeFileScanner = largeFileScanner;
        _fileMover = fileMover;
    }

    public Task<LargeFileScanResult> ScanAsync(
        IReadOnlyList<string> roots,
        LargeFileScanOptions options,
        IProgress<ScanProgress>? progress,
        CancellationToken cancellationToken)
    {
        return _largeFileScanner.ScanAsync(roots, options, progress, cancellationToken);
    }

    public Task<string> MoveAsync(string sourcePath, string destinationDirectory, CancellationToken cancellationToken)
    {
        return _fileMover.MoveAsync(sourcePath, destinationDirectory, cancellationToken);
    }
}
