using System.Runtime.InteropServices;

namespace CDriveCleanupMaster.Infrastructure.Services;

internal static class WindowsRecycleBinService
{
    private const uint SherbNoConfirmation = 0x00000001;
    private const uint SherbNoProgressUi = 0x00000002;
    private const uint SherbNoSound = 0x00000004;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    public static void EmptyDrive(string driveRoot)
    {
        if (string.IsNullOrWhiteSpace(driveRoot))
        {
            throw new ArgumentException("Drive root is required.", nameof(driveRoot));
        }

        string rootPath = driveRoot.EndsWith('\\') ? driveRoot : driveRoot + "\\";
        int result = SHEmptyRecycleBin(IntPtr.Zero, rootPath, SherbNoConfirmation | SherbNoProgressUi | SherbNoSound);
        if (result < 0)
        {
            throw new IOException($"清空回收站失败，错误码 0x{result:X8}。");
        }
    }
}
