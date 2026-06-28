using System.Windows;
using System.Windows.Controls;

namespace CDriveCleanupMaster.App.Controls;

public partial class AppLogoControl : System.Windows.Controls.UserControl
{
    public static readonly DependencyProperty LogoSizeProperty =
        DependencyProperty.Register(nameof(LogoSize), typeof(double), typeof(AppLogoControl), new PropertyMetadata(42.0));

    public AppLogoControl()
    {
        InitializeComponent();
    }

    public double LogoSize
    {
        get => (double)GetValue(LogoSizeProperty);
        set => SetValue(LogoSizeProperty, value);
    }
}
