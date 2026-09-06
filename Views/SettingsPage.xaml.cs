using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentGit.Services;

namespace FluentGit.Views;

public sealed partial class SettingsPage : Page
{
    private CancellationTokenSource? _infoBarCts;

    // InfoBar 显示时间（毫秒）—— 3 秒
    private const int InfoBarDisplayMilliseconds = 3000;

    // 预期 Git.exe 的 SHA-256 哈希值（小写）
    private const string ExpectedGitHash = "c470d205517c7a53ceca321df16a6e4549fcd52b576ab4d09536d36f26fda5a9";

    public SettingsPage()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            var settings = SettingsService.Load();

            // Git 路径加载
            string? gitPath = settings.GitPath;
            if (string.IsNullOrEmpty(gitPath))
            {
                gitPath = GitPathHelper.FindGitPath();
                if (!string.IsNullOrEmpty(gitPath) && GitPathHelper.ValidateGitPath(gitPath, ExpectedGitHash))
                {
                    settings.GitPath = gitPath;
                    settings.GitHash = GitPathHelper.ComputeFileHash(gitPath);
                    SettingsService.Save(settings);
                }
                else
                {
                    gitPath = null;
                }
            }

            if (!string.IsNullOrEmpty(gitPath))
            {
                GitPathTextBox.Text = gitPath;
                bool valid = GitPathHelper.ValidateGitPath(gitPath, ExpectedGitHash);
                if (valid)
                    ShowInfoBar("成功", "Git 路径有效", InfoBarSeverity.Success);
                else
                    ShowInfoBar("警告", "Git 路径无效或哈希不匹配，请重新设置", InfoBarSeverity.Warning);
                UpdateSelectedPathDisplay(gitPath);
            }
            else
            {
                ShowInfoBar("提示", "未设置 Git 路径", InfoBarSeverity.Informational);
                UpdateSelectedPathDisplay("");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载设置异常: {ex.Message}");
        }
    }

    private void OnBrowseGitPath(object sender, RoutedEventArgs e)
    {
        try
        {
            var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
            filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            filePicker.FileTypeFilter.Add(".exe");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            var file = filePicker.PickSingleFileAsync().GetAwaiter().GetResult();
            if (file != null && file.Name.Equals("git.exe", StringComparison.OrdinalIgnoreCase))
            {
                if (GitPathHelper.ValidateGitPath(file.Path, ExpectedGitHash))
                {
                    GitPathTextBox.Text = file.Path;
                    ShowInfoBar("成功", "Git 路径有效", InfoBarSeverity.Success);
                    UpdateSelectedPathDisplay(file.Path);
                }
                else
                {
                    ShowInfoBar("错误", "选择的 git.exe 哈希值不匹配，请确保来自官方 Git", InfoBarSeverity.Error);
                    UpdateSelectedPathDisplay(file.Path);
                }
            }
            else if (file != null)
            {
                ShowInfoBar("错误", "请选择 git.exe 文件", InfoBarSeverity.Error);
                UpdateSelectedPathDisplay(file.Path);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"浏览异常: {ex.Message}");
            ShowInfoBar("错误", $"浏览异常: {ex.Message}", InfoBarSeverity.Error);
        }
    }

    private void OnSaveGitPath(object sender, RoutedEventArgs e)
    {
        try
        {
            string path = GitPathTextBox.Text.Trim();
            if (string.IsNullOrEmpty(path))
            {
                ShowInfoBar("错误", "路径不能为空", InfoBarSeverity.Error);
                return;
            }
            if (!GitPathHelper.ValidateGitPath(path, ExpectedGitHash))
            {
                ShowInfoBar("错误", "无效的 git.exe，哈希值不匹配", InfoBarSeverity.Error);
                return;
            }
            var settings = SettingsService.Load();
            settings.GitPath = path;
            settings.GitHash = GitPathHelper.ComputeFileHash(path);
            SettingsService.Save(settings);
            ShowInfoBar("成功", "保存成功", InfoBarSeverity.Success);
            UpdateSelectedPathDisplay(path);
        }
        catch (Exception ex)
        {
            ShowInfoBar("错误", $"保存异常: {ex.Message}", InfoBarSeverity.Error);
        }
    }

    private void OnAutoFindGitPath(object sender, RoutedEventArgs e)
    {
        var gitPath = GitPathHelper.FindGitPath();
        if (!string.IsNullOrEmpty(gitPath) && GitPathHelper.ValidateGitPath(gitPath, ExpectedGitHash))
        {
            GitPathTextBox.Text = gitPath;
            UpdateSelectedPathDisplay(gitPath);
            var settings = SettingsService.Load();
            settings.GitPath = gitPath;
            settings.GitHash = GitPathHelper.ComputeFileHash(gitPath);
            SettingsService.Save(settings);
            ShowInfoBar("成功", "自动找到并保存 Git 路径", InfoBarSeverity.Success);
        }
        else
        {
            ShowInfoBar("错误", "未找到有效的 Git，请手动指定路径", InfoBarSeverity.Error);
        }
    }

    private void UpdateSelectedPathDisplay(string path)
    {
        SelectedPathDisplay.Text = string.IsNullOrEmpty(path) ? "当前选中路径：无" : $"当前选中路径：{path}";
    }

    // InfoBar 显示方法（动画 + 3秒自动消失）
    private async void ShowInfoBar(string title, string message, InfoBarSeverity severity)
    {
        _infoBarCts?.Cancel();
        _infoBarCts = new CancellationTokenSource();
        var token = _infoBarCts.Token;

        SlideInStoryboard.Stop();
        SlideOutStoryboard.Stop();

        StatusInfoBar.Title = title;
        StatusInfoBar.Message = message;
        StatusInfoBar.Severity = severity;

        // 复位到初始状态（透明、左侧外）
        InfoBarTransform.X = -400;
        InfoBarContainer.Opacity = 0;

        SlideInStoryboard.Begin();

        try
        {
            await Task.Delay(InfoBarDisplayMilliseconds, token);
            SlideOutStoryboard.Begin();
            await Task.Delay(200);
            InfoBarContainer.Opacity = 0;
            InfoBarTransform.X = -400;
        }
        catch (TaskCanceledException)
        {
            InfoBarContainer.Opacity = 0;
            InfoBarTransform.X = -400;
            SlideInStoryboard.Stop();
            SlideOutStoryboard.Stop();
        }
    }
}