using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
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

    // 防重入标志（按钮视觉不禁用）
    private bool _isBusy = false;

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
                CommitPanel.Visibility = Visibility.Collapsed;
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

    // ========== 浏览目录 ==========
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
        finally
        {
            _isBusy = false;
        }
    }

    // ====================================================================
    // ★ 以下克隆功能已注释（需要时取消注释即可）
    // ====================================================================

    /*
    // ========== 克隆仓库 ==========
    private async void OnCloneRepository(object sender, RoutedEventArgs e)
    {
        if (_isBusy) return;

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

            // ───── 构建输入对话框 ─────
            var urlBox = new TextBox
            {
                PlaceholderText = "https://github.com/用户名/仓库.git",
                MinHeight = 32,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var targetBox = new TextBox
            {
                PlaceholderText = "选择目标目录",
                MinHeight = 32,
                Margin = new Thickness(0, 0, 10, 0),
                IsReadOnly = true
            };

            var browseBtn = new Button
            {
                Content = "浏览...",
                MinHeight = 32
            };

            browseBtn.Click += async (_, _) =>
            {
                try
                {
                    var picker = new Windows.Storage.Pickers.FolderPicker();
                    picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
                    picker.FileTypeFilter.Add("*");
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                    WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
                    var folder = await picker.PickSingleFolderAsync();
                    if (folder != null)
                        targetBox.Text = folder.Path;
                }
                catch (Exception ex)
                {
                    AppLogger.Error(TAG, $"选择目标目录异常: {ex.Message}");
                }
            };

            var targetRow = new Grid();
            targetRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            targetRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(targetBox, 0);
            Grid.SetColumn(browseBtn, 1);
            targetRow.Children.Add(targetBox);
            targetRow.Children.Add(browseBtn);

            var panel = new StackPanel { Spacing = 5 };
            panel.Children.Add(new TextBlock { Text = "远程仓库 URL：" });
            panel.Children.Add(urlBox);
            panel.Children.Add(new TextBlock { Text = "目标目录：", Margin = new Thickness(0, 10, 0, 0) });
            panel.Children.Add(targetRow);

            var dialog = new ContentDialog
            {
                Title = "克隆远程仓库",
                Content = panel,
                PrimaryButtonText = "克隆",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            string url = urlBox.Text.Trim();
            string targetDir = targetBox.Text.Trim();

            // ───── 校验输入 ─────
            if (string.IsNullOrEmpty(url))
            {
                ShowInfoBar("警告", "请输入远程仓库 URL!", InfoBarSeverity.Warning);
                return;
            }
            if (string.IsNullOrEmpty(targetDir))
            {
                ShowInfoBar("警告", "请选择目标目录!", InfoBarSeverity.Warning);
                return;
            }
            if (!Directory.Exists(targetDir))
            {
                ShowInfoBar("错误", "目标目录不存在!", InfoBarSeverity.Error);
                return;
            }

            // ───── 检查目录是否为空 ─────
            bool isEmpty = Directory.GetFiles(targetDir).Length == 0 &&
                           Directory.GetDirectories(targetDir).Length == 0;

            if (!isEmpty)
            {
                var confirmDialog = new ContentDialog
                {
                    Title = "⚠️ 目录非空",
                    Content = $"目标目录不为空：\n\n{targetDir}\n\n" +
                              "Git clone 需要空目录。是否清空后克隆？\n\n" +
                              "此操作不可撤销！",
                    PrimaryButtonText = "清空并克隆",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };
                var confirmResult = await confirmDialog.ShowAsync();
                if (confirmResult != ContentDialogResult.Primary) return;

                AppLogger.Info(TAG, "清理残留 git 进程...");
                await Task.Run(() => GitService.KillLingeringGitProcesses());

                AppLogger.Info(TAG, $"删除目录: {targetDir}");
                bool deleted = await Task.Run(() => GitService.ForceDeleteDirectory(targetDir));

                if (!deleted)
                {
                    AppLogger.Error(TAG, $"目录删除失败: {targetDir}");
                    ShowInfoBar("错误",
                        "无法清空目录，可能有程序正在使用其中的文件。\n\n" +
                        "请尝试：\n" +
                        "1. 关闭所有 Git 客户端和终端\n" +
                        "2. 在资源管理器中关闭该目录\n" +
                        "3. 重启电脑后再试",
                        InfoBarSeverity.Error);
                    return;
                }

                try
                {
                    Directory.CreateDirectory(targetDir);
                    AppLogger.OK(TAG, $"目录已清空并重建: {targetDir}");
                }
                catch (Exception ex)
                {
                    AppLogger.Error(TAG, $"重建目录失败: {ex.Message}");
                    ShowInfoBar("错误", $"重建目录失败：{ex.Message}", InfoBarSeverity.Error);
                    return;
                }
            }

            // ───── 执行克隆（带进度窗口） ─────
            AppLogger.Info(TAG, $"开始克隆: {url} → {targetDir}");

            var (progressDialog, progressBar, progressStatus) = BuildCloneProgressDialog();

            var cts = new CancellationTokenSource();
            progressDialog.CloseButtonClick += (_, _) => cts.Cancel();

            var dq = this.DispatcherQueue;

            Action<int, string> onProgress = (percent, message) =>
            {
                dq.TryEnqueue(() =>
                {
                    progressBar.Value = percent;
                    progressStatus.Text = message;
                });
            };

            var cloneTask = Task.Run(() =>
                GitService.CloneRepositoryWithProgress(gitPath, url, targetDir, onProgress, cts.Token));

            _ = progressDialog.ShowAsync();

            bool success = false;
            bool cancelled = false;
            try
            {
                success = await cloneTask;
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
                AppLogger.Warning(TAG, "用户取消了克隆");
            }
            catch (Exception ex)
            {
                AppLogger.Error(TAG, $"克隆失败: {ex.Message}");
                progressStatus.Text = $"克隆失败：{ex.Message}";
            }
            finally
            {
                progressDialog.Hide();
            }

            // ───── 处理结果 ─────
            if (cancelled)
            {
                ShowInfoBar("提示", "克隆已取消", InfoBarSeverity.Informational);
            }
            else if (success)
            {
                AppLogger.OK(TAG, $"克隆成功: {targetDir}");
                ShowInfoBar("成功", $"克隆成功！已保存至: {targetDir}", InfoBarSeverity.Success);

                if (GitService.IsGitRepository(targetDir))
                {
                    AppState.CurrentRepoPath = targetDir;
                    RepoActionsPanel.Visibility = Visibility.Collapsed;
                    RepoNameDisplay.Text = Path.GetFileName(targetDir);
                    RepoNameDisplay.Visibility = Visibility.Visible;
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
        finally
        {
            _isBusy = false;
        }
    }

    // ========== 克隆进度对话框 ==========
    private (ContentDialog Dialog, ProgressBar Bar, TextBlock Status) BuildCloneProgressDialog()
    {
        var bar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Height = 6,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var status = new TextBlock
        {
            Text = "准备中...",
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 15, 0, 0),
            FontSize = 13,
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
        };

        var panel = new StackPanel
        {
            Spacing = 10,
            MinWidth = 320
        };
        panel.Children.Add(bar);
        panel.Children.Add(status);

        var dialog = new ContentDialog
        {
            Title = "克隆远程仓库",
            Content = panel,
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.None,
            XamlRoot = this.XamlRoot
        };

        return (dialog, bar, status);
    }
    */

    // ====================================================================
    // ★ 注释结束
    // ====================================================================

    // ========== 拉取更新 ==========
    private async void OnPullRepository(object sender, RoutedEventArgs e)
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
            _isBusy = false;
        }
    }

    // ========== 获取远程信息 ==========
    private async void OnFetchRepository(object sender, RoutedEventArgs e)
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
            _isBusy = false;
        }
    }

    // ========== 全部暂存 ==========
    private async void OnAddAll(object sender, RoutedEventArgs e)
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
            var result = await Task.Run(() => GitService.AddAll(gitPath, repoPath));

            if (result.Success)
            {
                ShowInfoBar("成功", result.Message, InfoBarSeverity.Success);
                RefreshChangeStatus();
            }
            else
            {
                ShowInfoBar("错误", result.Message, InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"OnAddAll 异常: {ex.Message}");
            ShowInfoBar("错误", $"操作失败: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    // ========== 提交 ==========
    private async void OnCommit(object sender, RoutedEventArgs e)
    {
        if (_isBusy) return;

        var repoPath = AppState.CurrentRepoPath;
        if (string.IsNullOrEmpty(repoPath) || !Directory.Exists(repoPath))
        {
            ShowInfoBar("错误", "请先打开一个 Git 仓库!", InfoBarSeverity.Error);
            return;
        }

        string message = CommitMessageTextBox.Text.Trim();
        if (string.IsNullOrEmpty(message))
        {
            ShowInfoBar("警告", "请输入提交信息!", InfoBarSeverity.Warning);
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
            var result = await Task.Run(() => GitService.Commit(gitPath, repoPath, message));

            if (result.Success)
            {
                ShowInfoBar("成功", result.Message, InfoBarSeverity.Success);
                CommitMessageTextBox.Text = "";
                RefreshChangeStatus();
            }
            else
            {
                ShowInfoBar("错误", result.Message, InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"OnCommit 异常: {ex.Message}");
            ShowInfoBar("错误", $"操作失败: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    // ========== 刷新变更状态 ==========
    private void OnRefreshChangeStatus(object sender, RoutedEventArgs e)
    {
        RefreshChangeStatus();
    }

    private async void RefreshChangeStatus()
    {
        var repoPath = AppState.CurrentRepoPath;
        if (string.IsNullOrEmpty(repoPath) || !Directory.Exists(repoPath))
        {
            ChangeStatusText.Text = "未打开仓库";
            return;
        }

        var settings = SettingsService.Load();
        string? gitPath = settings.GitPath;
        if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
        {
            ChangeStatusText.Text = "Git 路径无效";
            return;
        }

        try
        {
            ChangeStatusText.Text = "正在读取变更...";

            var summary = await Task.Run(() => GitService.GetChangeSummary(gitPath, repoPath));

            if (summary.Modified == 0 && summary.Added == 0 && summary.Untracked == 0)
            {
                ChangeStatusText.Text = "工作区干净，无变更";
            }
            else
            {
                var parts = new List<string>();
                if (summary.Modified > 0) parts.Add($"修改 {summary.Modified}");
                if (summary.Added > 0) parts.Add($"新增 {summary.Added}");
                if (summary.Untracked > 0) parts.Add($"未跟踪 {summary.Untracked}");
                ChangeStatusText.Text = string.Join("  ·  ", parts);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"RefreshChangeStatus 异常: {ex.Message}");
            ChangeStatusText.Text = "读取变更失败";
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
            CommitPanel.Visibility = Visibility.Visible;
            RefreshChangeStatus();
        }
        else
        {
            RepoOpsPanel.Visibility = Visibility.Collapsed;
            CommitPanel.Visibility = Visibility.Collapsed;
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