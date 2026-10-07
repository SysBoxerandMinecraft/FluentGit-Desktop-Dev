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

    /// <summary>绝对时间，形如 2026-10-07 12:34</summary>
    public string DateDisplay
    {
        get
        {
            if (DateTimeOffset.TryParse(Date, out var d))
                return d.LocalDateTime.ToString("yyyy-MM-dd HH:mm");
            return Date;
        }
    }

    /// <summary>人性化相对时间，形如 3 分钟前 / 昨天 14:30 / 10-01 14:30</summary>
    public string RelativeTime
    {
        get
        {
            if (!DateTimeOffset.TryParse(Date, out var dto)) return Date;

            var local = dto.LocalDateTime;
            var now = DateTime.Now;
            var diff = now - local;

            if (diff.TotalSeconds < 60) return "刚刚";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} 分钟前";

            if (local.Date == now.Date)
                return $"{(int)diff.TotalHours} 小时前";

            if (local.Date == now.Date.AddDays(-1))
                return $"昨天 {local:HH:mm}";

            if (local.Date > now.Date.AddDays(-7))
            {
                string[] weekdays = { "周日", "周一", "周二", "周三", "周四", "周五", "周六" };
                return $"{weekdays[(int)local.DayOfWeek]} {local:HH:mm}";
            }

            if (local.Year == now.Year)
                return local.ToString("MM-dd HH:mm");

            return local.ToString("yyyy-MM-dd");
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
            // %x1f = Unit Separator，字段分隔
            // %x1e = Record Separator，记录分隔
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