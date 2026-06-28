using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CDriveCleanupMaster.App.Helpers;

public static class AppBrandLogo
{
    public const string ProductName = "C盘无忧";

    private const string PackRoot = "pack://application:,,,/";

    public static ImageSource LogoImage { get; } = LoadPng("Assets/app-icon.png", decodePixelWidth: 96);

    private static ImageSource LoadPng(string relativePath, int decodePixelWidth)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = new Uri(PackRoot + relativePath, UriKind.Absolute);
        image.DecodePixelWidth = decodePixelWidth;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
