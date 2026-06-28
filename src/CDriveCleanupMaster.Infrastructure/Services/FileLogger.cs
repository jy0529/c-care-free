using System.Text;
using CDriveCleanupMaster.Application.Abstractions;

namespace CDriveCleanupMaster.Infrastructure.Services;

/// <summary>
/// 文件日志实现。
/// </summary>
public sealed class FileLogger : IAppLogger
{
    private readonly string _logDirectory;
    private readonly object _syncRoot = new();

    public FileLogger(string baseDirectory)
    {
        _logDirectory = Path.Combine(baseDirectory, "logs");
        Directory.CreateDirectory(_logDirectory);
    }

    public void Info(string message)
    {
        Write("INFO", message, null);
    }

    public void Error(string message, Exception exception)
    {
        Write("ERROR", message, exception);
    }

    private void Write(string level, string message, Exception? exception)
    {
        string filePath = Path.Combine(_logDirectory, $"app-{DateTime.UtcNow:yyyyMMdd}.log");
        var builder = new StringBuilder();
        builder.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
        if (exception is not null)
        {
            builder.AppendLine(exception.ToString());
        }

        lock (_syncRoot)
        {
            File.AppendAllText(filePath, builder.ToString() + Environment.NewLine, Encoding.UTF8);
        }
    }
}
