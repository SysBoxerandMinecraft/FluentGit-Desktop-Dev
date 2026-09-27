// Copyright (c) 2026 SysBoxerandMinecraft. Licensed under the MIT License.
using FluentGit.Services;
using FluentGit.Views;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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

        // 返回按钮可见性：Frame.CanGoBack 不是依赖属性，x:Bind 不生效，手动维护
        NavFrame.Navigated += (_, _) =>
        {
            AppTitleBar.IsBackButtonVisible = NavFrame.CanGoBack;
        };

        NavFrame.Navigate(typeof(RepoPage));
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
                case "admin":
                    NavFrame.Navigate(typeof(AdminPage));
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
}