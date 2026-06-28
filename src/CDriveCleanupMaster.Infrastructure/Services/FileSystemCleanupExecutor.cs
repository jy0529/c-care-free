using CDriveCleanupMaster.Application.Abstractions;
using CDriveCleanupMaster.Domain;
using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.Infrastructure.Services;

/// <summary>
/// 文件系统清理执行器。
/// </summary>
public sealed class FileSystemCleanupExecutor : ICleanupExecutor
{
    private readonly IAppLogger _logger;

    public FileSystemCleanupExecutor(IAppLogger logger)
    {
        _logger = logger;
    }

    public Task<CleanupExecutionResult> ExecuteAsync(CleanupExecutionRequest request, CancellationToken cancellationToken)
    {
        return Task.Run(() => ExecuteCore(request, cancellationToken), cancellationToken);
    }

    private CleanupExecutionResult ExecuteCore(CleanupExecutionRequest request, CancellationToken cancellationToken)
    {
        var items = new List<CleanupExecutionItemResult>();

        foreach (var category in request.Categories.Where(item => item.IsSelected))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (category.Definition.Id == CleanupCategoryIds.RecycleBin)
            {
                items.Add(EmptyRecycleBin(category, request.DriveRoot));
                continue;
            }

            if (!category.Definition.IsExecutable)
            {
                items.Add(new CleanupExecutionItemResult
                {
                    CategoryId = category.Definition.Id,
                    DisplayName = category.Definition.DisplayName,
                    Success = false,
                    BytesFreed = 0,
                    ErrorMessage = "该类别当前不允许自动清理。"
                });
                continue;
            }

            long bytesFreed = 0;
            int skippedCount = 0;
            bool pathAccessDenied = false;
            bool fileInUse = false;

            foreach (var path in category.ResolvedPaths)
            {
                ClearPathResult cleared = ClearPathContents(path);
                bytesFreed += cleared.BytesFreed;
                skippedCount += cleared.SkippedCount;
                pathAccessDenied |= cleared.PathAccessDenied;
                fileInUse |= cleared.FileInUse;
            }

            long remainingBytes = category.ResolvedPaths.Sum(MeasurePathBytesSafe);
            bool success = bytesFreed > 0
                || category.TotalBytes == 0
                || (category.TotalBytes > 0 && remainingBytes < category.TotalBytes);

            string? errorMessage = BuildErrorMessage(
                category,
                success,
                skippedCount,
                pathAccessDenied,
                fileInUse,
                remainingBytes);

            items.Add(new CleanupExecutionItemResult
            {
                CategoryId = category.Definition.Id,
                DisplayName = category.Definition.DisplayName,
                Success = success,
                BytesFreed = success ? Math.Max(bytesFreed, category.TotalBytes - remainingBytes) : bytesFreed,
                ErrorMessage = errorMessage
            });
        }

