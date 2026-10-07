using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            var result = RunGit(gitPath, new[] { "push" }, repoPath);

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

    // ========== 获取远程分支列表（不克隆，直接 ls-remote） ==========
    public static async Task<List<string>> GetRemoteBranchesAsync(
        string gitExePath,
        string remoteUrl,
        int timeoutMs = 15000)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(remoteUrl)) return result;

        try
        {
            AppLogger.Info(TAG, $"获取远程分支: {remoteUrl}");

            var psi = new System.Diagnostics.ProcessStartInfo(gitExePath)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            psi.ArgumentList.Add("ls-remote");
            psi.ArgumentList.Add("--heads");
            psi.ArgumentList.Add(remoteUrl);

            using var process = new System.Diagnostics.Process { StartInfo = psi };
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(timeoutMs))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                AppLogger.Warning(TAG, $"ls-remote 超时 ({timeoutMs}ms)");
                return result;
            }

            string output = await outputTask;
            string error = await errorTask;

            if (process.ExitCode != 0)
            {
                AppLogger.Error(TAG, $"git ls-remote 失败: {error}");
                return result;
            }

            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                int idx = line.IndexOf("refs/heads/", StringComparison.Ordinal);
                if (idx < 0) continue;

                string name = line.Substring(idx + "refs/heads/".Length).Trim();
                if (!string.IsNullOrEmpty(name))
                    result.Add(name);
            }

            result.Sort(StringComparer.OrdinalIgnoreCase);
            AppLogger.OK(TAG, $"获取到 {result.Count} 个远程分支");
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"GetRemoteBranchesAsync 异常: {ex.Message}");
        }

        return result;
    }

    // ========== 获取远程仓库的总提交数（仅 GitHub 支持） ==========
    public static async Task<int> GetRemoteCommitCountAsync(string remoteUrl, string? branch = null)
    {
        string? repoPath = ExtractGitHubRepoPath(remoteUrl);
        if (string.IsNullOrEmpty(repoPath)) return -1;   // 非 GitHub，未知

        try
        {
            using var http = new System.Net.Http.HttpClient();
            http.Timeout = TimeSpan.FromSeconds(10);
            http.DefaultRequestHeaders.UserAgent.ParseAdd("FluentGit");

            string apiUrl = $"https://api.github.com/repos/{repoPath}/commits?per_page=1";
            if (!string.IsNullOrEmpty(branch))
                apiUrl += $"&sha={Uri.EscapeDataString(branch)}";

            var resp = await http.GetAsync(apiUrl);
            if (!resp.IsSuccessStatusCode)
            {
                AppLogger.Warning(TAG, $"GitHub API 返回 {resp.StatusCode}: {repoPath}");
                return -1;
            }

            // Link header: <https://api.github.com/...?page=42>; rel="last"
            if (resp.Headers.TryGetValues("Link", out var links))
            {
                string link = links.FirstOrDefault() ?? "";
                var m = Regex.Match(link, @"[?&]page=(\d+)>;\s*rel=""last""");
                if (m.Success && int.TryParse(m.Groups[1].Value, out int total))
                {
                    AppLogger.OK(TAG, $"远程提交数: {total} ({repoPath})");
                    return total;
                }
            }

            // 没有 Link header = 只有一页 = 1 个提交
            AppLogger.OK(TAG, $"远程提交数: 1 ({repoPath})");
            return 1;
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"GetRemoteCommitCountAsync 异常: {ex.Message}");
            return -1;
        }
    }

    // ========== 从远程 URL 提取 GitHub 的 owner/repo ==========
    private static string? ExtractGitHubRepoPath(string remoteUrl)
    {
        if (string.IsNullOrEmpty(remoteUrl)) return null;

        remoteUrl = remoteUrl.Trim();
        if (remoteUrl.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            remoteUrl = remoteUrl[..^4];

        // 支持：https://github.com/owner/repo  git@github.com:owner/repo
        var m = Regex.Match(
            remoteUrl, @"github\.com[/:]([^/]+)/([^/]+)$",
            RegexOptions.IgnoreCase);

        if (m.Success)
            return $"{m.Groups[1].Value}/{m.Groups[2].Value}";

        return null;
    }
}