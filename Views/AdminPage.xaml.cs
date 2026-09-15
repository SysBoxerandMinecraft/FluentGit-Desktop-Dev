using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentGit.Services;
using Windows.ApplicationModel.DataTransfer;

namespace FluentGit.Views;

public class TreeItemModel
{
    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public bool IsFile { get; set; }
    public ImageSource? IconSource { get; set; }
}

public sealed partial class AdminPage : Page
{
    private const string TAG = "AdminPage";
    private int _treeNodeCount = 0;
    private const int MaxTreeNodes = 800;
    private static readonly ConcurrentDictionary<string, ImageSource> _iconCache = new();

    // ★ 全语言通用过滤清单
    private static readonly string[] SkipFolders =
    {
        ".git", ".svn", ".hg",
        "bin", "obj", "packages", "artifacts", "TestResults", ".vs",
        "__pycache__", ".pytest_cache", ".mypy_cache", ".ruff_cache",
        ".venv", "venv", "env", ".tox", ".eggs",
        ".ipynb_checkpoints", "__pypackages__",
        "node_modules", ".next", ".nuxt", ".turbo", ".vercel",
        "dist", "build", "out", "coverage", ".nyc_output",
        ".pnpm-store", ".yarn",
        "target", ".gradle", ".mvn",
        ".cargo",
        "vendor",
        "composer.phar",
        ".bundle", "log", "tmp", "pkg",
        "CMakeFiles", "cmake_install.cmake",
        "DerivedData",
        ".idea", ".vscode", ".fleet", ".history", ".settings",
        ".cache", ".temp", "temp", "logs",
        "$RECYCLE.BIN",
    };

    private DateTime _lastClickTime = DateTime.MinValue;
    private string? _lastClickedPath = null;

