using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using System.Threading.Tasks;
using FluentGit.Services;
using Windows.ApplicationModel.DataTransfer;

namespace FluentGit.Views;

public sealed partial class HistoryPage : Page
{
    private const string TAG = "HistoryPage";
    private bool _isLoading = false;

    // 对话框里用的字体：明确指定，避免触发系统字体解析
    private static readonly FontFamily UiFont =
        new FontFamily("Segoe UI Variable,Segoe UI");
    private static readonly FontFamily MonoFont =
        new FontFamily("Consolas,Cascadia Mono,Courier New");

    public HistoryPage()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _ = LoadLogAsync();
    }

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

    // ========== 右键前先选中该项 ==========
    private void OnItemRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is GitCommit commit)
        {
            LogListView.SelectedItem = commit;
        }
    }

    // ========== 左键点击某条提交：打开详情 ==========
    private async void OnCommitItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not GitCommit commit) return;
        await ShowCommitDetailAsync(commit);
    }

    // ========== 左键点击 hash：复制完整 Hash ==========
    private void OnHashClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string hash)
        {
            CopyToClipboard(hash, "Hash");
        }
    }

    // ========== 右键菜单：复制完整 Hash ==========
    private void OnCopyHashClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is string hash)
        {
            CopyToClipboard(hash, "完整 Hash");
        }
    }

    // ========== 右键菜单：复制短 Hash ==========
    private void OnCopyShortHashClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is string shortHash)
        {
            CopyToClipboard(shortHash, "短 Hash");
        }
    }

    // ========== 右键菜单：复制提交信息 ==========
    private void OnCopyMessageClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is string message)
        {
            CopyToClipboard(message, "提交信息");
        }
    }

    // ========== 右键菜单：查看详情 ==========
    private async void OnViewDetailClick(object sender, RoutedEventArgs e)
    {
        if (LogListView.SelectedItem is GitCommit commit)
        {
            await ShowCommitDetailAsync(commit);
        }
        else
        {
            AppLogger.Warning(TAG, "右键菜单找不到对应的 commit");
        }
    }

    // ========== 显示提交详情对话框 ==========
    private async Task ShowCommitDetailAsync(GitCommit commit)
    {
        var panel = new StackPanel { Spacing = 10 };

        panel.Children.Add(BuildDetailRow("Hash", commit.Hash, isMonospace: true));
        panel.Children.Add(BuildDetailRow("短 Hash", commit.ShortHash, isMonospace: true));
        panel.Children.Add(BuildSeparator());

        panel.Children.Add(BuildDetailRow("作者", commit.Author));
        panel.Children.Add(BuildDetailRow("邮箱", commit.Email));
        panel.Children.Add(BuildDetailRow("时间", commit.DateDisplay));
        panel.Children.Add(BuildDetailRow("相对", commit.RelativeTime));

        panel.Children.Add(BuildSeparator());

        panel.Children.Add(new TextBlock
        {
            Text = "提交信息",
            FontSize = 12,
            FontFamily = UiFont,
            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
        });

        panel.Children.Add(new TextBlock
        {
            Text = commit.Message,
            TextWrapping = TextWrapping.Wrap,
            IsTextSelectionEnabled = true,
            FontFamily = UiFont
        });

        var dialog = new ContentDialog
        {
            Title = "提交详情",
            Content = new ScrollViewer
            {
                Content = panel,
                MaxHeight = 400,
                HorizontalScrollMode = ScrollMode.Disabled,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            },
            PrimaryButtonText = "复制 Hash",
            CloseButtonText = "关闭",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            CopyToClipboard(commit.Hash, "Hash");
        }
    }

    private static StackPanel BuildDetailRow(string label, string value, bool isMonospace = false)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        row.Children.Add(new TextBlock
        {
            Text = label,
            Width = 60,
            FontSize = 12,
            FontFamily = UiFont,
            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
            VerticalAlignment = VerticalAlignment.Top
        });

        row.Children.Add(new TextBlock
        {
            Text = value,
            TextWrapping = TextWrapping.Wrap,
            IsTextSelectionEnabled = true,
            FontFamily = isMonospace ? MonoFont : UiFont,
            FontSize = 13
        });

        return row;
    }

    private static Border BuildSeparator()
    {
        return new Border
        {
            Height = 1,
            Background = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
            Margin = new Thickness(0, 4, 0, 4)
        };
    }

    // ========== 通用：复制到剪贴板 ==========
    private void CopyToClipboard(string text, string label)
    {
        try
        {
            var dp = new DataPackage();
            dp.SetText(text);
            Clipboard.SetContent(dp);
            AppLogger.OK(TAG, $"已复制{label}: {text}");
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"复制失败: {ex.Message}");
        }
    }
}