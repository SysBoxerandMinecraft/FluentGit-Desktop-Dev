using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentGit.Services;

public static partial class GitService
{
    // ========== 获取所有分支 ==========
    public static List<string> GetBranches(string repoPath, string gitExePath)
    {
        if (!IsGitRepository(repoPath))
        {
            AppLogger.Error(TAG, $"不是有效的 Git 仓库: {repoPath}");
            throw new Exception("不是有效的 Git 仓库");
        }

        try
        {
            AppLogger.Info(TAG, $"获取分支列表: {repoPath}");
            var (code, output, error) = RunGit(
                gitExePath, new[] { "branch", "--format=%(refname:short)" }, repoPath);

            if (code != 0)
            {
                AppLogger.Error(TAG, $"获取分支失败: {error}");
                throw new Exception("获取分支失败");
            }

            var branches = output
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            AppLogger.OK(TAG, $"获取到 {branches.Count} 个分支");
            return branches;
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"GetBranches 异常: {ex.Message}");
            throw;
        }
    }
}