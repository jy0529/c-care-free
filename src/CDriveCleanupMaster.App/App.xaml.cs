using System.IO;
using System.Windows;
using CDriveCleanupMaster.App.Helpers;
using CDriveCleanupMaster.App.ViewModels;
using CDriveCleanupMaster.Application.Services;
using CDriveCleanupMaster.Infrastructure.Services;

namespace CDriveCleanupMaster.App;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;

        string baseDirectory = AppContext.BaseDirectory;
        string rulePath = Path.Combine(baseDirectory, "Configuration", "cleanup-rules.json");
        var logger = new FileLogger(baseDirectory);
        var ruleProvider = new JsonCleanupRuleProvider(rulePath);
        var cleanupScanner = new FileSystemCleanupScanner(ruleProvider, logger);
        var cleanupExecutor = new FileSystemCleanupExecutor(logger);
        var largeFileScanner = new FileSystemLargeFileScanner(logger);
        var fileMover = new FileMoverService();
        var shellService = new ExplorerShellService();
        var cleanupWorkspaceService = new CleanupWorkspaceService(cleanupScanner, cleanupExecutor);
        var largeFileWorkspaceService = new LargeFileWorkspaceService(largeFileScanner, fileMover);
        var viewModel = new MainWindowViewModel(
            cleanupWorkspaceService,
            largeFileWorkspaceService,
            shellService,
            logger);

        var window = new MainWindow
        {
            DataContext = viewModel
        };

        window.Show();
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        System.Windows.MessageBox.Show(
            $"应用发生未处理错误：{e.Exception.Message}",
            AppBrandLogo.ProductName,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
