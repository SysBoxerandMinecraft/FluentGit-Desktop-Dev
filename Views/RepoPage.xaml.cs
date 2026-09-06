using Microsoft.UI.Xaml.Controls;
using System.IO;
using System.Diagnostics;
using FluentGit.Services;

namespace FluentGit.Views;

public sealed partial class RepoPage : Page
{
    // 预期 Git.exe 的 SHA-256 哈希值（与 SettingsPage 一致）
    private const string ExpectedGitHash = "c470d205517c7a53ceca321df16a6e4549fcd52b576ab4d09536d36f26fda5a9";

    public RepoPage()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        CheckGitAvailability();
    }

    private void CheckGitAvailability()
    {
        var settings = SettingsService.Load();
        string? gitPath = settings.GitPath;
        string? storedHash = settings.GitHash;

        // 如果存储的哈希为空，但路径存在，则尝试用预期哈希验证（兼容旧配置）
        if (string.IsNullOrEmpty(gitPath))
        {
            gitPath = GitPathHelper.FindGitPath();
            if (!string.IsNullOrEmpty(gitPath) && GitPathHelper.ValidateGitPath(gitPath, ExpectedGitHash))
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
        if (!string.IsNullOrEmpty(gitPath) && GitPathHelper.ValidateGitPath(gitPath, storedHash ?? ExpectedGitHash))
        {
            try
            {
                var process = new Process();
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
            GitWarningPanel.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            MainContent.Text = "Git 未找到";
        }
        else
        {
            GitWarningPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            MainContent.Text = "仓库管理页面";
        }
    }
}