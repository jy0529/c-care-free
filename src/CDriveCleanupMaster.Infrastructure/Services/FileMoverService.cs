using CDriveCleanupMaster.Application.Abstractions;

namespace CDriveCleanupMaster.Infrastructure.Services;

/// <summary>
/// 文件搬移服务。
/// </summary>
public sealed class FileMoverService : IFileMover
{
    public Task<string> MoveAsync(string sourcePath, string destinationDirectory, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Source file not found.", sourcePath);
        }

        Directory.CreateDirectory(destinationDirectory);
        string destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(sourcePath));
        if (File.Exists(destinationPath))
        {
            throw new IOException("Destination file already exists.");
        }

        File.Move(sourcePath, destinationPath);
        return Task.FromResult(destinationPath);
    }
}
