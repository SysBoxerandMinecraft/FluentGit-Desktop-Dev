using System;
using System.IO;

namespace FluentGit.Services;

public static partial class GitService
{
    // ========== 拉取更新 ==========
    public static (bool Success, string Message) PullRepository(string gitExePath, string repoPath)
    {
        if (!File.Exists(gitExePath))
            return (false, "git.exe 未找到");
        if (!IsGitRepository(repoPath))
            return (false, "不是有效的 Git 仓库");

        if (!HasRemote(gitExePath, repoPath))
        {
            AppLogger.Warning(TAG, "该仓库未配置远程仓库");
            return (false,
                "该仓库尚未关联远程仓库，无法拉取。\n\n" +
                "如果你是用「初始化仓库」创建的本地仓库，\n" +
                "请先添加远程：git remote add origin <URL>\n" +
                "或者直接用「克隆仓库」从远程拉取。");
        }

        try
        {
            AppLogger.Info(TAG, $"开始 git pull: {repoPath}");
            var (code, output, error) = RunGit(gitExePath, "pull", repoPath);

            string combined = output + "\n" + error;

            if (code == 0)
            {
                AppLogger.OK(TAG, "git pull 成功");
                return (true, "已拉取最新代码");
            }

            if (combined.Contains("no tracking information", StringComparison.OrdinalIgnoreCase))
            {
                AppLogger.Warning(TAG, "当前分支未关联远程分支");
                return (false,
                    "当前分支未关联远程分支。\n\n" +
                    "请先在终端执行：\ngit branch --set-upstream-to=origin/分支名");
            }

            if (combined.Contains("CONFLICT", StringComparison.OrdinalIgnoreCase) ||
                combined.Contains("Automatic merge failed", StringComparison.OrdinalIgnoreCase))
            {
                AppLogger.Warning(TAG, "合并冲突");
                return (false, "合并冲突，请手动解决");
            }

            if (combined.Contains("couldn't find remote ref", StringComparison.OrdinalIgnoreCase))
                return (false, "远程分支不存在");

            if (combined.Contains("Authentication failed", StringComparison.OrdinalIgnoreCase) ||
                combined.Contains("could not read Username", StringComparison.OrdinalIgnoreCase))
                return (false, "身份验证失败，请检查凭据");

            if (combined.Contains("Could not resolve host", StringComparison.OrdinalIgnoreCase) ||
                combined.Contains("unable to access", StringComparison.OrdinalIgnoreCase))
                return (false, "网络无法连接到远程仓库");

            if (combined.Contains("diverged", StringComparison.OrdinalIgnoreCase) ||
                combined.Contains("have diverged", StringComparison.OrdinalIgnoreCase))
                return (false, "本地与远程分支已分叉，请手动处理");

            AppLogger.Error(TAG, $"git pull 失败: {error.Trim()}");
            return (false, error.Trim());
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"git pull 异常: {ex.Message}");
            return (false, ex.Message);
        }
    }

    // ========== 获取远程信息 ==========
    public static (bool Success, string Message) FetchRepository(string gitExePath, string repoPath)
    {
        if (!File.Exists(gitExePath))
            return (false, "git.exe 未找到");
        if (!IsGitRepository(repoPath))
            return (false, "不是有效的 Git 仓库");

        if (!HasRemote(gitExePath, repoPath))
        {
            AppLogger.Warning(TAG, "该仓库未配置远程仓库");
            return (false,
                "该仓库尚未关联远程仓库，无法获取。\n\n" +
                "如果你是用「初始化仓库」创建的本地仓库，\n" +
                "请先添加远程：git remote add origin <URL>\n" +
                "或者直接用「克隆仓库」从远程拉取。");
        }

        try
        {
            AppLogger.Info(TAG, $"开始 git fetch --all: {repoPath}");
            var (code, output, error) = RunGit(gitExePath, "fetch --all", repoPath);

            string combined = output + "\n" + error;

            if (code == 0)
            {
                AppLogger.OK(TAG, "git fetch 成功");
                return (true, "已获取远程最新信息");
            }

            if (combined.Contains("Could not resolve host", StringComparison.OrdinalIgnoreCase) ||
                combined.Contains("unable to access", StringComparison.OrdinalIgnoreCase))
                return (false, "网络无法连接到远程仓库");

            AppLogger.Error(TAG, $"git fetch 失败: {error.Trim()}");
            return (false, error.Trim());
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"git fetch 异常: {ex.Message}");
            return (false, ex.Message);
        }
    }
}