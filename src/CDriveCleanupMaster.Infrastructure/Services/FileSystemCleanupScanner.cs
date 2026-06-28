using CDriveCleanupMaster.Application.Abstractions;
using CDriveCleanupMaster.Domain.Models;
using CDriveCleanupMaster.Domain.Services;

namespace CDriveCleanupMaster.Infrastructure.Services;

/// <summary>
/// 文件系统扫描器。
/// </summary>
public sealed class FileSystemCleanupScanner : ICleanupScanner
{
    private readonly ICleanupRuleProvider _cleanupRuleProvider;
    private readonly IAppLogger _logger;

    public FileSystemCleanupScanner(ICleanupRuleProvider cleanupRuleProvider, IAppLogger logger)
    {
        _cleanupRuleProvider = cleanupRuleProvider;
        _logger = logger;
    }

    public async Task<CleanupScanResult> ScanAsync(string driveRoot, CancellationToken cancellationToken)
    {
        var definitions = await _cleanupRuleProvider.LoadDefinitionsAsync(cancellationToken).ConfigureAwait(false);
        return await Task.Run(
            () => BuildScanResult(definitions, driveRoot, cancellationToken),
            cancellationToken).ConfigureAwait(false);
    }

    private CleanupScanResult BuildScanResult(
        IReadOnlyList<CleanupCategoryDefinition> definitions,
        string driveRoot,
        CancellationToken cancellationToken)
    {
        var categories = new List<CleanupCategoryResult>();

        foreach (var definition in definitions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            long totalBytes = 0;
            long fileCount = 0;
            var resolvedPaths = new List<string>();

            foreach (var path in definition.Paths)
            {
                resolvedPaths.Add(path);
                var (bytes, files) = GetMetricsSafe(path, cancellationToken);
                totalBytes += bytes;
                fileCount += files;
            }

            categories.Add(new CleanupCategoryResult
            {
                Definition = definition,
                TotalBytes = totalBytes,
                FileCount = fileCount,
                ResolvedPaths = resolvedPaths,
                IsSelected = definition.IsSelectedByDefault && RiskSelectionPolicy.CanSelectByDefault(definition.RiskLevel)
            });
        }

        var driveInfo = new DriveInfo(driveRoot);
        long reclaimable = categories.Sum(category => category.TotalBytes);
        long selected = CleanupSelectionCalculator.CalculateSelectedBytes(categories);

        return new CleanupScanResult
        {
            Summary = new DriveScanSummary
            {
                DriveName = driveInfo.Name,
                TotalBytes = driveInfo.TotalSize,
                UsedBytes = driveInfo.TotalSize - driveInfo.AvailableFreeSpace,
                FreeBytes = driveInfo.AvailableFreeSpace,
                ReclaimableBytes = reclaimable,
                SelectedBytes = selected
            },
            Categories = categories.OrderByDescending(category => category.TotalBytes).ToArray(),
            ScannedAt = DateTimeOffset.Now
        };
    }

    private (long bytes, long files) GetMetricsSafe(string path, CancellationToken cancellationToken)
    {
        try
        {
            if (File.Exists(path))
            {
                var info = new FileInfo(path);
                return (info.Length, 1);
            }

            if (!Directory.Exists(path))
            {
                return (0, 0);
            }

            long bytes = 0;
            long files = 0;
            foreach (var file in EnumerateFilesSafe(path, cancellationToken))
            {
                try
                {
                    var info = new FileInfo(file);
                    bytes += info.Length;
                    files++;
                }
                catch (Exception exception)
                {
                    _logger.Error($"Failed to inspect file: {file}", exception);
                }
            }

            return (bytes, files);
        }
        catch (Exception exception)
        {
            _logger.Error($"Failed to scan path: {path}", exception);
            return (0, 0);
        }
    }

    private IEnumerable<string> EnumerateFilesSafe(string root, CancellationToken cancellationToken)
    {
        var pending = new Stack<string>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string current = pending.Pop();

            string[] files = [];
            try
            {
                files = Directory.GetFiles(current);
            }
            catch (Exception exception)
            {
                _logger.Error($"Failed to enumerate files in {current}", exception);
            }

            foreach (var file in files)
            {
                yield return file;
            }

            string[] directories = [];
            try
            {
                directories = Directory.GetDirectories(current);
            }
            catch (Exception exception)
            {
                _logger.Error($"Failed to enumerate directories in {current}", exception);
            }

            foreach (var directory in directories)
            {
                pending.Push(directory);
            }
        }
    }
}
