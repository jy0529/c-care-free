namespace CDriveCleanupMaster.Application.Abstractions;

/// <summary>
/// 文件搬移接口。
/// </summary>
public interface IFileMover
{
    Task<string> MoveAsync(string sourcePath, string destinationDirectory, CancellationToken cancellationToken);
}
