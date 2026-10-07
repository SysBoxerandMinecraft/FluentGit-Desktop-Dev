using Microsoft.UI.Xaml.Controls;

namespace FluentGit.Services;

/// <summary>
/// 全局提示服务。任何页面调用 Toast.XXX(...) 即可在窗口顶部弹出 InfoBar。
/// 底层由 MainWindow.ShowInfoBar 实现。
/// </summary>
public static class Toast
{
    /// <summary>成功提示（绿色）</summary>
    public static void Success(string message, string title = "成功")
        => App.MainWindow?.ShowInfoBar(title, message, InfoBarSeverity.Success);

    /// <summary>警告提示（黄色）</summary>
    public static void Warning(string message, string title = "警告")
        => App.MainWindow?.ShowInfoBar(title, message, InfoBarSeverity.Warning);

    /// <summary>错误提示（红色）</summary>
    public static void Error(string message, string title = "错误")
        => App.MainWindow?.ShowInfoBar(title, message, InfoBarSeverity.Error);

    /// <summary>普通信息提示（蓝色）</summary>
    public static void Info(string message, string title = "提示")
        => App.MainWindow?.ShowInfoBar(title, message, InfoBarSeverity.Informational);

    /// <summary>通用入口，自定义 title + severity</summary>
    public static void Show(string message, string title = "", InfoBarSeverity severity = InfoBarSeverity.Informational)
        => App.MainWindow?.ShowInfoBar(title, message, severity);
}