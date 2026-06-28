using CDriveCleanupMaster.Application.Abstractions;
using CDriveCleanupMaster.Domain.Enums;
using CDriveCleanupMaster.Domain.Models;
using CDriveCleanupMaster.Infrastructure.Services;

namespace CDriveCleanupMaster.Tests.Integration;

public sealed class FileSystemCleanupScannerTests
{
    [Fact]
    public async Task ScanAsync_ShouldReturnMetrics_ForExistingDirectory()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string filePath = Path.Combine(root, "sample.bin");
        await File.WriteAllBytesAsync(filePath, new byte[1024]);

        try
        {
            var logger = new FakeLogger();
            var provider = new FakeRuleProvider(root);
            var scanner = new FileSystemCleanupScanner(provider, logger);
            CleanupScanResult result = await scanner.ScanAsync("C:\\", CancellationToken.None);

            Assert.Single(result.Categories);
            Assert.Equal(1024, result.Categories[0].TotalBytes);
            Assert.Equal(1, result.Categories[0].FileCount);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private sealed class FakeRuleProvider : ICleanupRuleProvider
    {
        private readonly string _path;

        public FakeRuleProvider(string path)
        {
            _path = path;
        }

        public Task<IReadOnlyList<CleanupCategoryDefinition>> LoadDefinitionsAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<CleanupCategoryDefinition> definitions =
            [
                new CleanupCategoryDefinition
                {
                    Id = "temp",
                    DisplayName = "Temp",
                    Description = "Temp",
                    GroupName = "G",
                    IconKey = "T",
                    RiskLevel = RiskLevel.Low,
                    IsExecutable = true,
                    IsSelectedByDefault = true,
                    Paths = [_path]
                }
            ];

            return Task.FromResult(definitions);
        }
    }

    private sealed class FakeLogger : IAppLogger
    {
        public void Error(string message, Exception exception) { }
        public void Info(string message) { }
    }
}
