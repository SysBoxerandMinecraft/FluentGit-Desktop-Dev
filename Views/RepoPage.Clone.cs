using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentGit.Services;

namespace FluentGit.Views;

public sealed partial class RepoPage
{
    // ========== 克隆仓库（带进度 + 取消） ==========
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

        // ---- 构建对话框内容 ----
        var urlBox = new TextBox
        {
            Header = "远程地址",
            PlaceholderText = "https://github.com/user/repo.git"
        };

        var pathBox = new TextBox
        {
            Header = "目标目录",
            PlaceholderText = "选择或输入一个空目录"
        };

        var browseBtn = new Button { Content = "浏览..." };

        var pathGrid = new Grid();
        pathGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        pathGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(pathBox, 0);
        Grid.SetColumn(browseBtn, 1);
        pathBox.Margin = new Thickness(0, 0, 10, 0);
        pathGrid.Children.Add(pathBox);
        pathGrid.Children.Add(browseBtn);

        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Visibility = Visibility.Collapsed
        };

        var statusText = new TextBlock
        {
            Text = "",
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
            Visibility = Visibility.Collapsed
        };

        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(urlBox);
        panel.Children.Add(pathGrid);
        panel.Children.Add(progressBar);
        panel.Children.Add(statusText);

        var dialog = new ContentDialog
        {
            Title = "克隆仓库",
            Content = panel,
            PrimaryButtonText = "克隆",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        // ---- 浏览按钮 ----
        browseBtn.Click += async (_, _) =>
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add("*");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            var folder = await picker.PickSingleFolderAsync();
            if (folder != null) pathBox.Text = folder.Path;
        };

        // ---- 主按钮：开始克隆 ----
        var cts = new CancellationTokenSource();
        bool started = false;

        dialog.PrimaryButtonClick += async (_, args) =>
        {
            if (started) { args.Cancel = true; return; }

            string url = urlBox.Text.Trim();
            string targetPath = pathBox.Text.Trim();

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(targetPath))
            {
                args.Cancel = true;
                statusText.Text = "远程地址和目标目录都必填";
                statusText.Visibility = Visibility.Visible;
                return;
            }

            // 阻止对话框关闭，保持打开显示进度
            args.Cancel = true;
            started = true;

            urlBox.IsEnabled = false;
            pathBox.IsEnabled = false;
            browseBtn.IsEnabled = false;
            dialog.IsPrimaryButtonEnabled = false;
            progressBar.Visibility = Visibility.Visible;
            statusText.Visibility = Visibility.Visible;
            statusText.Text = "正在克隆...";

            try
            {
                var progress = new Action<int, string>((percent, msg) =>
                {
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        progressBar.Value = percent;
                        statusText.Text = msg;
                    });
                });

                var success = await Task.Run(() =>
                    GitService.CloneRepositoryWithProgress(
                        gitPath, url, targetPath, progress, cts.Token));

                if (success)
                {
                    AppState.SetRepository(targetPath);
                    dialog.Hide();

                    RepoActionsPanel.Visibility = Visibility.Collapsed;
                    RepoNameDisplay.Text = Path.GetFileName(targetPath);
                    RepoNameDisplay.Visibility = Visibility.Visible;
                    UpdateCloneUI();

                    AppLogger.OK(TAG, $"克隆成功: {targetPath}");
                    ShowInfoBar("成功", $"已克隆到: {targetPath}", InfoBarSeverity.Success);
                }
                else
                {
                    statusText.Text = "克隆失败";
                    dialog.IsPrimaryButtonEnabled = true;
                    urlBox.IsEnabled = true;
                    pathBox.IsEnabled = true;
                    browseBtn.IsEnabled = true;
                    started = false;
                }
            }
            catch (OperationCanceledException)
            {
                dialog.Hide();
                ShowInfoBar("提示", "克隆已取消", InfoBarSeverity.Informational);
            }
            catch (Exception ex)
            {
                AppLogger.Error(TAG, $"克隆异常: {ex.Message}");
                statusText.Text = $"失败: {ex.Message}";
                dialog.IsPrimaryButtonEnabled = true;
                urlBox.IsEnabled = true;
                pathBox.IsEnabled = true;
                browseBtn.IsEnabled = true;
                started = false;
            }
        };

        // ---- 关闭按钮：如果正在克隆则取消 ----
        dialog.CloseButtonClick += (_, _) =>
        {
            if (started && !cts.IsCancellationRequested)
                cts.Cancel();
        };

        await dialog.ShowAsync();
    }
}