namespace CDriveCleanupMaster.Application.Abstractions;

/// <summary>
/// 资源管理器集成接口。
/// </summary>
public interface IShellService
{
    void OpenFileLocation(string path);
}
