using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using System.Reflection;
using FluentGit.Views; 

namespace FluentGit;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 直接设置标题栏扩展（标准做法，确保性能稳定）
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        // 设置窗口图标（任务栏、Alt+Tab）
        string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string iconPath = Path.Combine(appDir, "Assets", "AppIcon.ico");
        AppWindow.SetIcon(iconPath);

        // 主题和导航
        RootFrame.RequestedTheme = ElementTheme.Default;
        RootFrame.Navigate(typeof(MainPage));
    }
}