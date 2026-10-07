using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentGit.Services;

namespace FluentGit.Views;

public sealed partial class ClonePage : Page
{
    private const string TAG = "ClonePage";
    private CancellationTokenSource? _cts;
    private bool _isCloning = false;

    // ========== 分支相关状态 ==========
    private string? _selectedBranch = null;
    private string _lastFetchedUrl = "";
    private bool _isFetchingBranches = false;
    private DispatcherQueueTimer? _urlDebounceTimer;
    private int _totalCommits = -1;   // 远程总提交数，-1 表示未知

    public ClonePage()
    {
        InitializeComponent();
    }

    // ============================================================
    //  URL 文本变化：先禁用，防抖后检查远程，检查完再启用
    // ============================================================
    private void OnUrlTextChanged(object sender, TextChangedEventArgs e)
    {
        // URL 变了，关掉所有警告
        WarningBar.IsOpen = false;

        string currentUrl = UrlBox.Text.Trim();

        // 1. URL 变了，先禁用两个控件
        BranchDropdown.IsEnabled = false;
        DepthCombo.IsEnabled = false;

        // 2. URL 为空，回到初始提示
        if (string.IsNullOrEmpty(currentUrl))
        {
            BranchDropdown.Content = "默认分支";
            BranchHint.Text = "填完远程地址后自动加载分支";
            _lastFetchedUrl = "";
            _selectedBranch = null;
            _totalCommits = -1;
            return;
        }

        // 3. 检查中的提示
        BranchDropdown.Content = "检查中...";
        BranchHint.Text = "正在检查远程仓库...";

        // 4. 懒创建防抖计时器
        if (_urlDebounceTimer == null)
        {
            _urlDebounceTimer = DispatcherQueue.CreateTimer();
            _urlDebounceTimer.Interval = TimeSpan.FromMilliseconds(800);
            _urlDebounceTimer.IsRepeating = false;
            _urlDebounceTimer.Tick += async (_, _) =>
            {
                string url = UrlBox.Text.Trim();
                if (string.IsNullOrEmpty(url))
                {
                    BranchDropdown.IsEnabled = false;
                    DepthCombo.IsEnabled = false;
                    BranchDropdown.Content = "默认分支";
                    BranchHint.Text = "填完远程地址后自动加载分支";
                    _lastFetchedUrl = "";
                    _totalCommits = -1;
                    return;
                }

                if (url == _lastFetchedUrl)
                {
                    BranchDropdown.IsEnabled = true;
                    DepthCombo.IsEnabled = true;
                    return;
                }

                await FetchBranchesAsync(url);
            };
        }

        // 5. 重置计时器，等用户停手 800ms 后才真正发请求
        _urlDebounceTimer.Stop();
        _urlDebounceTimer.Start();
    }

    // ============================================================
    //  拉取远程分支，填充下拉菜单
    // ============================================================
    private async Task FetchBranchesAsync(string url)
    {
        if (_isFetchingBranches) return;
        _isFetchingBranches = true;

        try
        {
            var settings = SettingsService.Load();
            string? gitPath = settings.GitPath;
            if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
            {
                BranchHint.Text = "Git 路径无效，请到设置页检查";
                return;
            }

            BranchMenu.Items.Clear();
            BranchDropdown.Content = "加载中...";
            BranchDropdown.IsEnabled = false;
            DepthCombo.IsEnabled = false;
            BranchHint.Text = $"正在加载 {url} 的分支列表...";

            var branches = await GitService.GetRemoteBranchesAsync(gitPath, url);
            _lastFetchedUrl = url;

            // 顺便查一次总提交数（仅 GitHub 有效，其它返回 -1）
            _totalCommits = await GitService.GetRemoteCommitCountAsync(url);

            BranchMenu.Items.Clear();

            // 第一项：默认分支
            var defaultItem = new MenuFlyoutItem { Text = "（默认分支）" };
            defaultItem.Click += (_, _) => SelectBranch(null, "默认分支");
            BranchMenu.Items.Add(defaultItem);

            if (branches.Count > 0)
            {
                BranchMenu.Items.Add(new MenuFlyoutSeparator());

                foreach (var b in branches)
                {
                    var item = new MenuFlyoutItem { Text = b };
                    string captured = b;
                    item.Click += (_, _) => SelectBranch(captured, captured);
                    BranchMenu.Items.Add(item);
                }

                BranchHint.Text = $"已加载 {branches.Count} 个分支";
            }
            else
            {
                BranchMenu.Items.Add(new MenuFlyoutSeparator());
                var emptyItem = new MenuFlyoutItem { Text = "（未获取到分支）" };
                emptyItem.IsEnabled = false;
                BranchMenu.Items.Add(emptyItem);

                BranchHint.Text = "未获取到分支，可能是地址无效或网络问题，将使用默认分支";
            }

            // 检查完毕，启用两个控件
            BranchDropdown.IsEnabled = true;
            DepthCombo.IsEnabled = true;

            if (_selectedBranch != null &&
                branches.Contains(_selectedBranch, StringComparer.OrdinalIgnoreCase))
            {
                BranchDropdown.Content = _selectedBranch;
            }
            else
            {
                _selectedBranch = null;
                BranchDropdown.Content = "默认分支";
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"加载分支失败: {ex.Message}");
            BranchDropdown.Content = "默认分支";
            BranchDropdown.IsEnabled = true;
            DepthCombo.IsEnabled = true;
            BranchHint.Text = "加载分支失败，将使用默认分支";
        }
        finally
        {
            _isFetchingBranches = false;
        }
    }

