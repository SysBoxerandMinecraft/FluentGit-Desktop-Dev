// Copyright (c) 2026 SysBoxerandMinecraft. Licensed under the MIT License.
using FluentGit.Services;
using FluentGit.Views;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.IO;
using System.Reflection;

namespace FluentGit;

public sealed partial class MainWindow : Window
{
    private const string TAG = "MainWindow";

    public MainWindow()
    {
        /*
        *                       _ooOoo_
        *                      o8888888o
        *                      88" . "88
        *                      (| -_- |)
        *                      O\  =  /O
        *                   ____/`---'\____
        *                 .'  \\|     |//  `.
        *                /  \\|||  :  |||//  \
        *               /  _||||| -:- |||||-  \
        *               |   | \\\  -  /// |   |
        *               | \_|  ''\---/''  |   |
        *               \  .-\__  `-`  ___/-. /
        *             ___`. .'  /--.--\  `. . __
        *          ."" '<  `.___\_<|>_/___.'  >'"".
        *         | | :  `- \`.;`\ _ /`;.`/ - ` : | |
        *         \  \ `-.   \_ __\ /__ _/   .-` /  /
        *    ======`-.____`-.___\_____/___.-`____.-'======
        *                       `=---='
        *   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        *             佛祖保佑            永无BUG
         */

        InitializeComponent();
        InitializeBackdrop();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string iconPath = Path.Combine(appDir, "Assets", "AppIcon.ico");
        AppWindow.SetIcon(iconPath);

        // 主题：从设置读
        ApplyThemeFromSettings();

        // 返回按钮可见性 + 左侧导航选中项同步
        NavFrame.Navigated += OnNavFrameNavigated;

        NavFrame.Navigate(typeof(RepoPage));
    }

    // ========== 导航事件：同步返回按钮 + 左侧选中项 ==========
    private void OnNavFrameNavigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs args)
    {
        AppTitleBar.IsBackButtonVisible = NavFrame.CanGoBack;

        string? tag = args.SourcePageType switch
        {
            var t when t == typeof(RepoPage)     => "repo",
            var t when t == typeof(FilePage)     => "file",
            var t when t == typeof(HistoryPage)  => "history",
            var t when t == typeof(ClonePage)    => "clone",
            var t when t == typeof(SettingsPage) => "settings",
            _ => null
        };

        if (tag != null)
            SelectNavItem(tag);
    }

    // ========== 根据 tag 选中左侧导航项 ==========
    private void SelectNavItem(string tag)
    {
        if (NavView.MenuItems != null)
        {
            foreach (var obj in NavView.MenuItems)
            {
                if (obj is NavigationViewItem item && (item.Tag as string) == tag)
                {
                    NavView.SelectedItem = item;
                    return;
                }
            }
        }

        if (NavView.FooterMenuItems != null)
        {
            foreach (var obj in NavView.FooterMenuItems)
            {
                if (obj is NavigationViewItem item && (item.Tag as string) == tag)
                {
                    NavView.SelectedItem = item;
                    return;
                }
            }
        }
    }

    // ========== Mica / Acrylic / 回退 ==========
    private void InitializeBackdrop()
    {
        var settings = SettingsService.Load();
        ApplyBackdrop(settings.BackdropType ?? "Default");
    }

    /// <summary>
    /// 应用窗口背景效果。可在运行时调用，动态切换。
    /// </summary>
    public void ApplyBackdrop(string backdropType)
    {
        try
        {
            switch (backdropType)
            {
                case "Mica":
                    SystemBackdrop = MicaController.IsSupported()
                        ? new MicaBackdrop { Kind = MicaKind.Base }
                        : null;
                    AppLogger.OK(TAG, $"Backdrop: Mica (supported={MicaController.IsSupported()})");
                    break;

                case "MicaAlt":
                    SystemBackdrop = MicaController.IsSupported()
                        ? new MicaBackdrop { Kind = MicaKind.BaseAlt }
                        : null;
                    AppLogger.OK(TAG, $"Backdrop: MicaAlt (supported={MicaController.IsSupported()})");
                    break;

                case "Acrylic":
                    SystemBackdrop = DesktopAcrylicController.IsSupported()
                        ? new DesktopAcrylicBackdrop()
                        : null;
                    AppLogger.OK(TAG, $"Backdrop: Acrylic (supported={DesktopAcrylicController.IsSupported()})");
                    break;

                case "None":
                    SystemBackdrop = null;
                    AppLogger.OK(TAG, "Backdrop: None (solid color)");
                    break;

                case "Default":
                default:
                    SystemBackdrop = MicaController.IsSupported()
                        ? new MicaBackdrop()
                        : null;
                    AppLogger.OK(TAG, $"Backdrop: Default (Mica={MicaController.IsSupported()})");
                    break;
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"ApplyBackdrop 异常: {ex.Message}");
            SystemBackdrop = null;
        }
    }

    // ========== 主题 ==========
    private void ApplyThemeFromSettings()
    {
        try
        {
            var settings = SettingsService.Load();
            var theme = settings.AppTheme switch
            {
                "Dark"  => ElementTheme.Dark,
                "Light" => ElementTheme.Light,
                _       => ElementTheme.Default
            };

            if (Content is FrameworkElement root)
                root.RequestedTheme = theme;

            AppLogger.Info(TAG, $"主题已应用: {theme}");
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"ApplyThemeFromSettings 异常: {ex.Message}");
        }
    }

    // ========== 标题栏事件 ==========
    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        if (NavFrame.CanGoBack)
            NavFrame.GoBack();
    }

    // ========== 导航 ==========
    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "repo":
                    NavFrame.Navigate(typeof(RepoPage));
                    break;
                case "file":
                    NavFrame.Navigate(typeof(FilePage));
                    break;
                case "history":
                    NavFrame.Navigate(typeof(HistoryPage));
                    break;
                case "clone":
                    NavFrame.Navigate(typeof(ClonePage));
                    break;
                case "settings":
                    NavFrame.Navigate(typeof(SettingsPage));
                    break;
                default:
                    AppLogger.Warning(TAG, $"未知导航项: {item.Tag}");
                    break;
            }
        }
    }

    // ========== 点击事件：设置齿轮旋转 ==========
    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is NavigationViewItem item &&
            (item.Tag as string) == "settings")
        {
            RotateSettingsIcon();
        }
    }

    private void RotateSettingsIcon()
    {
        try
        {
            var anim = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(anim, SettingsIconRotation);
            Storyboard.SetTargetProperty(anim, "Angle");

            var sb = new Storyboard();
            sb.Children.Add(anim);
            sb.Completed += (_, _) => SettingsIconRotation.Angle = 0;

            sb.Begin();
        }
        catch (Exception ex)
        {
            AppLogger.Warning(TAG, $"齿轮旋转动画失败: {ex.Message}");
        }
    }
}