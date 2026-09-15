using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;

namespace FluentGit;

public partial class App : Application
{
    private Window? _window;
    public static MainWindow? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();

        // ★ 捕获 UI 线程未处理异常
        this.UnhandledException += OnUnhandledException;

        // ★ 捕获后台任务未处理异常
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Debug.WriteLine($"[AppDomain] {e.ExceptionObject}");
        };

        // ★ 捕获 Task 里的未处理异常
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Debug.WriteLine($"[Task] {e.Exception}");
            e.SetObserved();
        };
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Debug.WriteLine("========== UnhandledException ==========");
        Debug.WriteLine(e.Message);
        Debug.WriteLine(e.Exception?.ToString());
        Debug.WriteLine("========================================");
        // 暂不设置 e.Handled = true，让调试器能抓到
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        MainWindow = _window as MainWindow;
        _window.Activate();
    }
}