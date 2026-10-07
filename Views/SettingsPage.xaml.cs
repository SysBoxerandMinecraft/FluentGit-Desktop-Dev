using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentGit.Services;
using FluentGit.Services.Search;

namespace FluentGit.Views;

public sealed partial class SettingsPage : Page
{
    private const string TAG = "SettingsPage";

    private bool _isUpdating = false;

    private readonly List<SearchEntry> _settings = new();

    public SettingsPage()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        BuildSearchIndex();

        var settings = SettingsService.Load();

        _isUpdating = true;

        FilterArtifactsCheckBox.IsChecked = settings.FilterBuildArtifacts;

        string currentBackdrop = settings.BackdropType ?? "Default";
        if (currentBackdrop == "MicaAlt") currentBackdrop = "Default";

        foreach (var obj in BackdropComboBox.Items)
        {
            if (obj is ComboBoxItem item && (item.Tag as string) == currentBackdrop)
            {
                BackdropComboBox.SelectedItem = item;
                break;
            }
        }

        _isUpdating = false;

        UpdateBackdropHint(currentBackdrop);

        await AutoFindAndValidateGitAsync();
    }

    // ========== 搜索索引 ==========
    private void BuildSearchIndex()
    {
        _settings.Clear();

        _settings.Add(new SearchEntry
        {
            Title = "Git 安装路径",
            Keywords = new[] { "git", "安装", "路径", "path", "install", "位置", "目录", "版本", "version" },
            Item = ItemGitPath,
            Group = GroupGit
        });

        _settings.Add(new SearchEntry
        {
            Title = "窗口背景效果",
            Keywords = new[]
            {
                "背景", "效果", "外观", "主题",
                "mica", "acrylic", "亚克力", "毛玻璃",
                "backdrop", "background", "appearance"
            },
            Item = ItemBackdrop,
            Group = GroupAppearance
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
        Toast.Success("已开启过滤编译产物", "设置已更新");
    }

    private void OnFilterArtifactsUnchecked(object sender, RoutedEventArgs e)
    {
        if (_isUpdating) return;
        var settings = SettingsService.Load();
        settings.FilterBuildArtifacts = false;
        SettingsService.Save(settings);
        AppLogger.OK(TAG, "过滤编译产物：已关闭");
        Toast.Info("已关闭过滤编译产物", "设置已更新");
    }

    // ========== 背景效果 ==========
    private void OnBackdropChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdating) return;
        if (BackdropComboBox.SelectedItem is not ComboBoxItem item) return;
        if (item.Tag is not string tag) return;

        var settings = SettingsService.Load();
        settings.BackdropType = tag;
        SettingsService.Save(settings);

        App.MainWindow?.ApplyBackdrop(tag);

        UpdateBackdropHint(tag);
        AppLogger.OK(TAG, $"背景效果切换为: {tag}");

        // 全局 InfoBar 提示
        string displayName = tag switch
        {
            "Default" => "跟随系统",
            "Mica"    => "Mica",
            "Acrylic" => "亚克力",
            "None"    => "纯色",
            _         => tag
        };
        Toast.Success($"窗口背景已切换为「{displayName}」", "外观已更新");
    }

    private void UpdateBackdropHint(string tag)
    {
        string hint = tag switch
        {
            "Default" => MicaController.IsSupported()
                ? "根据系统自动选择最佳效果。当前系统支持 Mica。"
                : "根据系统自动选择最佳效果。当前系统不支持 Mica，将使用纯色背景。",

            "Mica" => MicaController.IsSupported()
                ? "当前系统支持 Mica。采样桌面壁纸色调，性能开销低。"
                : "当前系统不支持 Mica，将回退为纯色背景。",

            "Acrylic" => DesktopAcrylicController.IsSupported()
                ? "当前系统支持亚克力。实时模糊窗口背后内容，性能开销较大。"
                : "当前系统不支持亚克力，将回退为纯色背景。",

            "None" => "使用纯色背景，性能最好。",

            _ => ""
        };

        BackdropHint.Text = hint;
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
                Toast.Warning("未找到 Git，请先安装 Git for Windows");
                return;
            }

            if (!GitPathHelper.ValidateGitPath(gitPath))
            {
                UpdateSelectedPathDisplay(gitPath);
                await UpdateGitVersionAsync(gitPath);
                AppLogger.Error(TAG, $"签名验证失败: {gitPath}");
                Toast.Error("找到的 Git 未通过签名验证，可能不是官方版本");
                return;
            }

            settings.GitPath = gitPath;
            SettingsService.Save(settings);

            UpdateSelectedPathDisplay(gitPath);
            await UpdateGitVersionAsync(gitPath);
            AppLogger.OK(TAG, $"自动找到并保存 Git: {gitPath}");
            Toast.Success($"已找到 Git：{gitPath}");
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"查找异常: {ex.Message}");
            Toast.Error($"查找异常: {ex.Message}");
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
}