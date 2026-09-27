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

        // UI 线程未处理异常
        UnhandledException += OnUnhandledException;

        // 后台线程未处理异常（通常意味着进程即将终止，只能记录）
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            CrashLogger.Log("AppDomain", e.ExceptionObject as Exception);
        };

        // Task 未观察异常（吞掉，避免进程被提升为致命异常）
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            CrashLogger.Log("TaskScheduler", e.Exception);
            e.SetObserved();
        };
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        CrashLogger.Log("UI", e.Exception);
        AppLogger.Error("App", $"未处理异常: {e.Message}");

        // 对真正无法恢复的异常，让进程去死
        // 其余（UI 事件处理器里的 async void 抛出等）标记为已处理，避免整个应用崩掉
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