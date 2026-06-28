namespace CDriveCleanupMaster.Domain.Services;

public static class ScanPathSkipPolicy
{
    private static readonly HashSet<string> SkipDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "$Recycle.Bin",
        "System Volume Information",
        "$WinREAgent",
        "Recovery",
        "Config.Msi"
    };

    private static readonly string[] SkipPathFragments =
    [
        @"\Windows\WinSxS\",
        @"\Windows\Installer\",
        @"\Windows\SoftwareDistribution\Download\"
    ];

    public static bool ShouldSkipDirectory(string directoryPath)
    {
        string name = Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (SkipDirectoryNames.Contains(name))
        {
            return true;
        }

        string normalized = directoryPath.Replace('/', '\\');
        if (!normalized.EndsWith("\\", StringComparison.Ordinal))
        {
            normalized += '\\';
        }

        return SkipPathFragments.Any(fragment =>
            normalized.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}
