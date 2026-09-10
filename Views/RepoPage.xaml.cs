using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentGit.Services;

namespace FluentGit.Views;

public sealed partial class RepoPage : Page
{
    private CancellationTokenSource? _infoBarCts;
    private bool _isInfoBarAnimating = false;
    private string? _currentRepoPath;

    public RepoPage()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CheckGitAvailability();
        UpdateCloneUI();
    }

    private void CheckGitAvailability()
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
                {
                    gitFound = true;
                }
            }
            catch { }
        }

        if (!gitFound)
        {
            GitWarningPanel.Visibility = Visibility.Visible;
            RepoActionsPanel.Visibility = Visibility.Collapsed;
            ClonePanel.Visibility = Visibility.Collapsed;
            StatusPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            GitWarningPanel.Visibility = Visibility.Collapsed;
            RepoActionsPanel.Visibility = Visibility.Visible;
        }
    }

    private void OnBrowseForInit(object sender, RoutedEventArgs e)
    {
        var folderPicker = new Windows.Storage.Pickers.FolderPicker();
        folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
        folderPicker.FileTypeFilter.Add("*");
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
        var folder = folderPicker.PickSingleFolderAsync().GetAwaiter().GetResult();
        if (folder != null)
        {
            RepoPathTextBox.Text = folder.Path;
        }
    }

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
                ShowInfoBar("成功", $"仓库初始化成功: {path}", InfoBarSeverity.Success);
            }
            else
            {
                ShowInfoBar("错误", "初始化失败，请重试!", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            ShowInfoBar("错误", $"错误: {ex.Message}", InfoBarSeverity.Error);
        }
    }

    private async void OnOpenRepo(object sender, RoutedEventArgs e)
    {
        var folderPicker = new Windows.Storage.Pickers.FolderPicker();
        folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
        folderPicker.FileTypeFilter.Add("*");
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder == null)
            return;

        string repoPath = folder.Path;
        if (GitService.IsGitRepository(repoPath))
        {
            _currentRepoPath = repoPath;
            RepoActionsPanel.Visibility = Visibility.Collapsed;
            RepoNameDisplay.Text = Path.GetFileName(repoPath);
            RepoNameDisplay.Visibility = Visibility.Visible;
            UpdateCloneUI();
            RefreshStatus();
            ShowInfoBar("成功", $"已打开仓库: {Path.GetFileName(repoPath)}", InfoBarSeverity.Success);
        }
        else
        {
            ShowInfoBar("错误", "所选目录不是 Git 仓库！", InfoBarSeverity.Error);
        }
    }

    private void OnBrowseCloneTarget(object sender, RoutedEventArgs e)
    {
        ShowInfoBar("提示", "已打开仓库，将克隆到当前仓库目录", InfoBarSeverity.Informational);
    }

    private async void OnCloneRepository(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentRepoPath) || !Directory.Exists(_currentRepoPath))
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

        string targetDir = _currentRepoPath;

        var dialog = new ContentDialog
        {
            Title = "确认重置",
            Content = $"将删除当前仓库的 .git 目录并克隆新仓库到:\n{targetDir}\n\n是否继续？",
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
            XamlRoot = this.XamlRoot
        };
        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
            return;

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
            if (Directory.Exists(gitDir))
            {
                Directory.Delete(gitDir, true);
            }

            ShowInfoBar("提示", "正在克隆，请稍候...", InfoBarSeverity.Informational);
            bool success = await Task.Run(() => GitService.CloneRepository(gitPath, url, targetDir));
            if (success)
            {
                ShowInfoBar("成功", $"克隆成功！已保存至: {targetDir}", InfoBarSeverity.Success);
                if (GitService.IsGitRepository(targetDir))
                {
                    _currentRepoPath = targetDir;
                    RepoNameDisplay.Text = Path.GetFileName(targetDir);
                    UpdateCloneUI();
                    RefreshStatus();
                }
            }
            else
            {
                ShowInfoBar("错误", "克隆失败，请检查 URL 或网络！", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            ShowInfoBar("错误", $"克隆异常: {ex.Message}", InfoBarSeverity.Error);
        }
    }

    private void UpdateCloneUI()
    {
        bool isRepoOpen = !string.IsNullOrEmpty(_currentRepoPath) && Directory.Exists(_currentRepoPath);
        if (isRepoOpen)
        {
            ClonePanel.Visibility = Visibility.Visible;
            CloneTargetTextBox.Text = _currentRepoPath;
            CloneTargetTextBox.IsEnabled = false;
            CloneTargetHint.Text = "将克隆到当前仓库目录（会删除 .git）";
            CloneTargetHint.Visibility = Visibility.Visible;
            CloneUrlTextBox.Focus(FocusState.Programmatic);
            StatusPanel.Visibility = Visibility.Visible;
        }
        else
        {
            ClonePanel.Visibility = Visibility.Collapsed;
            StatusPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void RefreshStatus()
    {
        if (string.IsNullOrEmpty(_currentRepoPath) || !Directory.Exists(_currentRepoPath))
        {
            StatusPanel.Visibility = Visibility.Collapsed;
            return;
        }

        var settings = SettingsService.Load();
        string? gitPath = settings.GitPath;
        if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
        {
            ShowInfoBar("错误", "Git 路径无效!", InfoBarSeverity.Error);
            return;
        }

        try
        {
            // 获取当前分支
            var branchProcess = new System.Diagnostics.Process();
            branchProcess.StartInfo.FileName = gitPath;
            branchProcess.StartInfo.Arguments = "branch --show-current";
            branchProcess.StartInfo.WorkingDirectory = _currentRepoPath;
            branchProcess.StartInfo.UseShellExecute = false;
            branchProcess.StartInfo.RedirectStandardOutput = true;
            branchProcess.StartInfo.CreateNoWindow = true;
            branchProcess.Start();
            string branch = branchProcess.StandardOutput.ReadToEnd().Trim();
            branchProcess.WaitForExit();
            BranchNameText.Text = branch;

            // 获取状态
            var status = GitService.GetStatus(_currentRepoPath, gitPath);
            StatusListView.ItemsSource = status;
            StatusPanel.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            ShowInfoBar("错误", $"获取状态失败: {ex.Message}", InfoBarSeverity.Error);
            StatusPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void OnRefreshStatus(object sender, RoutedEventArgs e)
    {
        RefreshStatus();
    }

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
        InfoBarContainer.Opacity = 0;
        InfoBarTransform.Y = -100;

        StatusInfoBar.Title = title;
        StatusInfoBar.Message = message;
        StatusInfoBar.Severity = severity;

        SlideInStoryboard.Begin();

        try
        {
            await Task.Delay(3000, token);
            SlideOutStoryboard.Begin();
            await Task.Delay(200);
            InfoBarContainer.Opacity = 0;
            InfoBarTransform.Y = -100;
        }
        catch (TaskCanceledException)
        {
            InfoBarContainer.Opacity = 0;
            InfoBarTransform.Y = -100;
            SlideInStoryboard.Stop();
            SlideOutStoryboard.Stop();
        }
        finally
        {
            _isInfoBarAnimating = false;
        }
    }
}