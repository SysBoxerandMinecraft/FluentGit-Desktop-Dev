using System;

namespace FluentGit.Services;

public static partial class GitService
{
    // ========== 拉取 ==========
    public static (bool Success, string Message) PullRepository(string gitPath, string repoPath)
    {
        try
        {
            var result = RunGit(gitPath, new[] { "pull" }, repoPath);

            if (result.ExitCode == 0)
            {
                string msg = string.IsNullOrWhiteSpace(result.Output)
                    ? "拉取成功"
                    : result.Output.Trim();
                return (true, msg);
            }

            string err = string.IsNullOrWhiteSpace(result.Error)
                ? "拉取失败，请检查远程配置或网络"
                : result.Error.Trim();
            return (false, err);
        }
        catch (Exception ex)
        {
            return (false, $"拉取异常: {ex.Message}");
        }
    }

    // ========== 获取远程信息 ==========
    public static (bool Success, string Message) FetchRepository(string gitPath, string repoPath)
    {
        try
        {
            var result = RunGit(gitPath, new[] { "fetch", "--all" }, repoPath);

            if (result.ExitCode == 0)
            {
                string msg = string.IsNullOrWhiteSpace(result.Output)
                    ? "获取远程信息成功"
                    : result.Output.Trim();
                return (true, msg);
            }

            string err = string.IsNullOrWhiteSpace(result.Error)
                ? "获取远程信息失败"
                : result.Error.Trim();
            return (false, err);
        }
        catch (Exception ex)
        {
            return (false, $"获取远程信息异常: {ex.Message}");
        }
    }

    // ========== 推送 ==========
    public static (bool Success, string Message) PushRepository(string gitPath, string repoPath)
    {
        try
        {
            // 先尝试普通 push
            var result = RunGit(gitPath, new[] { "push" }, repoPath);

            // 如果没有 upstream，用 -u origin HEAD 兜底
            if (result.ExitCode != 0 &&
                !string.IsNullOrEmpty(result.Error) &&
                result.Error.Contains("no upstream branch", StringComparison.OrdinalIgnoreCase))
            {
                result = RunGit(gitPath, new[] { "push", "-u", "origin", "HEAD" }, repoPath);
            }

            if (result.ExitCode == 0)
            {
                string msg = string.IsNullOrWhiteSpace(result.Output)
                    ? "推送成功"
                    : result.Output.Trim();
                return (true, msg);
            }

            string err = string.IsNullOrWhiteSpace(result.Error)
                ? "推送失败，请检查远程配置或网络"
                : result.Error.Trim();
            return (false, err);
        }
        catch (Exception ex)
        {
            return (false, $"推送异常: {ex.Message}");
        }
    }
}