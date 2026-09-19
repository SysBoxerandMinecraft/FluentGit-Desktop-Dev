using System;
using System.Collections.Generic;

namespace FluentGit.Services;

public static partial class GitService
{
    // ========== 获取仓库状态 ==========
    public static List<GitStatusEntry> GetStatus(string repoPath, string gitExePath)
    {
        if (!IsGitRepository(repoPath))
        {
            AppLogger.Error(TAG, $"不是有效的 Git 仓库: {repoPath}");
            throw new Exception("不是有效的 Git 仓库");
        }

        try
        {
            AppLogger.Info(TAG, $"获取仓库状态: {repoPath}");
            var (code, output, error) = RunGit(gitExePath, "status --porcelain", repoPath);

            if (code != 0)
            {
                AppLogger.Error(TAG, $"git status 失败: {error}");
                throw new Exception("获取状态失败");
            }

            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var entries = new List<GitStatusEntry>();
            foreach (var line in lines)
            {
                if (line.Length < 3) continue;
                entries.Add(new GitStatusEntry
                {
                    X = line[0],
                    Y = line[1],
                    Path = line[3..].Trim()
                });
            }

            AppLogger.OK(TAG, $"获取到 {entries.Count} 条状态");
            return entries;
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"GetStatus 异常: {ex.Message}");
            throw;
        }
    }

    // ========== 获取变更摘要 ==========
    public static (int Modified, int Added, int Untracked) GetChangeSummary(string gitExePath, string repoPath)
    {
        var entries = GetStatus(repoPath, gitExePath);
        int modified = 0, added = 0, untracked = 0;

        foreach (var e in entries)
        {
            if (e.X == '?' || e.Y == '?') untracked++;
            else if (e.X == 'A') added++;
            else modified++;
        }

        return (modified, added, untracked);
    }
}