    // ============================================================
    //  用户从下拉菜单选择分支
    // ============================================================
    private void SelectBranch(string? branch, string display)
    {
        _selectedBranch = branch;
        BranchDropdown.Content = display;
        AppLogger.Info(TAG, $"选择分支: {branch ?? "(默认)"}");
    }

    // ============================================================
    //  克隆深度：用户按回车提交自定义数字
    // ============================================================
    private void OnDepthSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
    {
        string text = args.Text?.Trim() ?? "";
        int depth = 0;

        if (int.TryParse(text, out int n) && n >= 0)
        {
            depth = n;
            sender.Text = n == 0
                ? "完整历史（默认）"
                : $"最近 {n} 个提交";
        }
        else
        {
            sender.Text = "完整历史（默认）";
            depth = 0;
        }

        args.Handled = true;

        CheckDepthWarning(depth);
    }

    // ============================================================
    //  检查克隆深度是否超出仓库提交数
    // ============================================================
    private void CheckDepthWarning(int depth)
    {
        if (depth <= 0)
        {
            WarningBar.IsOpen = false;
            return;
        }

        if (_totalCommits < 0)
        {
            WarningBar.IsOpen = false;
            return;
        }

        if (depth > _totalCommits)
        {
            WarningBar.Title = "克隆深度超过实际提交数";
            WarningBar.Message = $"你输入了 {depth}，但远程仓库只有 {_totalCommits} 个提交。" +
                                 $"Git 会自动拉取全部 {_totalCommits} 个提交，不会报错。";
            WarningBar.Severity = InfoBarSeverity.Warning;
            WarningBar.IsOpen = true;
        }
        else
        {
            WarningBar.IsOpen = false;
        }
    }

    // ============================================================
    //  浏览目标目录
    // ============================================================
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

    // ============================================================
    //  开始克隆
    // ============================================================
    private async void OnStartClick(object sender, RoutedEventArgs e)
    {
        if (_isCloning) return;

        var settings = SettingsService.Load();
        string? gitPath = settings.GitPath;
        if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
        {
            await ShowErrorAsync("Git 路径无效，请到设置页检查。");
            return;
        }

        string url = UrlBox.Text.Trim();
        string targetPath = PathBox.Text.Trim();

        // 解析克隆深度
        int depth = 0;
        string depthText = DepthCombo.Text?.Trim() ?? "";

        if (int.TryParse(depthText, out int customDepth) && customDepth >= 0)
        {
            depth = customDepth;
        }
        else if (DepthCombo.SelectedItem is ComboBoxItem depthItem &&
                 depthItem.Tag is string depthTag &&
                 int.TryParse(depthTag, out int parsedDepth) &&
                 parsedDepth >= 0)
        {
            depth = parsedDepth;
        }

        // 开始前再检查一次深度警告（万一用户没按回车）
        CheckDepthWarning(depth);

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

        _isCloning = true;
        _cts = new CancellationTokenSource();

        UrlBox.IsEnabled = false;
        PathBox.IsEnabled = false;
        BranchDropdown.IsEnabled = false;
        DepthCombo.IsEnabled = false;
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

            int? depthParam = depth > 0 ? depth : null;
            string? branchParam = string.IsNullOrEmpty(_selectedBranch) ? null : _selectedBranch;

            var success = await Task.Run(() =>
                GitService.CloneRepositoryWithProgress(
                    gitPath, url, targetPath, progress, _cts.Token,
                    depthParam, branchParam));

            if (success)
            {
                AppState.SetRepository(targetPath);
                AppLogger.OK(TAG, $"克隆成功: {targetPath}");

                StatusText.Text = $"克隆成功：{targetPath}";
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

    // ============================================================
    //  取消按钮
    // ============================================================
    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        if (_isCloning && _cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
        else
        {
            Frame.Navigate(typeof(RepoPage));
        }
    }

    // ============================================================
    //  恢复 UI（克隆失败/取消后）
    // ============================================================
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

        bool urlChecked = !string.IsNullOrEmpty(_lastFetchedUrl);
        BranchDropdown.IsEnabled = urlChecked;
        DepthCombo.IsEnabled = urlChecked;
    }

    // ============================================================
    //  错误对话框
    // ============================================================
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