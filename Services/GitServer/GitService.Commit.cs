using System;
using System.IO;

namespace FluentGit.Services;

public static partial class GitService
{
    // ========== 暂存所有变更 ==========
    public static (bool Success, string Message) AddAll(string gitExePath, string repoPath)
    {
        if (!File.Exists(gitExePath))
            return (false, "git.exe 未找到");
        if (!IsGitRepository(repoPath))
            return (false, "不是有效的 Git 仓库");

        try
        {
            AppLogger.Info(TAG, $"开始 git add -A: {repoPath}");
            var (code, _, error) = RunGit(gitExePath, new[] { "add", "-A" }, repoPath);

            if (code == 0)
            {
                AppLogger.OK(TAG, "git add 成功");
                return (true, "已暂存所有变更");
            }

            AppLogger.Error(TAG, $"git add 失败: {error}");
            return (false, error.Trim());
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"git add 异常: {ex.Message}");
            return (false, ex.Message);
        }
    }

    // ========== 提交变更 ==========
    public static (bool Success, string Message) Commit(string gitExePath, string repoPath, string message)
    {
        if (!File.Exists(gitExePath))
            return (false, "git.exe 未找到");
        if (!IsGitRepository(repoPath))
            return (false, "不是有效的 Git 仓库");
        if (string.IsNullOrWhiteSpace(message))
            return (false, "提交信息不能为空");

        try
        {
            AppLogger.Info(TAG, $"开始 git commit: {message}");

            // 直接传字符串，由 ArgumentList 负责转义
            var (code, output, error) = RunGit(
                gitExePath, new[] { "commit", "-m", message }, repoPath);

            string combined = output + "\n" + error;

            if (code == 0)
            {
                AppLogger.OK(TAG, "git commit 成功");
                return (true, "提交成功");
            }

            if (combined.Contains("nothing to commit", StringComparison.OrdinalIgnoreCase))
                return (false, "没有需要提交的变更");

            if (combined.Contains("Please tell me who you are", StringComparison.OrdinalIgnoreCase) ||
                combined.Contains("user.email", StringComparison.OrdinalIgnoreCase))
            {
                return (false,
                    "尚未配置 Git 用户信息。\n\n" +
                    "请先在终端执行：\n" +
                    "git config --global user.name \"你的名字\"\n" +
                    "git config --global user.email \"你的邮箱\"");
            }

            AppLogger.Error(TAG, $"git commit 失败: {error}");
            return (false, error.Trim());
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"git commit 异常: {ex.Message}");
            return (false, ex.Message);
        }
    }
}