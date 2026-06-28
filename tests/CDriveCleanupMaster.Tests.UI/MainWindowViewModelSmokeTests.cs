using System.IO;
using CDriveCleanupMaster.Application.Abstractions;
using CDriveCleanupMaster.Application.Services;
using CDriveCleanupMaster.App.ViewModels;
using CDriveCleanupMaster.Domain.Enums;
using CDriveCleanupMaster.Domain.Models;
using CDriveCleanupMaster.Domain.Services;

namespace CDriveCleanupMaster.Tests.UI;

public sealed class MainWindowViewModelSmokeTests
{
    [Fact]
    public async Task ScanCleanupAsync_ShouldPopulateCleanupOnly()
    {
        var viewModel = CreateViewModel();

        await viewModel.ScanCleanupAsync();

        Assert.True(viewModel.HasCleanupScanResults);
        Assert.False(viewModel.HasLargeFileScanResults);
        Assert.Single(viewModel.CleanupCategories);
        Assert.Equal("用户临时文件", viewModel.CleanupCategories[0].DisplayName);
        Assert.Empty(viewModel.LargeFiles);
        Assert.Equal("1.00 GB", viewModel.ReclaimableText);
    }

    [Fact]
    public async Task ScanLargeFilesAsync_ShouldPopulateLargeFilesOnly()
    {
        var viewModel = CreateViewModel();

        await viewModel.ScanLargeFilesAsync();

        Assert.False(viewModel.HasCleanupScanResults);
        Assert.True(viewModel.HasLargeFileScanResults);
        Assert.Empty(viewModel.CleanupCategories);
        Assert.Single(viewModel.LargeFiles);
    }

    [Fact]
    public void Constructor_ShouldShowDriveSummaryWithoutWaitingForScan()
    {
        var viewModel = CreateViewModel();

        Assert.DoesNotContain("将在扫描后显示", viewModel.DriveSummaryText);

        if (DriveInfo.GetDrives().Any(drive => drive.Name == "C:\\" && drive.IsReady))
        {
            Assert.Contains("总共", viewModel.DriveSummaryText);
            Assert.Contains("可用", viewModel.DriveSummaryText);
        }
    }

    [Fact]
    public async Task ScanLargeFilesAsync_ShouldShowRelocationHintDuringScan()
    {
        var viewModel = CreateViewModel();

        await viewModel.ScanLargeFilesAsync();

        Assert.Contains("Docker", viewModel.LargeFiles[0].RelocationHintText, StringComparison.OrdinalIgnoreCase);
    }

    private static MainWindowViewModel CreateViewModel()
    {
        var logger = new FakeLogger();
        var cleanupService = new CleanupWorkspaceService(new FakeCleanupScanner(), new FakeCleanupExecutor());
        var largeFileService = new LargeFileWorkspaceService(new FakeLargeFileScanner(), new FakeFileMover());
        var shellService = new FakeShellService();
        return new MainWindowViewModel(cleanupService, largeFileService, shellService, logger);
    }

    private sealed class FakeCleanupScanner : ICleanupScanner
    {
        public Task<CleanupScanResult> ScanAsync(string driveRoot, CancellationToken cancellationToken)
        {
            return Task.FromResult(new CleanupScanResult
            {
                Summary = new DriveScanSummary
                {
                    DriveName = "C:\\",
                    TotalBytes = 1024L * 1024 * 1024,
                    UsedBytes = 800L * 1024 * 1024,
                    FreeBytes = 224L * 1024 * 1024,
                    ReclaimableBytes = 1024L * 1024 * 1024,
                    SelectedBytes = 1024L * 1024 * 1024
                },
                Categories =
                [
                    new CleanupCategoryResult
                    {
                        Definition = new CleanupCategoryDefinition
                        {
                            Id = "user-temp",
                            DisplayName = "用户临时文件",
                            Description = "temp",
                            GroupName = "系统",
                            IconKey = "Temp",
                            RiskLevel = RiskLevel.Low,
                            IsExecutable = true,
                            IsSelectedByDefault = true,
                            Paths = ["C:\\Temp"]
                        },
                        TotalBytes = 1024L * 1024 * 1024,
                        FileCount = 2,
                        ResolvedPaths = ["C:\\Temp"],
                        IsSelected = true
                    }
                ],
                ScannedAt = DateTimeOffset.Now
            });
        }
    }

    private sealed class FakeCleanupExecutor : ICleanupExecutor
    {
        public Task<CleanupExecutionResult> ExecuteAsync(CleanupExecutionRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new CleanupExecutionResult
            {
                TotalBytesFreed = 0,
                Items = []
            });
        }
    }

    private sealed class FakeLargeFileScanner : ILargeFileScanner
    {
        public Task<LargeFileScanResult> ScanAsync(
            IReadOnlyList<string> roots,
            LargeFileScanOptions options,
            IProgress<ScanProgress>? progress,
            CancellationToken cancellationToken)
        {
            var items = new[]
            {
                new LargeFileItem
                {
                    Path = @"C:\Docker\wsl\data\ext4.vhdx",
                    Name = "ext4.vhdx",
                    Extension = ".vhdx",
                    Bytes = 2L * 1024 * 1024 * 1024,
                    ModifiedAt = DateTimeOffset.Now
                }
            };

            var preview = LargeFileRelocationMatcher.ApplyHints(items);
            progress?.Report(new ScanProgress
            {
                Phase = "正在扫描大文件...",
                Percent = 50,
                LargeFileCount = preview.Count,
                LargeFilePreview = preview
            });

            return Task.FromResult(new LargeFileScanResult
            {
                Items = preview,
                ThresholdBytes = options.ThresholdBytes,
                MaxResultCount = options.MaxResultCount,
                ScannedAt = DateTimeOffset.Now
            });
        }
    }

    private sealed class FakeFileMover : IFileMover
    {
        public Task<string> MoveAsync(string sourcePath, string destinationDirectory, CancellationToken cancellationToken)
        {
            return Task.FromResult(sourcePath);
        }
    }

    private sealed class FakeShellService : IShellService
    {
        public void OpenFileLocation(string path) { }
    }

    private sealed class FakeLogger : IAppLogger
    {
        public void Error(string message, Exception exception) { }
        public void Info(string message) { }
    }
}
