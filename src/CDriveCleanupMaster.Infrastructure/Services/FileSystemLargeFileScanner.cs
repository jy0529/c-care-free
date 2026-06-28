using System.Collections.Concurrent;
using System.IO.Enumeration;
using CDriveCleanupMaster.Application.Abstractions;
using CDriveCleanupMaster.Domain.Models;
using CDriveCleanupMaster.Domain.Services;

namespace CDriveCleanupMaster.Infrastructure.Services;

/// <summary>
/// 大文件扫描器。
/// </summary>
public sealed class FileSystemLargeFileScanner : ILargeFileScanner
{
    private const int ProgressIntervalDirectories = 100;
    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        IgnoreInaccessible = true,
        RecurseSubdirectories = false,
        AttributesToSkip = FileAttributes.ReparsePoint
    };

    private readonly IAppLogger _logger;

    public FileSystemLargeFileScanner(IAppLogger logger)
    {
        _logger = logger;
    }

    public Task<LargeFileScanResult> ScanAsync(
        IReadOnlyList<string> roots,
        LargeFileScanOptions options,
        IProgress<ScanProgress>? progress,
        CancellationToken cancellationToken)
    {
        return Task.Run(
            () => ScanCore(roots, options, progress, cancellationToken),
            cancellationToken);
    }

    private LargeFileScanResult ScanCore(
        IReadOnlyList<string> roots,
        LargeFileScanOptions options,
        IProgress<ScanProgress>? progress,
        CancellationToken cancellationToken)
    {
        var pending = new ConcurrentStack<string>();
        foreach (var root in roots.Where(Directory.Exists))
        {
            pending.Push(root);
        }

        var topN = new LargeFileTopNCollector(options.MaxResultCount);
        int directoriesScanned = 0;
        int workerCount = Math.Max(2, Environment.ProcessorCount);

        Parallel.ForEach(
            Enumerable.Range(0, workerCount),
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = workerCount
            },
            _ =>
            {
                while (pending.TryPop(out string? current))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    foreach (var file in GetFilesSafe(current))
                    {
                        try
                        {
                            var info = new FileInfo(file);
                            if (!LargeFileThresholdPolicy.ShouldInclude(info.Length, options.ThresholdBytes))
                            {
                                continue;
                            }

                            topN.TryAdd(new LargeFileItem
                            {
                                Path = info.FullName,
                                Name = info.Name,
                                Extension = info.Extension,
                                Bytes = info.Length,
                                ModifiedAt = info.LastWriteTimeUtc
                            });

                            long matched = topN.TotalMatched;
                            if (matched == 1 || matched % 5 == 0)
                            {
                                int scannedNow = Volatile.Read(ref directoriesScanned);
                                ReportProgress(progress, scannedNow, topN, options, isComplete: false);
                            }
                        }
                        catch (Exception exception)
                        {
                            _logger.Error($"Failed to inspect large file candidate: {file}", exception);
                        }
                    }

                    foreach (var directory in GetDirectoriesSafe(current))
                    {
                        if (!ScanPathSkipPolicy.ShouldSkipDirectory(directory))
                        {
                            pending.Push(directory);
                        }
                    }

                    int scanned = Interlocked.Increment(ref directoriesScanned);
                    if (scanned % ProgressIntervalDirectories == 0)
                    {
                        ReportProgress(progress, scanned, topN, options, isComplete: false);
                    }
                }
            });

        var ranked = LargeFileRelocationMatcher.ApplyHints(topN.ToOrderedArray());

        ReportProgress(progress, directoriesScanned, topN, options, isComplete: true, rankedPreview: ranked);

        return new LargeFileScanResult
        {
            Items = ranked,
            ThresholdBytes = options.ThresholdBytes,
            MaxResultCount = options.MaxResultCount,
            ScannedAt = DateTimeOffset.Now
        };
    }

    private static void ReportProgress(
        IProgress<ScanProgress>? progress,
        int directoriesScanned,
        LargeFileTopNCollector topN,
        LargeFileScanOptions options,
        bool isComplete,
        IReadOnlyList<LargeFileItem>? rankedPreview = null)
    {
        if (progress is null)
        {
            return;
        }

        var preview = rankedPreview ?? LargeFileRelocationMatcher.ApplyHints(topN.ToOrderedArray());
        long largestBytes = preview.Count > 0 ? preview.Max(item => item.Bytes) : 0;
        int percent = isComplete
            ? 100
            : Math.Min(99, 10 + directoriesScanned / 40);

        string countNote = topN.TotalMatched > preview.Count
            ? $"（显示前 {preview.Count:N0} 个，共匹配 {topN.TotalMatched:N0} 个）"
            : string.Empty;

        progress.Report(new ScanProgress
        {
            Phase = isComplete
                ? $"大文件扫描完成，共匹配 {topN.TotalMatched:N0} 个{countNote}，最大 {FormatBytes(largestBytes)}"
                : $"正在多线程扫描大文件… 已遍历 {directoriesScanned:N0} 个目录，匹配 {topN.TotalMatched:N0} 个，当前最大 {FormatBytes(largestBytes)}",
            Percent = percent,
            LargeFileCount = preview.Count,
            LargestBytesFound = largestBytes,
            LargeFilePreview = preview
        });
    }

    private string[] GetFilesSafe(string directory)
    {
        try
        {
            return Directory.GetFiles(directory, "*", EnumerationOptions);
        }
        catch (Exception exception)
        {
            _logger.Error($"Failed to enumerate files in {directory}", exception);
            return [];
        }
    }

    private string[] GetDirectoriesSafe(string directory)
    {
        try
        {
            return Directory.GetDirectories(directory, "*", EnumerationOptions);
        }
        catch (Exception exception)
        {
            _logger.Error($"Failed to enumerate directories in {directory}", exception);
            return [];
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        int index = 0;
        while (value >= 1024 && index < units.Length - 1)
        {
            value /= 1024;
            index++;
        }

        return $"{value:F2} {units[index]}";
    }
}
