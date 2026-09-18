using FluentGit.Services;
using Microsoft.UI.Xaml.Media;   // ★ 加这行
using FluentGit.Views;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
         *          _ooOoo_
         *         o8888888o
         *         88" . "88
         *         (| -_- |)
         *         O\  =  /O
         *      ____/`---'\____
         *    .'  \\|     |//  `.
         *   /  \\|||  :  |||//  \
         *  /  _||||| -:- |||||-  \
         *  |   | \\\  -  /// |   |
         *  | \_|  ''\---/''  |   |
         *  \  .-\__  `-`  ___/-. /
         * ___`. .'  /--.--\  `. . ___
         * ."" '<  `.___\_<|>_/___.'  >'"".
         * | | :  `- \`.;`\ _ /`;.`/ - ` : | |
         * \  \ `-.   \_ __\ /__ _/   .-` /  /
         * ======`-.____`-.___\_____/___.-`____.-'======
         *                    `=---='
         * ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
         *               佛祖保佑   永无BUG
         */

        InitializeComponent();
        InitializeBackdrop();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string iconPath = Path.Combine(appDir, "Assets", "AppIcon.ico");
        AppWindow.SetIcon(iconPath);

        NavFrame.RequestedTheme = ElementTheme.Default;
        NavFrame.Navigate(typeof(RepoPage));
    }

    // ========== Mica / 回退 ==========
    private void InitializeBackdrop()
    {
        try
        {
            if (MicaController.IsSupported())
            {
                SystemBackdrop = new MicaBackdrop();
                AppLogger.OK(TAG, "Mica backdrop enabled.");
            }
            else
            {
                SystemBackdrop = null;
                AppLogger.Info(TAG, "Mica not supported. Falling back to default background.");
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"InitializeBackdrop 异常: {ex.Message}");
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
                    throw new InvalidOperationException($"Unknown navigation item tag: {item.Tag}");
            }
        }
    }
}