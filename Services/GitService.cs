using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace FluentGit.Services;

public static class GitService
{
    /// <summary>
    /// 执行 git init
    /// </summary>
    public static bool InitRepository(string gitExePath, string workingDirectory)
    {
        if (!File.Exists(gitExePath))
            throw new FileNotFoundException("git.exe 未找到", gitExePath);
        if (!Directory.Exists(workingDirectory))
            throw new DirectoryNotFoundException("目标目录不存在");

        try
        {
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
                return true;
            else
                throw new Exception($"Git init 失败: {error}");
        }
        catch (Exception ex)
        {
            throw new Exception($"执行 git init 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 检查目录是否已是一个 Git 仓库（存在 .git 子目录）
    /// </summary>
    public static bool IsGitRepository(string directory)
    {
        return Directory.Exists(Path.Combine(directory, ".git"));
    }

    /// <summary>
    /// 获取仓库所有本地分支
    /// </summary>
    public static List<string> GetBranches(string repoPath, string gitExePath)
    {
        if (!IsGitRepository(repoPath))
            throw new Exception("不是有效的 Git 仓库");

        var process = new Process();
        process.StartInfo.FileName = gitExePath;
        process.StartInfo.Arguments = "branch --format=%(refname:short)";
        process.StartInfo.WorkingDirectory = repoPath;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new Exception("获取分支失败");

        return output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    /// <summary>
    /// 执行 git clone
    /// </summary>
    public static bool CloneRepository(string gitExePath, string remoteUrl, string targetDirectory)
    {
        if (!File.Exists(gitExePath))
            throw new FileNotFoundException("git.exe 未找到", gitExePath);
        if (Directory.Exists(targetDirectory) && Directory.GetFileSystemEntries(targetDirectory).Length > 0)
            throw new Exception("目标目录非空，请选择空目录或指定新目录");

        try
        {
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
                return true;
            else
                throw new Exception($"克隆失败: {error}");
        }
        catch (Exception ex)
        {
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
            throw new Exception("不是有效的 Git 仓库");

        var process = new Process();
        process.StartInfo.FileName = gitExePath;
        process.StartInfo.Arguments = "status --porcelain";
        process.StartInfo.WorkingDirectory = repoPath;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new Exception("获取状态失败");

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
        return entries;
    }
}