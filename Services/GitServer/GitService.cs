using System;
using System.Diagnostics;
using System.IO;

namespace FluentGit.Services;

public static partial class GitService
{
    private const string TAG = "GitService";

    // ========== 是否为 Git 仓库 ==========
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

    // ========== 是否配置了远程 ==========
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

    // ========== 通用：执行 git 命令并返回结果 ==========
    private static (int ExitCode, string Output, string Error) RunGit(
        string gitExePath, string arguments, string workingDirectory)
    {
        var process = new Process();
        process.StartInfo.FileName = gitExePath;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return (process.ExitCode, output, error);
    }
}