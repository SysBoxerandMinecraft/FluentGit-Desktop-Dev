using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using FluentGit.Services;
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

        var settings = SettingsService.Load();
        var isDark = settings.AppTheme == "Dark";
        ThemeToggle.IsOn = isDark;
        FollowSystemCheckBox.IsChecked = string.IsNullOrEmpty(settings.AppTheme);

        _isUpdating = false;
    }

    private void OnThemeToggled(object sender, RoutedEventArgs e)
    {
        if (_isUpdating) return;

        var newTheme = ThemeToggle.IsOn ? ElementTheme.Dark : ElementTheme.Light;
        ApplyTheme(newTheme);

        // 手动切换主题时，取消「跟随系统」
        _isUpdating = true;
        FollowSystemCheckBox.IsChecked = false;
        _isUpdating = false;

        var settings = SettingsService.Load();
        settings.AppTheme = newTheme.ToString();
        SettingsService.Save(settings);
    }

    private void OnFollowSystemChecked(object sender, RoutedEventArgs e)
    {
        if (_isUpdating) return;

        ApplyTheme(ElementTheme.Default);

        var settings = SettingsService.Load();
        settings.AppTheme = null;   // null = 跟随系统
        SettingsService.Save(settings);
    }

    private void OnFollowSystemUnchecked(object sender, RoutedEventArgs e) { }

    private void ApplyTheme(ElementTheme theme)
    {
        // 作用于窗口根元素，所有 Frame 都会继承
        if (App.MainWindow?.Content is FrameworkElement root)
        {
            root.RequestedTheme = theme;
        }
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