        return new CleanupExecutionResult
        {
            TotalBytesFreed = items.Where(item => item.Success).Sum(item => item.BytesFreed),
            Items = items
        };
    }

    private static string? BuildErrorMessage(
        CleanupCategoryResult category,
        bool success,
        int skippedCount,
        bool pathAccessDenied,
        bool fileInUse,
        long remainingBytes)
    {
        if (success)
        {
            if (skippedCount > 0 || remainingBytes > 0)
            {
                return remainingBytes > 0
                    ? $"已清理大部分内容，仍有 {FormatBytes(remainingBytes)} 未能删除。"
                    : $"部分文件被跳过（{skippedCount} 个）。";
            }

            return null;
        }

        if (fileInUse)
        {
            return category.Definition.GroupName == "浏览器缓存"
                ? "缓存文件正在被浏览器占用，请先完全关闭 Edge/Chrome 后重试。"
                : "部分文件正在被其他程序占用，请关闭相关程序后重试。";
        }

        if (pathAccessDenied && IsSystemPath(category))
        {
            return "该目录需要管理员权限，请右键以管理员身份运行本程序后重试。";
        }

        if (skippedCount > 0)
        {
            return $"有 {skippedCount} 个文件无法删除，可能正在被使用。";
        }

        return category.TotalBytes > 0 ? "未能释放空间，请确认目录存在且未被占用。" : null;
    }

    private static bool IsSystemPath(CleanupCategoryResult category) =>
        category.ResolvedPaths.Any(path =>
            path.Contains(@"\Windows\", StringComparison.OrdinalIgnoreCase)
            || path.Contains(@"\Program Files", StringComparison.OrdinalIgnoreCase));

    private CleanupExecutionItemResult EmptyRecycleBin(CleanupCategoryResult category, string driveRoot)
    {
        if (!category.Definition.IsExecutable)
        {
            return new CleanupExecutionItemResult
            {
                CategoryId = category.Definition.Id,
                DisplayName = category.Definition.DisplayName,
                Success = false,
                BytesFreed = 0,
                ErrorMessage = "该类别当前不允许自动清理。"
            };
        }

        try
        {
            WindowsRecycleBinService.EmptyDrive(driveRoot);
            return new CleanupExecutionItemResult
            {
                CategoryId = category.Definition.Id,
                DisplayName = category.Definition.DisplayName,
                Success = true,
                BytesFreed = category.TotalBytes,
                ErrorMessage = null
            };
        }
        catch (Exception exception)
        {
            _logger.Error("Failed to empty recycle bin.", exception);
            return new CleanupExecutionItemResult
            {
                CategoryId = category.Definition.Id,
                DisplayName = category.Definition.DisplayName,
                Success = false,
                BytesFreed = 0,
                ErrorMessage = exception.Message
            };
        }
    }

    private ClearPathResult ClearPathContents(string path)
    {
        if (File.Exists(path))
        {
            return TryDeleteFile(path);
        }

        if (!Directory.Exists(path))
        {
            return new ClearPathResult(0, 0, PathAccessDenied: false, FileInUse: false);
        }

        long bytesFreed = 0;
        int skippedCount = 0;
        bool pathAccessDenied = false;
        bool fileInUse = false;

        foreach (var file in EnumerateFilesSafe(path, ref pathAccessDenied))
        {
            ClearPathResult deleted = TryDeleteFile(file);
            bytesFreed += deleted.BytesFreed;
            skippedCount += deleted.SkippedCount;
            pathAccessDenied |= deleted.PathAccessDenied;
            fileInUse |= deleted.FileInUse;
        }

        foreach (var directory in EnumerateDirectoriesSafe(path, ref pathAccessDenied).ToArray())
        {
            ClearPathResult deleted = ClearPathContents(directory);
            bytesFreed += deleted.BytesFreed;
            skippedCount += deleted.SkippedCount;
            pathAccessDenied |= deleted.PathAccessDenied;
            fileInUse |= deleted.FileInUse;

            if (!Directory.Exists(directory))
            {
                continue;
            }

            long dirBytes = MeasurePathBytesSafe(directory);
            try
            {
                Directory.Delete(directory, recursive: true);
                bytesFreed += dirBytes;
            }
            catch (Exception exception)
            {
                skippedCount++;
                ClassifyDeleteException(exception, ref pathAccessDenied, ref fileInUse);
                _logger.Error($"Failed to delete directory: {directory}", exception);
            }
        }

        return new ClearPathResult(bytesFreed, skippedCount, pathAccessDenied, fileInUse);
    }

    private ClearPathResult TryDeleteFile(string filePath)
    {
        try
        {
            long bytes = new FileInfo(filePath).Length;
            if (File.Exists(filePath))
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            }

            return new ClearPathResult(bytes, 0, PathAccessDenied: false, FileInUse: false);
        }
        catch (Exception exception)
        {
            bool accessDenied = false;
            bool fileInUse = false;
            ClassifyDeleteException(exception, ref accessDenied, ref fileInUse);
            _logger.Error($"Failed to delete file: {filePath}", exception);
            return new ClearPathResult(0, 1, accessDenied, fileInUse);
        }
    }

    private static void ClassifyDeleteException(Exception exception, ref bool accessDenied, ref bool fileInUse)
    {
        if (IsAccessDenied(exception))
        {
            accessDenied = true;
            return;
        }

        if (exception is IOException
            && (exception.Message.Contains("being used", StringComparison.OrdinalIgnoreCase)
                || exception.Message.Contains("in use", StringComparison.OrdinalIgnoreCase)
                || exception.HResult == unchecked((int)0x80070020)))
        {
            fileInUse = true;
        }
    }

    private static long MeasurePathBytesSafe(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }

            if (!Directory.Exists(path))
            {
                return 0;
            }

            long total = 0;
            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                try
                {
                    total += new FileInfo(file).Length;
                }
                catch
                {
                    // Ignore inaccessible files when estimating remaining size.
                }
            }

            return total;
        }
        catch
        {
            return 0;
        }
    }

    private static IEnumerable<string> EnumerateFilesSafe(string path, ref bool pathAccessDenied)
    {
        try
        {
            return Directory.GetFiles(path);
        }
        catch (Exception exception)
        {
            if (IsAccessDenied(exception))
            {
                pathAccessDenied = true;
            }

            return [];
        }
    }

    private static IEnumerable<string> EnumerateDirectoriesSafe(string path, ref bool pathAccessDenied)
    {
        try
        {
            return Directory.GetDirectories(path);
        }
        catch (Exception exception)
        {
            if (IsAccessDenied(exception))
            {
                pathAccessDenied = true;
            }

            return [];
        }
    }

    private static bool IsAccessDenied(Exception exception) =>
        exception is UnauthorizedAccessException
        || (exception is IOException ioException && ioException.Message.Contains("denied", StringComparison.OrdinalIgnoreCase));

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

    private readonly record struct ClearPathResult(
        long BytesFreed,
        int SkippedCount,
        bool PathAccessDenied,
        bool FileInUse);
}
