using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentGit.Services;

namespace FluentGit.Views;

public sealed partial class RepoPage : Page
{
    private const string TAG = "RepoPage";
    private CancellationTokenSource? _infoBarCts;
    private bool _isInfoBarAnimating = false;

    public RepoPage()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CheckGitAvailability();

        var saved = AppState.CurrentRepoPath;
        if (!string.IsNullOrEmpty(saved) && Directory.Exists(saved))
        {
            RepoActionsPanel.Visibility = Visibility.Collapsed;
            RepoNameDisplay.Text = Path.GetFileName(saved);
            RepoNameDisplay.Visibility = Visibility.Visible;
        }
        UpdateCloneUI();
    }

    // ========== Git 检测 ==========
    private void CheckGitAvailability()
    {
        try
        {
            var settings = SettingsService.Load();
            string? gitPath = settings.GitPath;

            if (string.IsNullOrEmpty(gitPath))
            {
                gitPath = GitPathHelper.FindGitPath();
                if (!string.IsNullOrEmpty(gitPath) && GitPathHelper.ValidateGitPath(gitPath, GitPathHelper.ExpectedGitHash))
                {
                    settings.GitPath = gitPath;
                    settings.GitHash = GitPathHelper.ComputeFileHash(gitPath);
                    SettingsService.Save(settings);
                    AppLogger.OK(TAG, $"自动找到 Git: {gitPath}");
                }
                else
                {
                    gitPath = null;
                }
            }

            bool gitFound = false;
            if (!string.IsNullOrEmpty(gitPath) && GitPathHelper.ValidateGitPath(gitPath, GitPathHelper.ExpectedGitHash))
            {
                try
                {
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = gitPath;
                    process.StartInfo.Arguments = "--version";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    if (process.ExitCode == 0 && output.Contains("git version"))
                        gitFound = true;
                }
                catch (Exception ex)
                {
                    AppLogger.Error(TAG, $"git --version 异常: {ex.Message}");
                }
            }

            if (!gitFound)
            {
                AppLogger.Warning(TAG, "未找到 Git");
                GitWarningPanel.Visibility = Visibility.Visible;
                RepoActionsPanel.Visibility = Visibility.Collapsed;
                RepoOpsPanel.Visibility = Visibility.Collapsed;
                ClonePanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                GitWarningPanel.Visibility = Visibility.Collapsed;
                RepoActionsPanel.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"CheckGitAvailability 异常: {ex.Message}");
        }
    }

    // ========== 浏览目录（初始化用） ==========
    private void OnBrowseForInit(object sender, RoutedEventArgs e)
    {
        try
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
            var folder = folderPicker.PickSingleFolderAsync().GetAwaiter().GetResult();
            if (folder != null) RepoPathTextBox.Text = folder.Path;
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"浏览目录异常: {ex.Message}");
        }
    }

    // ========== 初始化仓库 ==========
    private void OnInitRepository(object sender, RoutedEventArgs e)
    {
        string path = RepoPathTextBox.Text.Trim();
        if (string.IsNullOrEmpty(path))
        {
            ShowInfoBar("警告", "请选择或输入目录路径!", InfoBarSeverity.Warning);
            return;
        }
        if (!Directory.Exists(path))
        {
            ShowInfoBar("错误", "目录不存在，请检查路径!", InfoBarSeverity.Error);
            return;
        }
        if (GitService.IsGitRepository(path))
        {
            ShowInfoBar("提示", "该目录已是 Git 仓库!", InfoBarSeverity.Informational);
            return;
        }

        var settings = SettingsService.Load();
        string? gitPath = settings.GitPath;
        if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
        {
            ShowInfoBar("错误", "Git 路径无效，请检查设置!", InfoBarSeverity.Error);
            return;
        }

        try
        {
            bool success = GitService.InitRepository(gitPath, path);
            if (success)
            {
                AppLogger.OK(TAG, $"仓库初始化成功: {path}");
                ShowInfoBar("成功", $"仓库初始化成功: {path}", InfoBarSeverity.Success);
            }
            else
            {
                ShowInfoBar("错误", "初始化失败，请重试!", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"git init 异常: {ex.Message}");
            ShowInfoBar("错误", $"错误: {ex.Message}", InfoBarSeverity.Error);
        }
    }

    // ========== 打开仓库 ==========
    private async void OnOpenRepo(object sender, RoutedEventArgs e)
    {
        try
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null) return;

            string repoPath = folder.Path;
            if (GitService.IsGitRepository(repoPath))
            {
                AppState.CurrentRepoPath = repoPath;
                RepoActionsPanel.Visibility = Visibility.Collapsed;
                RepoNameDisplay.Text = Path.GetFileName(repoPath);
                RepoNameDisplay.Visibility = Visibility.Visible;
                UpdateCloneUI();
                AppLogger.OK(TAG, $"已打开仓库: {repoPath}");
                ShowInfoBar("成功", $"已打开仓库: {Path.GetFileName(repoPath)}", InfoBarSeverity.Success);
            }
            else
            {
                ShowInfoBar("错误", "所选目录不是 Git 仓库！", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"打开仓库异常: {ex.Message}");
            ShowInfoBar("错误", $"打开异常: {ex.Message}", InfoBarSeverity.Error);
        }
    }

    // ========== 克隆仓库 ==========
    private async void OnCloneRepository(object sender, RoutedEventArgs e)
    {
        var repoPath = AppState.CurrentRepoPath;
        if (string.IsNullOrEmpty(repoPath) || !Directory.Exists(repoPath))
        {
            ShowInfoBar("错误", "请先打开一个 Git 仓库!", InfoBarSeverity.Error);
            return;
        }

        string url = CloneUrlTextBox.Text.Trim();
        if (string.IsNullOrEmpty(url))
        {
            ShowInfoBar("警告", "请输入远程仓库 URL!", InfoBarSeverity.Warning);
            return;
        }

        string targetDir = repoPath;

        var dialog = new ContentDialog
        {
            Title = "确认重置",
            Content = $"将删除当前仓库的 .git 目录并克隆新仓库到:\n{targetDir}\n\n是否继续？",
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
            XamlRoot = this.XamlRoot
        };
        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        var settings = SettingsService.Load();
        string? gitPath = settings.GitPath;
        if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
        {
            ShowInfoBar("错误", "Git 路径无效，请检查设置!", InfoBarSeverity.Error);
            return;
        }

        try
        {
            string gitDir = Path.Combine(targetDir, ".git");
            if (Directory.Exists(gitDir)) Directory.Delete(gitDir, true);

            ShowInfoBar("提示", "正在克隆，请稍候...", InfoBarSeverity.Informational);
            AppLogger.Info(TAG, $"开始克隆: {url} → {targetDir}");
            bool success = await Task.Run(() => GitService.CloneRepository(gitPath, url, targetDir));
            if (success)
            {
                AppLogger.OK(TAG, $"克隆成功: {targetDir}");
                ShowInfoBar("成功", $"克隆成功！已保存至: {targetDir}", InfoBarSeverity.Success);
                if (GitService.IsGitRepository(targetDir))
                {
                    AppState.CurrentRepoPath = targetDir;
                    RepoNameDisplay.Text = Path.GetFileName(targetDir);
                    UpdateCloneUI();
                }
            }
            else
            {
                ShowInfoBar("错误", "克隆失败，请检查 URL 或网络！", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"克隆异常: {ex.Message}");
            ShowInfoBar("错误", $"克隆异常: {ex.Message}", InfoBarSeverity.Error);
        }
    }

    // ========== 拉取更新（git pull） ==========
    private async void OnPullRepository(object sender, RoutedEventArgs e)
    {
        var repoPath = AppState.CurrentRepoPath;
        if (string.IsNullOrEmpty(repoPath) || !Directory.Exists(repoPath))
        {
            ShowInfoBar("错误", "请先打开一个 Git 仓库!", InfoBarSeverity.Error);
            return;
        }

        var settings = SettingsService.Load();
        string? gitPath = settings.GitPath;
        if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
        {
            ShowInfoBar("错误", "Git 路径无效，请检查设置!", InfoBarSeverity.Error);
            return;
        }

        try
        {
            PullButton.IsEnabled = false;
            FetchButton.IsEnabled = false;

            ShowInfoBar("提示", "正在拉取更新，请稍候...", InfoBarSeverity.Informational);

            var result = await Task.Run(() => GitService.PullRepository(gitPath, repoPath));

            if (result.Success)
                ShowInfoBar("成功", result.Message, InfoBarSeverity.Success);
            else
                ShowInfoBar("错误", result.Message, InfoBarSeverity.Error);
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"OnPullRepository 异常: {ex.Message}");
            ShowInfoBar("错误", $"操作失败: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            PullButton.IsEnabled = true;
            FetchButton.IsEnabled = true;
        }
    }

    // ========== 获取远程信息（git fetch） ==========
    private async void OnFetchRepository(object sender, RoutedEventArgs e)
    {
        var repoPath = AppState.CurrentRepoPath;
        if (string.IsNullOrEmpty(repoPath) || !Directory.Exists(repoPath))
        {
            ShowInfoBar("错误", "请先打开一个 Git 仓库!", InfoBarSeverity.Error);
            return;
        }

        var settings = SettingsService.Load();
        string? gitPath = settings.GitPath;
        if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
        {
            ShowInfoBar("错误", "Git 路径无效，请检查设置!", InfoBarSeverity.Error);
            return;
        }

        try
        {
            PullButton.IsEnabled = false;
            FetchButton.IsEnabled = false;

            ShowInfoBar("提示", "正在获取远程信息...", InfoBarSeverity.Informational);

            var result = await Task.Run(() => GitService.FetchRepository(gitPath, repoPath));

            if (result.Success)
                ShowInfoBar("成功", result.Message, InfoBarSeverity.Success);
            else
                ShowInfoBar("错误", result.Message, InfoBarSeverity.Error);
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"OnFetchRepository 异常: {ex.Message}");
            ShowInfoBar("错误", $"操作失败: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            PullButton.IsEnabled = true;
            FetchButton.IsEnabled = true;
        }
    }

    // ========== 更新界面状态 ==========
    private void UpdateCloneUI()
    {
        var repoPath = AppState.CurrentRepoPath;
        bool isRepoOpen = !string.IsNullOrEmpty(repoPath) && Directory.Exists(repoPath);
        if (isRepoOpen)
        {
            RepoOpsPanel.Visibility = Visibility.Visible;
            ClonePanel.Visibility = Visibility.Visible;
            CloneTargetHint.Visibility = Visibility.Visible;
        }
        else
        {
            RepoOpsPanel.Visibility = Visibility.Collapsed;
            ClonePanel.Visibility = Visibility.Collapsed;
        }
    }

    // ========== InfoBar ==========
    private async void ShowInfoBar(string title, string message, InfoBarSeverity severity)
    {
        if (_isInfoBarAnimating)
        {
            _infoBarCts?.Cancel();
            await Task.Delay(50);
        }

        _isInfoBarAnimating = true;
        _infoBarCts = new CancellationTokenSource();
        var token = _infoBarCts.Token;

        SlideInStoryboard.Stop();
        SlideOutStoryboard.Stop();
        InfoBarTransform.Y = -80;
        InfoBarContainer.Opacity = 0;
        InfoBarContainer.Visibility = Visibility.Visible;

        StatusInfoBar.Title = title;
        StatusInfoBar.Message = message;
        StatusInfoBar.Severity = severity;

        SlideInStoryboard.Begin();

        try
        {
            await Task.Delay(3000, token);
            SlideOutStoryboard.Begin();
            await Task.Delay(200);
            InfoBarContainer.Visibility = Visibility.Collapsed;
        }
        catch (TaskCanceledException)
        {
            InfoBarContainer.Visibility = Visibility.Collapsed;
            InfoBarTransform.Y = -80;
            InfoBarContainer.Opacity = 0;
            SlideInStoryboard.Stop();
            SlideOutStoryboard.Stop();
        }
        finally
        {
            _isInfoBarAnimating = false;
        }
    }
}