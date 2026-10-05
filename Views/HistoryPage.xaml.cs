using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using FluentGit.Services;

namespace FluentGit.Views;

public sealed partial class HistoryPage : Page
{
    private const string TAG = "HistoryPage";
    private bool _isLoading = false;

    public HistoryPage()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _ = LoadLogAsync();
    }

    // 每次导航到本页也刷新（从缓存来的也算）
    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _ = LoadLogAsync();
    }

    private async void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        await LoadLogAsync();
    }

    private async Task LoadLogAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            var repoPath = AppState.CurrentRepoPath;

            // 未打开仓库
            if (string.IsNullOrEmpty(repoPath) || !Directory.Exists(repoPath))
            {
                NoRepoText.Visibility = Visibility.Visible;
                EmptyText.Visibility = Visibility.Collapsed;
                LogListView.Visibility = Visibility.Collapsed;
                return;
            }

            var settings = SettingsService.Load();
            string? gitPath = settings.GitPath;
            if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
            {
                NoRepoText.Text = "Git 路径无效，请到设置页检查。";
                NoRepoText.Visibility = Visibility.Visible;
                EmptyText.Visibility = Visibility.Collapsed;
                LogListView.Visibility = Visibility.Collapsed;
                return;
            }

            // 显示 loading
            LoadingRing.Visibility = Visibility.Visible;
            LoadingRing.IsActive = true;
            RefreshButton.IsEnabled = false;

            var commits = await Task.Run(() => GitService.GetLog(gitPath, repoPath));

            if (commits == null || commits.Count == 0)
            {
                NoRepoText.Visibility = Visibility.Collapsed;
                EmptyText.Visibility = Visibility.Visible;
                LogListView.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoRepoText.Visibility = Visibility.Collapsed;
                EmptyText.Visibility = Visibility.Collapsed;
                LogListView.Visibility = Visibility.Visible;
                LogListView.ItemsSource = commits;
            }

            AppLogger.OK(TAG, $"加载 {commits?.Count ?? 0} 条提交记录");
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"LoadLogAsync 异常: {ex.Message}");
        }
        finally
        {
            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Visibility.Collapsed;
            RefreshButton.IsEnabled = true;
            _isLoading = false;
        }
    }

    // ========== 点击某条提交 ==========
    private async void OnCommitItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not GitCommit commit) return;

        var dialog = new ContentDialog
        {
            Title = "提交详情",
            Content = new ScrollViewer
            {
                Content = new TextBlock
                {
                    Text = $"Hash: {commit.Hash}\n\n" +
                           $"作者: {commit.Author}\n\n" +
                           $"邮箱: {commit.Email}\n\n" +
                           $"时间: {commit.Date}\n\n" +
                           $"消息:\n{commit.Message}",
                    TextWrapping = TextWrapping.Wrap,
                    IsTextSelectionEnabled = true
                },
                MaxHeight = 400
            },
            CloseButtonText = "关闭",
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
    }
}