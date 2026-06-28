using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Forms = System.Windows.Forms;
using CDriveCleanupMaster.App.Helpers;
using CDriveCleanupMaster.Application.Abstractions;
using CDriveCleanupMaster.Application.Services;
using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private const string DriveRoot = "C:\\";
    private readonly CleanupWorkspaceService _cleanupWorkspaceService;
    private readonly LargeFileWorkspaceService _largeFileWorkspaceService;
    private readonly IShellService _shellService;
    private readonly IAppLogger _logger;
    private bool _isScanning;
    private bool _isCleanupScanning;
    private bool _isLargeFileScanning;
    private bool _hasCleanupScanResults;
    private bool _hasLargeFileScanResults;
    private int _selectedNavIndex;
    private int _scanProgress;
    private string _statusText = "选择左侧功能并开始扫描";
    private string _scanPhaseText = string.Empty;
    private string _driveSummaryText = "正在读取 C: 盘空间信息...";
    private string _reclaimableText = "—";
    private string _largeFileCountText = "—";
    private string _selectedCleanupBytesText = "0 B";
    private ThresholdOptionViewModel _selectedThreshold;
    private CountLimitOptionViewModel _selectedCountLimit;
    private LargeFileItemViewModel? _selectedLargeFile;

    public MainWindowViewModel(
        CleanupWorkspaceService cleanupWorkspaceService,
        LargeFileWorkspaceService largeFileWorkspaceService,
        IShellService shellService,
        IAppLogger logger)
    {
        _cleanupWorkspaceService = cleanupWorkspaceService;
        _largeFileWorkspaceService = largeFileWorkspaceService;
        _shellService = shellService;
        _logger = logger;

        ThresholdOptions =
        [
            new ThresholdOptionViewModel(50, "50 MB"),
            new ThresholdOptionViewModel(100, "100 MB"),
            new ThresholdOptionViewModel(500, "500 MB"),
            new ThresholdOptionViewModel(1024, "1 GB")
        ];
        _selectedThreshold = ThresholdOptions[1];

        CountLimitOptions =
        [
            new CountLimitOptionViewModel(100, "100 个"),
            new CountLimitOptionViewModel(200, "200 个"),
            new CountLimitOptionViewModel(500, "500 个"),
            new CountLimitOptionViewModel(1000, "1000 个")
        ];
        _selectedCountLimit = CountLimitOptions[2];

        ScanCleanupCommand = new AsyncRelayCommand(_ => ScanCleanupAsync(), _ => !IsScanning);
        ScanLargeFilesCommand = new AsyncRelayCommand(_ => ScanLargeFilesAsync(), _ => !IsScanning);
        SelectCleanupNavCommand = new RelayCommand(_ => SelectedNavIndex = 0);
        SelectLargeFilesNavCommand = new RelayCommand(_ => SelectedNavIndex = 1);
        ExecuteCleanupCommand = new AsyncRelayCommand(_ => ExecuteCleanupAsync(), _ => HasCleanupScanResults && CleanupCategories.Any(item => item.IsSelected));
        MoveSelectedLargeFileCommand = new AsyncRelayCommand(_ => MoveSelectedLargeFileAsync(), _ => SelectedLargeFile is not null);
        OpenSelectedLargeFileLocationCommand = new RelayCommand(_ => OpenSelectedLargeFileLocation(), _ => SelectedLargeFile is not null);

        RefreshDriveSummary();
    }

    public ObservableCollection<CleanupCategoryItemViewModel> CleanupCategories { get; } = [];
    public ObservableCollection<LargeFileItemViewModel> LargeFiles { get; } = [];
    public ObservableCollection<ThresholdOptionViewModel> ThresholdOptions { get; }
    public ObservableCollection<CountLimitOptionViewModel> CountLimitOptions { get; }

    public AsyncRelayCommand ScanCleanupCommand { get; }
    public AsyncRelayCommand ScanLargeFilesCommand { get; }
    public RelayCommand SelectCleanupNavCommand { get; }
    public RelayCommand SelectLargeFilesNavCommand { get; }
    public AsyncRelayCommand ExecuteCleanupCommand { get; }
    public AsyncRelayCommand MoveSelectedLargeFileCommand { get; }
    public RelayCommand OpenSelectedLargeFileLocationCommand { get; }

    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            if (SetProperty(ref _isScanning, value))
            {
                ScanCleanupCommand.RaiseCanExecuteChanged();
                ScanLargeFilesCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasCleanupScanResults
    {
        get => _hasCleanupScanResults;
        private set
        {
            if (SetProperty(ref _hasCleanupScanResults, value))
            {
                RaisePropertyChanged(nameof(ShowCleanupList));
                ExecuteCleanupCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasLargeFileScanResults
    {
        get => _hasLargeFileScanResults;
        private set
        {
            if (SetProperty(ref _hasLargeFileScanResults, value))
            {
                RaisePropertyChanged(nameof(ShowLargeFileList));
            }
        }
    }

    public bool IsLargeFileScanning
    {
        get => _isLargeFileScanning;
        private set
        {
            if (SetProperty(ref _isLargeFileScanning, value))
            {
                RaisePropertyChanged(nameof(ShowLargeFileList));
            }
        }
    }

    public bool IsCleanupScanning
    {
        get => _isCleanupScanning;
        private set
        {
            if (SetProperty(ref _isCleanupScanning, value))
            {
                RaisePropertyChanged(nameof(ShowCleanupList));
            }
        }
    }

    public bool ShowCleanupList => HasCleanupScanResults || IsCleanupScanning;

    public bool ShowLargeFileList => HasLargeFileScanResults || IsLargeFileScanning;

    public int SelectedNavIndex
    {
        get => _selectedNavIndex;
        set
        {
            if (SetProperty(ref _selectedNavIndex, value))
            {
                RaisePropertyChanged(nameof(IsCleanupNavSelected));
                RaisePropertyChanged(nameof(IsLargeFilesNavSelected));
            }
        }
    }

    public bool IsCleanupNavSelected => SelectedNavIndex == 0;
    public bool IsLargeFilesNavSelected => SelectedNavIndex == 1;

    public string CleanupNavBadge => CleanupCategories.Count > 0 ? CleanupCategories.Count.ToString() : string.Empty;

    public string LargeFileNavBadge => LargeFiles.Count > 0 ? LargeFiles.Count.ToString() : string.Empty;

    public int ScanProgress
    {
        get => _scanProgress;
        private set => SetProperty(ref _scanProgress, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string ScanPhaseText
    {
        get => _scanPhaseText;
        private set => SetProperty(ref _scanPhaseText, value);
    }

    public string DriveSummaryText
    {
        get => _driveSummaryText;
        private set => SetProperty(ref _driveSummaryText, value);
    }

    public string ReclaimableText
    {
        get => _reclaimableText;
        private set => SetProperty(ref _reclaimableText, value);
    }

    public string LargeFileCountText
    {
        get => _largeFileCountText;
        private set => SetProperty(ref _largeFileCountText, value);
    }

    public string SelectedCleanupBytesText
    {
        get => _selectedCleanupBytesText;
        private set => SetProperty(ref _selectedCleanupBytesText, value);
    }

    public ThresholdOptionViewModel SelectedThreshold
    {
        get => _selectedThreshold;
        set => SetProperty(ref _selectedThreshold, value);
    }

    public CountLimitOptionViewModel SelectedCountLimit
    {
        get => _selectedCountLimit;
        set => SetProperty(ref _selectedCountLimit, value);
    }

    public LargeFileItemViewModel? SelectedLargeFile
    {
        get => _selectedLargeFile;
        set
        {
            if (SetProperty(ref _selectedLargeFile, value))
            {
                OpenSelectedLargeFileLocationCommand.RaiseCanExecuteChanged();
                MoveSelectedLargeFileCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public async Task ScanCleanupAsync()
    {
        try
        {
            IsScanning = true;
            IsCleanupScanning = true;
            ScanProgress = 0;
            ScanPhaseText = "正在扫描垃圾清理项...";
            StatusText = "正在扫描垃圾清理项...";

            CleanupScanResult result = await _cleanupWorkspaceService
                .ScanAsync(DriveRoot, CancellationToken.None)
                .ConfigureAwait(true);

            BindCleanupResult(result);
            HasCleanupScanResults = true;
            ScanProgress = 100;
            StatusText = "垃圾扫描完成";
            ScanPhaseText = string.Empty;
        }
        catch (Exception exception)
        {
            _logger.Error("Failed to run cleanup scan.", exception);
            StatusText = "垃圾扫描失败";
            ScanPhaseText = string.Empty;
            System.Windows.MessageBox.Show($"垃圾扫描失败：{exception.Message}", AppBrandLogo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsCleanupScanning = false;
            IsScanning = false;
        }
    }

    public async Task ScanLargeFilesAsync()
    {
        try
        {
            IsScanning = true;
            IsLargeFileScanning = true;
            ScanProgress = 0;
            ScanPhaseText = "正在扫描大文件...";
            StatusText = "正在扫描大文件...";
            LargeFiles.Clear();
            SelectedLargeFile = null;
            LargeFileCountText = "0 个";

            var options = new LargeFileScanOptions
            {
                ThresholdBytes = SelectedThreshold.Megabytes * 1024L * 1024L,
                MaxResultCount = SelectedCountLimit.Count
            };

            var progress = new Progress<ScanProgress>(report =>
            {
                void ApplyReport()
                {
                    ScanPhaseText = report.Phase;
                    ScanProgress = report.Percent;
                    if (report.LargeFileCount is not null)
                    {
                        LargeFileCountText = $"{report.LargeFileCount.Value:N0} 个";
                    }

                    if (report.LargeFilePreview is not null)
                    {
                        ApplyLargeFilePreview(report.LargeFilePreview);
                    }
                }

                if (System.Windows.Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess())
                {
                    dispatcher.Invoke(ApplyReport);
                }
                else
                {
                    ApplyReport();
                }
            });

            LargeFileScanResult result = await _largeFileWorkspaceService
                .ScanAsync([DriveRoot], options, progress, CancellationToken.None)
                .ConfigureAwait(true);

            BindLargeFiles(result);
            HasLargeFileScanResults = true;
            StatusText = "大文件扫描完成";
            ScanPhaseText = string.Empty;
        }
        catch (Exception exception)
        {
            _logger.Error("Failed to run large file scan.", exception);
            StatusText = "大文件扫描失败";
            ScanPhaseText = string.Empty;
            System.Windows.MessageBox.Show($"大文件扫描失败：{exception.Message}", AppBrandLogo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLargeFileScanning = false;
            IsScanning = false;
        }
    }

    public async Task ExecuteCleanupAsync()
    {
        try
        {
            var selectedCategories = CleanupCategories.Select(item => item.ToModel()).Where(item => item.IsSelected).ToArray();
            if (selectedCategories.Length == 0)
            {
                return;
            }

            var confirmResult = System.Windows.MessageBox.Show(
                $"将尝试清理 {selectedCategories.Length} 个类别，预计释放 {SelectedCleanupBytesText}。是否继续？",
                "确认清理",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmResult != MessageBoxResult.Yes)
            {
                return;
            }

            IsScanning = true;
            StatusText = "正在执行清理...";
            CleanupExecutionResult result = await _cleanupWorkspaceService
                .ExecuteAsync(DriveRoot, selectedCategories, CancellationToken.None)
                .ConfigureAwait(true);

            StatusText = "正在刷新清理结果...";
            await RescanCleanupOnlyAsync().ConfigureAwait(true);

            int failedCount = result.Items.Count(item => !item.Success);
            StatusText = failedCount > 0
                ? $"清理完成，释放 {ByteSizeFormatter.Format(result.TotalBytesFreed)}，{failedCount} 项未完全成功。"
                : $"清理完成，实际释放 {ByteSizeFormatter.Format(result.TotalBytesFreed)}。";

            if (failedCount > 0)
            {
                string details = string.Join(
                    Environment.NewLine,
                    result.Items
                        .Where(item => !item.Success)
                        .Select(item => $"• {item.DisplayName}：{item.ErrorMessage ?? "未完成"}"));
                System.Windows.MessageBox.Show(
                    $"部分项目未能完成：{Environment.NewLine}{Environment.NewLine}{details}",
                    "清理结果",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception exception)
        {
            _logger.Error("Failed to execute cleanup.", exception);
            StatusText = "清理失败";
            System.Windows.MessageBox.Show($"清理失败：{exception.Message}", AppBrandLogo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsScanning = false;
        }
    }

    public void OpenSelectedLargeFileLocation()
    {
        if (SelectedLargeFile is null)
        {
            return;
        }

        _shellService.OpenFileLocation(SelectedLargeFile.Model.Path);
    }

    public async Task MoveSelectedLargeFileAsync()
    {
        if (SelectedLargeFile is null)
        {
            return;
        }

        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "选择目标目录以搬移文件"
        };

        if (dialog.ShowDialog() != Forms.DialogResult.OK)
        {
            return;
        }

        try
        {
            StatusText = "正在搬移大文件...";
            string destinationPath = await _largeFileWorkspaceService.MoveAsync(
                SelectedLargeFile.Model.Path,
                dialog.SelectedPath,
                CancellationToken.None);

            StatusText = $"搬移完成：{destinationPath}";
            await RescanLargeFilesOnlyAsync();
        }
        catch (Exception exception)
        {
            _logger.Error("Failed to move large file.", exception);
            StatusText = "搬移失败";
            System.Windows.MessageBox.Show($"搬移失败：{exception.Message}", AppBrandLogo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RefreshDriveSummary()
    {
        try
        {
            var drive = new DriveInfo("C");
            if (!drive.IsReady)
            {
                DriveSummaryText = "C: 盘暂不可用";
                return;
            }

            ApplyDriveSummary(
                drive.Name,
                drive.TotalSize,
                drive.TotalSize - drive.AvailableFreeSpace,
                drive.AvailableFreeSpace);
        }
        catch (Exception exception)
        {
            _logger.Error("Failed to read C: drive info.", exception);
            DriveSummaryText = "C: 盘空间信息读取失败";
        }
    }

    private void ApplyDriveSummary(string driveName, long totalBytes, long usedBytes, long freeBytes)
    {
        DriveSummaryText =
            $"{driveName} 总共 {ByteSizeFormatter.Format(totalBytes)}，" +
            $"已用 {ByteSizeFormatter.Format(usedBytes)}，" +
            $"可用 {ByteSizeFormatter.Format(freeBytes)}";
    }

    private void RaiseNavBadges()
    {
        RaisePropertyChanged(nameof(CleanupNavBadge));
        RaisePropertyChanged(nameof(LargeFileNavBadge));
    }

    private void BindCleanupResult(CleanupScanResult cleanup)
    {
        CleanupCategories.Clear();
        foreach (var category in cleanup.Categories)
        {
            CleanupCategories.Add(new CleanupCategoryItemViewModel(category, OnSelectionChanged));
        }

        var summary = cleanup.Summary;
        ApplyDriveSummary(summary.DriveName, summary.TotalBytes, summary.UsedBytes, summary.FreeBytes);
        ReclaimableText = ByteSizeFormatter.Format(summary.ReclaimableBytes);
        UpdateSelectedCleanupBytes();
        ExecuteCleanupCommand.RaiseCanExecuteChanged();
        RaiseNavBadges();
    }

    private void BindLargeFiles(LargeFileScanResult largeFiles)
    {
        ApplyLargeFilePreview(largeFiles.Items);
        LargeFileCountText = $"{largeFiles.Items.Count:N0} 个";
    }

    private void ApplyLargeFilePreview(IReadOnlyList<LargeFileItem> items)
    {
        void Apply()
        {
            LargeFiles.Clear();
            foreach (var item in items)
            {
                LargeFiles.Add(new LargeFileItemViewModel(item));
            }

            RaiseNavBadges();
        }

        if (System.Windows.Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess())
        {
            dispatcher.Invoke(Apply);
            return;
        }

        Apply();
    }

    private async Task RescanCleanupOnlyAsync()
    {
        CleanupScanResult cleanupResult = await _cleanupWorkspaceService
            .ScanAsync(DriveRoot, CancellationToken.None)
            .ConfigureAwait(true);
        BindCleanupResult(cleanupResult);
        HasCleanupScanResults = true;
    }

    private async Task RescanLargeFilesOnlyAsync()
    {
        var options = new LargeFileScanOptions
        {
            ThresholdBytes = SelectedThreshold.Megabytes * 1024L * 1024L,
            MaxResultCount = SelectedCountLimit.Count
        };
        LargeFileScanResult largeFileResult = await _largeFileWorkspaceService
            .ScanAsync([DriveRoot], options, progress: null, CancellationToken.None)
            .ConfigureAwait(true);
        BindLargeFiles(largeFileResult);
        HasLargeFileScanResults = true;
    }

    private void OnSelectionChanged()
    {
        UpdateSelectedCleanupBytes();
        ExecuteCleanupCommand.RaiseCanExecuteChanged();
    }

    private void UpdateSelectedCleanupBytes()
    {
        SelectedCleanupBytesText = ByteSizeFormatter.Format(CleanupCategories.Where(item => item.IsSelected).Sum(item => item.TotalBytes));
    }
}
