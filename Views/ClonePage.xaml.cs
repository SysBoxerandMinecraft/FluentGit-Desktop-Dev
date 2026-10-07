using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentGit.Services;

namespace FluentGit.Views;

public sealed partial class ClonePage : Page
{
    private const string TAG = "ClonePage";
    private CancellationTokenSource? _cts;
    private bool _isCloning = false;

    public ClonePage()
    {
        InitializeComponent();
    }

    // ========== 浏览目标目录 ==========
    private async void OnBrowseClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add("*");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            var folder = await picker.PickSingleFolderAsync();
            if (folder != null) PathBox.Text = folder.Path;
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"浏览目录异常: {ex.Message}");
        }
    }

    // ========== 开始克隆 ==========
    private async void OnStartClick(object sender, RoutedEventArgs e)
    {
        if (_isCloning) return;

        // 检查 Git 路径
        var settings = SettingsService.Load();
        string? gitPath = settings.GitPath;
        if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
        {
            await ShowErrorAsync("Git 路径无效，请到设置页检查。");
            return;
        }

        // 校验输入
        string url = UrlBox.Text.Trim();
        string targetPath = PathBox.Text.Trim();

        if (string.IsNullOrEmpty(url))
        {
            await ShowErrorAsync("请填写远程地址。");
            return;
        }

        if (string.IsNullOrEmpty(targetPath))
        {
            await ShowErrorAsync("请选择目标目录。");
            return;
        }

        // 进入克隆状态
        _isCloning = true;
        _cts = new CancellationTokenSource();

        UrlBox.IsEnabled = false;
        PathBox.IsEnabled = false;
        BrowseButton.IsEnabled = false;
        StartButton.IsEnabled = false;
        CancelButton.IsEnabled = true;
        ProgressBar.Visibility = Visibility.Visible;
        ProgressBar.Value = 0;
        StatusText.Visibility = Visibility.Visible;
        StatusText.Text = "正在克隆...";

        try
        {
            var progress = new Action<int, string>((percent, msg) =>
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    ProgressBar.Value = percent;
                    StatusText.Text = msg;
                });
            });

            var success = await Task.Run(() =>
                GitService.CloneRepositoryWithProgress(
                    gitPath, url, targetPath, progress, _cts.Token));

            if (success)
            {
                AppState.SetRepository(targetPath);
                AppLogger.OK(TAG, $"克隆成功: {targetPath}");

                StatusText.Text = $"克隆成功：{targetPath}";

                // 统一跳到"仓库"页
                Frame.Navigate(typeof(RepoPage));
            }
            else
            {
                StatusText.Text = "克隆失败";
                ResetUI();
            }
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "克隆已取消";
            AppLogger.Warning(TAG, "克隆被用户取消");
            ResetUI();
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"克隆异常: {ex.Message}");
            StatusText.Text = $"失败: {ex.Message}";
            ResetUI();
        }
    }

    // ========== 取消 ==========
    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        if (_isCloning && _cts != null && !_cts.IsCancellationRequested)
        {
            // 正在克隆 → 取消克隆
            _cts.Cancel();
        }
        else
        {
            // 不在克隆中 → 跳回"仓库"页
            Frame.Navigate(typeof(RepoPage));
        }
    }

    // ========== 恢复 UI ==========
    private void ResetUI()
    {
        _isCloning = false;
        _cts?.Dispose();
        _cts = null;

        UrlBox.IsEnabled = true;
        PathBox.IsEnabled = true;
        BrowseButton.IsEnabled = true;
        StartButton.IsEnabled = true;
        CancelButton.IsEnabled = false;
    }

    // ========== 简单的错误对话框 ==========
    private async Task ShowErrorAsync(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "提示",
            Content = message,
            CloseButtonText = "确定",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}