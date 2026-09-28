using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using FluentGit.Services;

namespace FluentGit.Views;

public sealed partial class RepoPage
{
    // ========== 通用 Git 操作执行器 ==========
    private async Task ExecuteGitOperationAsync(
        string operationName,
        Func<string, string, Task<(bool Success, string Message)>> action,
        Action? onSuccess = null,
        string? startMessage = null)
    {
        if (_isBusy) return;

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
            _isBusy = true;

            if (!string.IsNullOrEmpty(startMessage))
                ShowInfoBar("提示", startMessage, InfoBarSeverity.Informational);

            var result = await action(gitPath, repoPath);

            if (result.Success)
            {
                ShowInfoBar("成功", result.Message, InfoBarSeverity.Success);
                onSuccess?.Invoke();
            }
            else
            {
                ShowInfoBar("错误", result.Message, InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"{operationName} 异常: {ex.Message}");
            ShowInfoBar("错误", $"操作失败: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    // ========== 初始化仓库 ==========
    private void OnInitRepository(object sender, RoutedEventArgs e)
    {
        if (_isBusy) return;

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
            _isBusy = true;
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
        finally
        {
            _isBusy = false;
        }
    }

    // ========== 打开仓库 ==========
    private async void OnOpenRepo(object sender, RoutedEventArgs e)
    {
        if (_isBusy) return;

        try
        {
            _isBusy = true;

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
                AppState.SetRepository(repoPath);
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
        finally
        {
            _isBusy = false;
        }
    }

    // ========== 拉取更新 ==========
    private async void OnPullRepository(object sender, RoutedEventArgs e)
    {
        await ExecuteGitOperationAsync(
            "OnPullRepository",
            async (git, repo) =>
            {
                var r = await Task.Run(() => GitService.PullRepository(git, repo));
                return (r.Success, r.Message);
            },
            startMessage: "正在拉取更新，请稍候...");
    }

    // ========== 获取远程信息 ==========
    private async void OnFetchRepository(object sender, RoutedEventArgs e)
    {
        await ExecuteGitOperationAsync(
            "OnFetchRepository",
            async (git, repo) =>
            {
                var r = await Task.Run(() => GitService.FetchRepository(git, repo));
                return (r.Success, r.Message);
            },
            startMessage: "正在获取远程信息...");
    }

    // ========== 全部暂存 ==========
    private async void OnAddAll(object sender, RoutedEventArgs e)
    {
        await ExecuteGitOperationAsync(
            "OnAddAll",
            async (git, repo) =>
            {
                var r = await Task.Run(() => GitService.AddAll(git, repo));
                return (r.Success, r.Message);
            },
            onSuccess: RefreshChangeStatus);
    }

    // ========== 提交 ==========
    private async void OnCommit(object sender, RoutedEventArgs e)
    {
        string message = CommitMessageTextBox.Text.Trim();
        if (string.IsNullOrEmpty(message))
        {
            ShowInfoBar("警告", "请输入提交信息!", InfoBarSeverity.Warning);
            return;
        }

        await ExecuteGitOperationAsync(
            "OnCommit",
            async (git, repo) =>
            {
                var r = await Task.Run(() => GitService.Commit(git, repo, message));
                return (r.Success, r.Message);
            },
            onSuccess: () =>
            {
                CommitMessageTextBox.Text = "";
                RefreshChangeStatus();
            });
    }

    // ========== 刷新变更状态（点击事件） ==========
    private void OnRefreshChangeStatus(object sender, RoutedEventArgs e)
    {
        RefreshChangeStatus();
    }
}