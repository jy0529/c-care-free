namespace CDriveCleanupMaster.App.ViewModels;

public static class ByteSizeFormatter
{
    public static string Format(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        int index = 0;
        while (value >= 1024 && index < units.Length - 1)
        {
            value /= 1024;
            index++;
        }

        return $"{value:F2} {units[index]}";
    }
}
