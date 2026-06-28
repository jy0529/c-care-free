using System.Windows;
using System.Windows.Media;

namespace CDriveCleanupMaster.App.Helpers;

public static class CleanupIconCatalog
{
    public static Geometry NavCleanupIcon { get; } = Parse("M9,5 H15 V7 H13 V19 H11 V7 H9 Z M7,19 H17");
    public static Geometry NavLargeFilesIcon { get; } = Parse("M5,8 H19 V18 H5 Z M8,8 V6 H16 V8 M12,11 V15 M10,13 H14");
    public static System.Windows.Media.Brush NavAccentBrush { get; } = MakeBrush("#E67E22");

    public static System.Windows.Media.Brush GetBackgroundBrush(string iconKey)
    {
        return iconKey switch
        {
            "Temp" => MakeBrush("#3498DB"),
            "Code" => MakeBrush("#9B59B6"),
            "Browser" => MakeBrush("#27AE60"),
            "Recycle" => MakeBrush("#95A5A6"),
            "Download" => MakeBrush("#E67E22"),
            "Log" => MakeBrush("#566573"),
            "Dump" => MakeBrush("#C0392B"),
            "Shader" => MakeBrush("#8E44AD"),
            "Update" => MakeBrush("#2980B9"),
            "Docker" => MakeBrush("#1ABC9C"),
            _ => MakeBrush("#5D6D7E")
        };
    }

    public static Geometry GetIconGeometry(string iconKey)
    {
        return iconKey switch
        {
            "Temp" => Parse("M4,4 H20 V8 H16 V20 H12 V8 H8 V20 H4 Z"),
            "Code" => Parse("M8,6 L4,12 L8,18 M16,6 L20,12 L16,18 M14,4 L10,20"),
            "Browser" => Parse("M4,6 H20 V18 H4 Z M4,9 H20 M8,6 V4 H16 V6"),
            "Recycle" => Parse("M9,6 H15 L13,4 H11 Z M7,8 H17 V18 H7 Z"),
            "Download" => Parse("M12,4 V14 M8,10 L12,14 L16,10 M6,18 H18"),
            "Log" => Parse("M6,4 H18 V20 H6 Z M9,8 H15 M9,12 H15 M9,16 H12"),
            "Dump" => Parse("M12,3 L20,9 V19 H4 V9 Z M12,11 V15 M10,13 H14"),
            "Shader" => Parse("M4,8 L12,4 L20,8 L12,12 Z M4,14 L12,18 L20,14 L12,10 Z"),
            "Update" => Parse("M12,4 V12 M8,8 L12,12 L16,8 M6,18 H18"),
            "Docker" => Parse("M6,10 H8 V14 H6 Z M10,8 H12 V14 H10 Z M14,10 H16 V14 H14 Z M4,16 H20"),
            _ => Parse("M6,6 H18 V18 H6 Z")
        };
    }

    private static SolidColorBrush MakeBrush(string color) =>
        new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));

    private static Geometry Parse(string data)
    {
        return Geometry.Parse(data);
    }
}
