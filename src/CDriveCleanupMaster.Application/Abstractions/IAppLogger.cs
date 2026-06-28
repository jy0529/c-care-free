namespace CDriveCleanupMaster.Application.Abstractions;

/// <summary>
/// 日志抽象。
/// </summary>
public interface IAppLogger
{
    void Info(string message);
    void Error(string message, Exception exception);
}
