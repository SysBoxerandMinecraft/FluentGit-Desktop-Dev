using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentGit.Services;
using FluentGit.Services.Search;

namespace FluentGit.Views;

public sealed partial class SettingsPage : Page
{
    private const string TAG = "SettingsPage";
    private CancellationTokenSource? _infoBarCts;
    private const int InfoBarDisplayMilliseconds = 3000;

    private bool _isUpdating = false;

    // 设置项索引（用于搜索）
    private readonly List<SearchEntry> _settings = new();

    public SettingsPage()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        BuildSearchIndex();

        _isUpdating = true;
        FilterArtifactsCheckBox.IsChecked = SettingsService.Load().FilterBuildArtifacts;
        _isUpdating = false;

        await AutoFindAndValidateGitAsync();
    }

    // ========== 搜索索引 ==========
    private void BuildSearchIndex()
    {
        _settings.Clear();

        _settings.Add(new SearchEntry
        {
            Title = "Git 安装路径",
            Keywords = new[] { "git", "安装", "路径", "path", "install", "位置", "目录" },
            Item = ItemGitPath,
            Group = GroupGit
        });

        _settings.Add(new SearchEntry
        {
            Title = "过滤编译产物 / 临时文件",
            Keywords = new[]
            {
                "过滤", "编译", "产物", "临时", "文件", "隐藏",
                "bin", "obj", "node_modules", "__pycache__",
                "filter", "artifact", "build", "temp", "cache"
            },
            Item = ItemFilterArtifacts,
            Group = GroupTreeView
        });
    }

    // ========== 搜索事件 ==========
    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
        ApplySearch(sender.Text.Trim());
    }

    private void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is string title)
        {
            sender.Text = title;
            ApplySearch(title);
        }
    }

    private void OnSearchSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is string title)
            sender.Text = title;
    }

    private void ApplySearch(string query)
    {
        var matchedTitles = SearchService.ApplyFilter(_settings, query, out bool hasAnyMatch);

        NoResultText.Visibility = hasAnyMatch ? Visibility.Collapsed : Visibility.Visible;
        SearchBox.ItemsSource = matchedTitles.Count > 0 ? matchedTitles : null;
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
                GitVersionDisplay.Visibility = Visibility.Collapsed;
                AppLogger.Warning(TAG, "未找到 Git");
                ShowInfoBar("警告", "未找到 Git，请先安装 Git for Windows", InfoBarSeverity.Warning);
                return;
            }

            if (!GitPathHelper.ValidateGitPath(gitPath))
            {
                UpdateSelectedPathDisplay(gitPath);
                await UpdateGitVersionAsync(gitPath);
                AppLogger.Error(TAG, $"签名验证失败: {gitPath}");
                ShowInfoBar("错误",
                    "找到的 Git 未通过签名验证，可能不是官方版本。",
                    InfoBarSeverity.Error);
                return;
            }

            settings.GitPath = gitPath;
            SettingsService.Save(settings);

            UpdateSelectedPathDisplay(gitPath);
            await UpdateGitVersionAsync(gitPath);
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

    // ========== 获取 Git 版本号 ==========
    private async Task UpdateGitVersionAsync(string? gitPath)
    {
        if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
        {
            GitVersionDisplay.Visibility = Visibility.Collapsed;
            return;
        }

        try
        {
            var psi = new ProcessStartInfo(gitPath)
            {
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                GitVersionDisplay.Visibility = Visibility.Collapsed;
                return;
            }

            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                // git --version 输出形如: "git version 2.43.0.windows.1"
                string version = output.Trim();
                const string prefix = "git version ";
                if (version.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    version = version.Substring(prefix.Length);

                GitVersionDisplay.Text = $"版本：{version}";
                GitVersionDisplay.Visibility = Visibility.Visible;
                AppLogger.OK(TAG, $"Git 版本: {version}");
            }
            else
            {
                GitVersionDisplay.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"获取 Git 版本失败: {ex.Message}");
            GitVersionDisplay.Visibility = Visibility.Collapsed;
        }
    }

    // ========== InfoBar（从上方滑入，向上滑出） ==========
    private async void ShowInfoBar(string title, string message, InfoBarSeverity severity)
    {
        _infoBarCts?.Cancel();
        var newCts = new CancellationTokenSource();
        _infoBarCts = newCts;
        var token = newCts.Token;

        SlideInStoryboard.Stop();
        SlideOutStoryboard.Stop();
        InfoBarTransform.Y = -30;
        InfoBarContainer.Opacity = 0;
        InfoBarContainer.Visibility = Visibility.Visible;

        StatusInfoBar.Title = title;
        StatusInfoBar.Message = message;
        StatusInfoBar.Severity = severity;

        SlideInStoryboard.Begin();

        try
        {
            await Task.Delay(InfoBarDisplayMilliseconds, token);

            SlideOutStoryboard.Begin();
            await Task.Delay(200);

            InfoBarContainer.Visibility = Visibility.Collapsed;
            InfoBarTransform.Y = -30;
            InfoBarContainer.Opacity = 0;
        }
        catch (TaskCanceledException)
        {
            InfoBarContainer.Visibility = Visibility.Collapsed;
            InfoBarTransform.Y = -30;
            InfoBarContainer.Opacity = 0;
            SlideInStoryboard.Stop();
            SlideOutStoryboard.Stop();
        }
    }
}