using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

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
            var (code, output, _) = RunGit(gitExePath, new[] { "remote" }, repoPath);
            bool hasRemote = code == 0 && !string.IsNullOrWhiteSpace(output);
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
    // 使用 ArgumentList，避免命令拼接带来的转义 / 注入问题
    private static (int ExitCode, string Output, string Error) RunGit(
        string gitExePath,
        IEnumerable<string> args,
        string workingDirectory)
    {
        var psi = new ProcessStartInfo(gitExePath)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        foreach (var a in args)
            psi.ArgumentList.Add(a);

        using var process = new Process { StartInfo = psi };
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return (process.ExitCode, output, error);
    }
}