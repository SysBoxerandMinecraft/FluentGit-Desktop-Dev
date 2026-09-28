using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentGit.Services;

namespace FluentGit.Views;

public sealed partial class RepoPage
{
    // ========== 刷新变更状态 ==========
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
}