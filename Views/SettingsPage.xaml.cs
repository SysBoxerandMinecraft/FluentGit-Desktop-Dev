using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentGit.Services;

namespace FluentGit.Views;

public sealed partial class SettingsPage : Page
{
    private const string TAG = "SettingsPage";
    private CancellationTokenSource? _infoBarCts;
    private const int InfoBarDisplayMilliseconds = 3000;

    // 防止 OnLoaded 初始化 checkbox 时触发事件
    private bool _isUpdating = false;

    public SettingsPage()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // ★ 初始化 checkbox 状态
        _isUpdating = true;
        FilterArtifactsCheckBox.IsChecked = SettingsService.Load().FilterBuildArtifacts;
        _isUpdating = false;

        await AutoFindAndValidateGitAsync();
    }

    // ========== 过滤选项事件 ==========
    private void OnFilterArtifactsChecked(object sender, RoutedEventArgs e)
    {
        if (_isUpdating) return;
        var settings = SettingsService.Load();
        settings.FilterBuildArtifacts = true;
        SettingsService.Save(settings);
        AppLogger.OK(TAG, "过滤编译产物：已开启");
    }

    private void OnFilterArtifactsUnchecked(object sender, RoutedEventArgs e)
    {
        if (_isUpdating) return;
        var settings = SettingsService.Load();
        settings.FilterBuildArtifacts = false;
        SettingsService.Save(settings);
        AppLogger.OK(TAG, "过滤编译产物：已关闭");
    }

    // ========== Git 自动查找 ==========
    private async Task AutoFindAndValidateGitAsync()
    {
        LoadingRing.IsActive = true;

        try
        {
            var settings = SettingsService.Load();
            string? gitPath = settings.GitPath;

            if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
            {
                await Task.Delay(100);
                gitPath = await Task.Run(() => GitPathHelper.FindGitPath());
            }

            if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
            {
                UpdateSelectedPathDisplay("");
                AppLogger.Warning(TAG, "未找到 Git");
                ShowInfoBar("警告", "未找到 Git，请先安装 Git for Windows", InfoBarSeverity.Warning);
                return;
            }

            if (!GitPathHelper.ValidateGitPath(gitPath, GitPathHelper.ExpectedGitHash))
            {
                var actualHash = GitPathHelper.ComputeFileHash(gitPath);
                UpdateSelectedPathDisplay(gitPath);
                AppLogger.Error(TAG, $"哈希不匹配: {gitPath}");
                ShowInfoBar("错误",
                    $"找到的 git.exe 哈希值不匹配！期望: {GitPathHelper.ExpectedGitHash[..16]}... 实际: {actualHash?[..16]}...",
                    InfoBarSeverity.Error);
                return;
            }

            settings.GitPath = gitPath;
            settings.GitHash = GitPathHelper.ComputeFileHash(gitPath);
            SettingsService.Save(settings);

            UpdateSelectedPathDisplay(gitPath);
            AppLogger.OK(TAG, $"自动找到并保存 Git: {gitPath}");
            ShowInfoBar("成功", $"已找到 Git 并保存: {gitPath}", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"查找异常: {ex.Message}");
            ShowInfoBar("错误", $"查找异常: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            LoadingRing.IsActive = false;
        }
    }

    private void UpdateSelectedPathDisplay(string path)
    {
        if (string.IsNullOrEmpty(path))
            SelectedPathDisplay.Text = "当前选中路径：无";
        else
            SelectedPathDisplay.Text = $"当前 Git 路径：{path}";
    }

    // ========== InfoBar ==========
    private async void ShowInfoBar(string title, string message, InfoBarSeverity severity)
    {
        _infoBarCts?.Cancel();
        var newCts = new CancellationTokenSource();
        _infoBarCts = newCts;

        StatusInfoBar.Title = title;
        StatusInfoBar.Message = message;
        StatusInfoBar.Severity = severity;

        StatusInfoBar.IsOpen = false;
        await Task.Delay(50);
        StatusInfoBar.IsOpen = true;

        try
        {
            await Task.Delay(InfoBarDisplayMilliseconds, newCts.Token);
            if (_infoBarCts == newCts)
                StatusInfoBar.IsOpen = false;
        }
        catch (TaskCanceledException) { }
    }
}