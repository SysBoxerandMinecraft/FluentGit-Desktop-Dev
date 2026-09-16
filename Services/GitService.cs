using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace FluentGit.Services;

public static class GitService
{
    private const string TAG = "GitService";

    // ========== 初始化仓库 ==========
    public static bool InitRepository(string gitExePath, string workingDirectory)
    {
        if (!File.Exists(gitExePath))
            throw new FileNotFoundException("git.exe 未找到", gitExePath);
        if (!Directory.Exists(workingDirectory))
            throw new DirectoryNotFoundException("目标目录不存在");

        try
        {
            AppLogger.Info(TAG, $"开始 git init: {workingDirectory}");

            var process = new Process();
            process.StartInfo.FileName = gitExePath;
            process.StartInfo.Arguments = "init";
            process.StartInfo.WorkingDirectory = workingDirectory;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                AppLogger.OK(TAG, $"git init 成功: {workingDirectory}");
                return true;
            }

            AppLogger.Error(TAG, $"git init 失败: {error}");
            throw new Exception($"Git init 失败: {error}");
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"git init 异常: {ex.Message}");
            throw new Exception($"执行 git init 异常: {ex.Message}");
        }
    }

    // ========== 检查是否为 Git 仓库 ==========
    public static bool IsGitRepository(string directory)
    {
        try
        {
            return Directory.Exists(Path.Combine(directory, ".git"));
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"IsGitRepository 异常: {ex.Message}");
            return false;
        }
    }

    // ========== 检查仓库是否配置了远程 ==========
    public static bool HasRemote(string gitExePath, string repoPath)
    {
        try
        {
            var process = new Process();
            process.StartInfo.FileName = gitExePath;
            process.StartInfo.Arguments = "remote";
            process.StartInfo.WorkingDirectory = repoPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            bool hasRemote = process.ExitCode == 0 && !string.IsNullOrEmpty(output);
            AppLogger.Info(TAG, $"HasRemote: {hasRemote} ({repoPath})");
            return hasRemote;
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"HasRemote 异常: {ex.Message}");
            return false;
        }
    }

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

            var process = new Process();
            process.StartInfo.FileName = gitExePath;
            process.StartInfo.Arguments = "branch --format=%(refname:short)";
            process.StartInfo.WorkingDirectory = repoPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
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

    // ========== 克隆仓库 ==========
    public static bool CloneRepository(string gitExePath, string remoteUrl, string targetDirectory)
    {
        if (!File.Exists(gitExePath))
            throw new FileNotFoundException("git.exe 未找到", gitExePath);
        if (Directory.Exists(targetDirectory) && Directory.GetFileSystemEntries(targetDirectory).Length > 0)
        {
            AppLogger.Error(TAG, $"目标目录非空: {targetDirectory}");
            throw new Exception("目标目录非空，请选择空目录或指定新目录");
        }

        try
        {
            AppLogger.Info(TAG, $"开始 git clone: {remoteUrl} → {targetDirectory}");

            var process = new Process();
            process.StartInfo.FileName = gitExePath;
            process.StartInfo.Arguments = $"clone \"{remoteUrl}\" \"{targetDirectory}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                AppLogger.OK(TAG, $"git clone 成功: {remoteUrl}");
                return true;
            }

            AppLogger.Error(TAG, $"git clone 失败: {error}");
            throw new Exception($"克隆失败: {error}");
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"git clone 异常: {ex.Message}");
            throw new Exception($"执行 git clone 异常: {ex.Message}");
        }
    }

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

            var process = new Process();
            process.StartInfo.FileName = gitExePath;
            process.StartInfo.Arguments = "pull";
            process.StartInfo.WorkingDirectory = repoPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            string combined = output + "\n" + error;

            if (process.ExitCode == 0)
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

            var process = new Process();
            process.StartInfo.FileName = gitExePath;
            process.StartInfo.Arguments = "fetch --all";
            process.StartInfo.WorkingDirectory = repoPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            string combined = output + "\n" + error;

            if (process.ExitCode == 0)
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

    // ========== 状态相关 ==========
    public class GitStatusEntry
    {
        public string? Path { get; set; }
        public char X { get; set; }
        public char Y { get; set; }
    }

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

            var process = new Process();
            process.StartInfo.FileName = gitExePath;
            process.StartInfo.Arguments = "status --porcelain";
            process.StartInfo.WorkingDirectory = repoPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
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
}