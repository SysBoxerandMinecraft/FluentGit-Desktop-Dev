using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;

namespace FluentGit.Views;

public sealed partial class PlaceholderPage : Page
{
    private bool _isUpdating = false;

    public PlaceholderPage()
    {
        InitializeComponent();
        ThemeToggle.Toggled += OnThemeToggled;
        FollowSystemCheckBox.Checked += OnFollowSystemChecked;
        FollowSystemCheckBox.Unchecked += OnFollowSystemUnchecked;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is string title)
            TitleText.Text = title;

        _isUpdating = true;
        var frame = App.MainWindow?.RootFrame;
        if (frame != null)
        {
            ThemeToggle.IsOn = (frame.RequestedTheme == ElementTheme.Dark);
        }
        FollowSystemCheckBox.IsChecked = false;
        _isUpdating = false;
    }

    private void OnThemeToggled(object sender, RoutedEventArgs e)
    {
        if (_isUpdating) return;
        var frame = App.MainWindow?.RootFrame;
        if (frame == null) return;

        var newTheme = ThemeToggle.IsOn ? ElementTheme.Dark : ElementTheme.Light;
        if (frame.RequestedTheme != newTheme)
            frame.RequestedTheme = newTheme;
    }

    private void OnFollowSystemChecked(object sender, RoutedEventArgs e)
    {
        if (_isUpdating) return;
        var frame = App.MainWindow?.RootFrame;
        if (frame == null) return;

        bool systemIsDark = IsSystemDarkTheme();
        var newTheme = systemIsDark ? ElementTheme.Dark : ElementTheme.Light;
        if (frame.RequestedTheme != newTheme)
        {
            _isUpdating = true;
            frame.RequestedTheme = newTheme;
            ThemeToggle.IsOn = systemIsDark;
            _isUpdating = false;
        }
    }

    private void OnFollowSystemUnchecked(object sender, RoutedEventArgs e)
    {
        // 不操作
    }

    private bool IsSystemDarkTheme()
    {
        try
        {
            var uiSettings = new UISettings();
            var color = uiSettings.GetColorValue(UIColorType.Background);
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
            return luminance < 0.5;
        }
        catch { return false; }
    }
}