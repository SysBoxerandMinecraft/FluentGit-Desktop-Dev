using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading;
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
                if (!string.IsNullOrEmpty(gitPath) && GitPathHelper.ValidateGitPath(gitPath))
                {
                    settings.GitPath = gitPath;
                    SettingsService.Save(settings);
                    AppLogger.OK(TAG, $"自动找到 Git: {gitPath}");
                }
                else
                {
                    gitPath = null;
                }
            }

            bool gitFound = false;
            if (!string.IsNullOrEmpty(gitPath) && GitPathHelper.ValidateGitPath(gitPath))
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

    // ====================================================================
    // ★ 以下克隆功能已注释（需要时取消注释即可）
    // ====================================================================

    /*
    // ========== 克隆仓库 ==========
    private async void OnCloneRepository(object sender, RoutedEventArgs e)
    {
        // ... 已注释，内容同你当前版本
    }

    // ========== 克隆进度对话框 ==========
    private (ContentDialog Dialog, ProgressBar Bar, TextBlock Status) BuildCloneProgressDialog()
    {
        // ... 已注释，内容同你当前版本
    }
    */

    // ====================================================================
    // ★ 注释结束
    // ====================================================================
}