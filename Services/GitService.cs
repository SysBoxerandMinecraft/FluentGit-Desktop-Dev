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

    // ========== 状态相关 ==========
    public class GitStatusEntry
    {
        public string? Path { get; set; }
        public char X { get; set; } // 暂存区状态
        public char Y { get; set; } // 工作区状态
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