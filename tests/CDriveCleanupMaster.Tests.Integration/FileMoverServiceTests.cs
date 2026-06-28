using CDriveCleanupMaster.Infrastructure.Services;

namespace CDriveCleanupMaster.Tests.Integration;

public sealed class FileMoverServiceTests
{
    [Fact]
    public async Task MoveAsync_ShouldMoveFile_WhenDestinationIsAvailable()
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string sourceDir = Path.Combine(tempRoot, "source");
        string destinationDir = Path.Combine(tempRoot, "dest");
        Directory.CreateDirectory(sourceDir);
        Directory.CreateDirectory(destinationDir);
        string sourceFile = Path.Combine(sourceDir, "demo.txt");
        await File.WriteAllTextAsync(sourceFile, "hello");

        try
        {
            var service = new FileMoverService();
            string destinationPath = await service.MoveAsync(sourceFile, destinationDir, CancellationToken.None);

            Assert.True(File.Exists(destinationPath));
            Assert.False(File.Exists(sourceFile));
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }
}
