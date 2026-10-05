using System;
using System.Collections.Generic;

namespace FluentGit.Services;

// ========== 提交记录模型（顶级类，方便 XAML x:DataType 使用） ==========
public class GitCommit
{
    public string Hash { get; set; } = "";
    public string ShortHash { get; set; } = "";
    public string Author { get; set; } = "";
    public string Email { get; set; } = "";
    public string Date { get; set; } = "";           // ISO 8601 原始值
    public string Message { get; set; } = "";

    public string DateDisplay
    {
        get
        {
            if (DateTimeOffset.TryParse(Date, out var d))
                return d.LocalDateTime.ToString("yyyy-MM-dd HH:mm");
            return Date;
        }
    }
}

public static partial class GitService
{
    // ========== 获取提交历史 ==========
    public static List<GitCommit> GetLog(string gitExePath, string repoPath, int maxCount = 200)
    {
        var result = new List<GitCommit>();
        if (!IsGitRepository(repoPath)) return result;

        try
        {
            var (code, output, error) = RunGit(
                gitExePath,
                new[]
                {
                    "log",
                    $"--max-count={maxCount}",
                    "--pretty=format:%H%x1f%h%x1f%an%x1f%ae%x1f%aI%x1f%s%x1e"
                },
                repoPath);

            if (code != 0)
            {
                AppLogger.Error(TAG, $"git log 失败: {error}");
                return result;
            }

            var records = output.Split('\x1e', StringSplitOptions.RemoveEmptyEntries);
            foreach (var record in records)
            {
                var trimmed = record.Trim('\r', '\n');
                if (string.IsNullOrEmpty(trimmed)) continue;

                var parts = trimmed.Split('\x1f');
                if (parts.Length < 6) continue;

                result.Add(new GitCommit
                {
                    Hash = parts[0],
                    ShortHash = parts[1],
                    Author = parts[2],
                    Email = parts[3],
                    Date = parts[4],
                    Message = parts[5]
                });
            }

            AppLogger.OK(TAG, $"获取到 {result.Count} 条提交记录");
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"GetLog 异常: {ex.Message}");
        }

        return result;
    }
}