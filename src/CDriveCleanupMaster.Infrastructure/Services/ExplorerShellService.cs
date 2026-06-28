using System.Diagnostics;
using CDriveCleanupMaster.Application.Abstractions;

namespace CDriveCleanupMaster.Infrastructure.Services;

/// <summary>
/// 打开系统资源管理器。
/// </summary>
public sealed class ExplorerShellService : IShellService
{
    public void OpenFileLocation(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        string arguments = File.Exists(path)
            ? $"/select,\"{path}\""
            : $"\"{path}\"";

        Process.Start(new ProcessStartInfo("explorer.exe", arguments)
        {
            UseShellExecute = true
        });
    }
}
