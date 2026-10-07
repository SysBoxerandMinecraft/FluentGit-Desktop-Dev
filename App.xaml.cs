// Copyright (c) 2026 SysBoxerandMinecraft. Licensed under the MIT License.
using Microsoft.UI.Xaml;
using System;
using FluentGit.Services;

namespace FluentGit;

public partial class App : Application
{
    private Window? _window;
    public static MainWindow? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();

        UnhandledException += OnUnhandledException;

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            CrashLogger.Log("AppDomain", e.ExceptionObject as Exception);
        };

        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            CrashLogger.Log("TaskScheduler", e.Exception);
            e.SetObserved();
        };
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // 过滤 WinUI 3 已知问题：Popup 上下文里 SymbolThemeFontFamily 解析失败
        if (e.Exception is ArgumentException argEx &&
            argEx.Message.Contains("FontFamily", StringComparison.OrdinalIgnoreCase))
        {
            AppLogger.Warning("App", $"忽略已知的 FontFamily 异常: {e.Message}");
            e.Handled = true;
            return;
        }

        CrashLogger.Log("UI", e.Exception);
        AppLogger.Error("App", $"未处理异常: {e.Message}");

        if (e.Exception is OutOfMemoryException or StackOverflowException)
        {
            e.Handled = false;
        }
        else
        {
            e.Handled = true;
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = new MainWindow();
        MainWindow = window;
        _window = window;
        window.Activate();
    }
}