    public AdminPage()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        LoadDirectoryTreeFromState();
    }

    private void LoadDirectoryTreeFromState()
    {
        var repoPath = AppState.CurrentRepoPath;

        if (string.IsNullOrEmpty(repoPath) || !Directory.Exists(repoPath))
        {
            NoRepoText.Visibility = Visibility.Visible;
            DirectoryTreeView.Visibility = Visibility.Collapsed;
            DirectoryTreeView.RootNodes.Clear();
            return;
        }

        NoRepoText.Visibility = Visibility.Collapsed;
        DirectoryTreeView.Visibility = Visibility.Visible;
        LoadDirectoryTree(repoPath);
    }

    private string[] GetActiveSkipFolders()
    {
        var settings = SettingsService.Load();
        if (!settings.FilterBuildArtifacts)
            return new[] { ".git" };

        return SkipFolders;
    }

    // ========== 内置图标 ==========
    private static string IconRoot =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "File_Icon");

    private static ImageSource? GetIcon(string iconFile)
    {
        if (_iconCache.TryGetValue(iconFile, out var cached))
            return cached;

        try
        {
            string full = Path.Combine(IconRoot, iconFile);
            if (File.Exists(full))
            {
                var img = new BitmapImage(new Uri(full));
                _iconCache[iconFile] = img;
                return img;
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"加载图标异常 {iconFile}: {ex.Message}");
        }
        return null;
    }

    // ========== 目录树 ==========
    private void LoadDirectoryTree(string rootPath)
    {
        AppLogger.Info(TAG, $"开始加载目录树: {rootPath}");
        var sw = Stopwatch.StartNew();

        _treeNodeCount = 0;
        DirectoryTreeView.RootNodes.Clear();

        try
        {
            var rootName = Path.GetFileName(rootPath);
            if (string.IsNullOrEmpty(rootName)) rootName = rootPath;

            var rootModel = new TreeItemModel
            {
                Name = rootName + "（仓库）",
                FullPath = rootPath,
                IsFile = false,
                IconSource = GetIcon("Full_Folder.png")
            };

            var rootNode = new TreeViewNode
            {
                Content = rootModel,
                IsExpanded = true
            };

            AddChildren(rootNode, rootPath, 0, 4);

            DirectoryTreeView.RootNodes.Add(rootNode);

            sw.Stop();
            AppLogger.OK(TAG, $"目录树完成: {_treeNodeCount} 节点, {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"加载目录树异常: {ex.Message}");
        }
    }

    private void AddChildren(TreeViewNode parent, string path, int depth, int maxDepth)
    {
        if (depth >= maxDepth || _treeNodeCount >= MaxTreeNodes) return;

        var skipFolders = GetActiveSkipFolders();

        try
        {
            foreach (var dir in Directory.GetDirectories(path).OrderBy(d => d))
            {
                if (_treeNodeCount >= MaxTreeNodes) return;
                string name = Path.GetFileName(dir);
                if (skipFolders.Contains(name, StringComparer.OrdinalIgnoreCase)) continue;

                bool isEmpty = true;
                try
                {
                    isEmpty = !Directory.EnumerateFileSystemEntries(dir)
                                        .Where(p => !skipFolders.Contains(Path.GetFileName(p), StringComparer.OrdinalIgnoreCase))
                                        .Any();
                }
                catch { }

                var model = new TreeItemModel
                {
                    Name = name,
                    FullPath = dir,
                    IsFile = false,
                    IconSource = GetIcon(isEmpty ? "Empty_Folder.png" : "Full_Folder.png")
                };

                var node = new TreeViewNode { Content = model };
                parent.Children.Add(node);
                _treeNodeCount++;

                AddChildren(node, dir, depth + 1, maxDepth);
            }

            foreach (var file in Directory.GetFiles(path).OrderBy(f => f))
            {
                if (_treeNodeCount >= MaxTreeNodes) return;

                var model = new TreeItemModel
                {
                    Name = Path.GetFileName(file),
                    FullPath = file,
                    IsFile = true,
                    IconSource = GetIcon("File.png")
                };

                var node = new TreeViewNode { Content = model };
                parent.Children.Add(node);
                _treeNodeCount++;
            }
        }
        catch { }
    }

    // ========== 双击打开 ==========
    private void DirectoryTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        TreeItemModel? model = ExtractModel(args.InvokedItem);
        if (model == null) return;

        var now = DateTime.Now;
        bool isDoubleClick =
            (now - _lastClickTime).TotalMilliseconds < 400 &&
            model.FullPath == _lastClickedPath;

        _lastClickTime = now;
        _lastClickedPath = model.FullPath;

        if (isDoubleClick && model.IsFile && File.Exists(model.FullPath))
        {
            OpenFile(model.FullPath);
        }
    }

    // ========== 右键菜单 ==========
    private void DirectoryTreeView_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var element = e.OriginalSource as FrameworkElement;
        if (element == null) return;

        // 向上找 TreeViewItem
        var tvi = FindAncestor<TreeViewItem>(element);
        if (tvi == null) return;

        // 从容器拿 TreeViewNode，再拿 TreeItemModel
        var node = DirectoryTreeView.NodeFromContainer(tvi);
        if (node?.Content is not TreeItemModel model) return;

        // 顺手选中
        DirectoryTreeView.SelectedNode = node;

        // 构建菜单
        var menu = BuildContextMenu(model);

        // 显示
        menu.ShowAt(element, e.GetPosition(element));
        e.Handled = true;
    }

    private MenuFlyout BuildContextMenu(TreeItemModel model)
{
    var menu = new MenuFlyout();

    if (model.IsFile)
    {
        // 文件：打开
        var openItem = new MenuFlyoutItem
        {
            Text = "打开",
            Icon = new SymbolIcon(Symbol.OpenFile)
        };
        openItem.Click += (_, _) => OpenFile(model.FullPath);
        menu.Items.Add(openItem);

        // ★ 删掉了「用其他程序打开...」
    }
    else
    {
        // 文件夹：在终端打开、在资源管理器打开
        var terminalItem = new MenuFlyoutItem
        {
            Text = "在终端中打开",
            Icon = new FontIcon { Glyph = "\uE756" }
        };
        terminalItem.Click += (_, _) => OpenTerminal(model.FullPath);
        menu.Items.Add(terminalItem);

        var explorerItem = new MenuFlyoutItem
        {
            Text = "在文件资源管理器中打开",
            Icon = new SymbolIcon(Symbol.Folder)
        };
        explorerItem.Click += (_, _) => OpenExplorer(model.FullPath);
        menu.Items.Add(explorerItem);
    }

    menu.Items.Add(new MenuFlyoutSeparator());

    // 复制路径
    var copyPathItem = new MenuFlyoutItem
    {
        Text = "复制完整路径",
        Icon = new SymbolIcon(Symbol.Copy)
    };
    copyPathItem.Click += (_, _) => CopyToClipboard(model.FullPath);
    menu.Items.Add(copyPathItem);

    // 复制名称
    var copyNameItem = new MenuFlyoutItem
    {
        Text = "复制名称",
        Icon = new SymbolIcon(Symbol.Copy)
    };
    copyNameItem.Click += (_, _) => CopyToClipboard(model.Name);
    menu.Items.Add(copyNameItem);

    menu.Items.Add(new MenuFlyoutSeparator());

    // 刷新
    var refreshItem = new MenuFlyoutItem
    {
        Text = "刷新目录树",
        Icon = new SymbolIcon(Symbol.Refresh)
    };
    refreshItem.Click += (_, _) =>
    {
        var repo = AppState.CurrentRepoPath;
        if (!string.IsNullOrEmpty(repo))
            LoadDirectoryTree(repo);
    };
    menu.Items.Add(refreshItem);

    return menu;
}

    // ========== 右键菜单具体动作 ==========

    private void OpenFile(string path)
    {
        try
        {
            if (!File.Exists(path)) return;
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
            AppLogger.OK(TAG, $"打开文件: {path}");
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"打开文件失败: {ex.Message}");
        }
    }

    private void OpenTerminal(string folderPath)
    {
        try
        {
            if (!Directory.Exists(folderPath)) return;

            // 优先 Windows Terminal
            string wtPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\WindowsApps\wt.exe");

            if (File.Exists(wtPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = wtPath,
                    Arguments = $"-d \"{folderPath}\"",
                    UseShellExecute = true
                });
                AppLogger.OK(TAG, $"WT 打开: {folderPath}");
            }
            else
            {
                // 回退到 cmd
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/K cd /d \"{folderPath}\"",
                    UseShellExecute = true
                });
                AppLogger.OK(TAG, $"cmd 打开: {folderPath}");
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"打开终端失败: {ex.Message}");
        }
    }

    private void OpenExplorer(string folderPath)
    {
        try
        {
            if (!Directory.Exists(folderPath)) return;
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{folderPath}\"",
                UseShellExecute = true
            });
            AppLogger.OK(TAG, $"资源管理器打开: {folderPath}");
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"打开资源管理器失败: {ex.Message}");
        }
    }

    private void CopyToClipboard(string text)
    {
        try
        {
            var dp = new DataPackage();
            dp.SetText(text);
            Clipboard.SetContent(dp);
            AppLogger.OK(TAG, $"已复制: {text}");
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"复制失败: {ex.Message}");
        }
    }

    // ========== 工具方法 ==========

    private static T? FindAncestor<T>(DependencyObject? obj) where T : DependencyObject
    {
        var cur = obj;
        while (cur != null)
        {
            if (cur is T t) return t;
            cur = VisualTreeHelper.GetParent(cur);
        }
        return null;
    }

    private TreeItemModel? ExtractModel(object? obj)
    {
        if (obj is TreeViewNode node && node.Content is TreeItemModel m)
            return m;

        if (obj is TreeItemModel m2)
            return m2;

        return null;
    }
}