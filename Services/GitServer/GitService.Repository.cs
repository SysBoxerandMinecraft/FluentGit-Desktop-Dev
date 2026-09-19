using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace FluentGit.Services;

public static partial class GitService
{
    // 当前正在执行的克隆进程（用于取消）
    private static System.Diagnostics.Process? _currentCloneProcess;
    private static readonly object _cloneProcessLock = new();

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
            var (code, output, error) = RunGit(gitExePath, "init", workingDirectory);

            if (code == 0)
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

    // ========== 克隆仓库（无进度） ==========
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
            var args = $"clone \"{remoteUrl}\" \"{targetDirectory}\"";
            var (code, output, error) = RunGit(gitExePath, args, Directory.GetCurrentDirectory());

            if (code == 0)
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

    // ========== 克隆仓库（带进度 + 可取消） ==========
    public static bool CloneRepositoryWithProgress(
        string gitExePath,
        string remoteUrl,
        string targetDirectory,
        Action<int, string>? onProgress,
        CancellationToken token = default)
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
            AppLogger.Info(TAG, $"开始 git clone (带进度): {remoteUrl} → {targetDirectory}");

            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = gitExePath;
            process.StartInfo.Arguments = $"clone --progress \"{remoteUrl}\" \"{targetDirectory}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    ParseGitProgress(e.Data, onProgress);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    ParseGitProgress(e.Data, onProgress);
            };

            lock (_cloneProcessLock)
            {
                _currentCloneProcess = process;
            }

            token.Register(() =>
            {
                try
                {
                    lock (_cloneProcessLock)
                    {
                        if (_currentCloneProcess != null && !_currentCloneProcess.HasExited)
                        {
                            _currentCloneProcess.Kill(entireProcessTree: true);
                            AppLogger.Warning(TAG, "克隆被用户取消");
                        }
                    }
                }
                catch { }
            });

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            lock (_cloneProcessLock)
            {
                _currentCloneProcess = null;
            }

            if (token.IsCancellationRequested)
            {
                try
                {
                    if (Directory.Exists(targetDirectory))
                        Directory.Delete(targetDirectory, true);
                }
                catch { }
                throw new OperationCanceledException("克隆已取消");
            }

            if (process.ExitCode == 0)
            {
                onProgress?.Invoke(100, "克隆完成");
                AppLogger.OK(TAG, $"git clone 成功: {remoteUrl}");
                return true;
            }

            AppLogger.Error(TAG, $"git clone 失败 (exit {process.ExitCode})");
            throw new Exception($"克隆失败 (退出码 {process.ExitCode})");
        }
        catch (OperationCanceledException)
        {
            lock (_cloneProcessLock) { _currentCloneProcess = null; }
            throw;
        }
        catch (Exception ex)
        {
            lock (_cloneProcessLock) { _currentCloneProcess = null; }
            AppLogger.Error(TAG, $"git clone 异常: {ex.Message}");
            throw new Exception($"执行 git clone 异常: {ex.Message}");
        }
    }

    // ========== 解析 git 进度输出 ==========
    private static void ParseGitProgress(string line, Action<int, string>? onProgress)
    {
        if (onProgress == null) return;

        var match = Regex.Match(line, @"(\d+)%");
        if (!match.Success) return;
        if (!int.TryParse(match.Groups[1].Value, out int rawPercent)) return;

        int mappedPercent;
        string phase;

        if (line.Contains("Receiving objects"))
        {
            mappedPercent = (int)(rawPercent * 0.6);
            phase = "接收对象";
        }
        else if (line.Contains("Resolving deltas"))
        {
            mappedPercent = 60 + (int)(rawPercent * 0.3);
            phase = "解析差异";
        }
        else if (line.Contains("Checking out files"))
        {
            mappedPercent = 90 + (int)(rawPercent * 0.1);
            phase = "检出文件";
        }
        else
        {
            return;
        }

        onProgress(mappedPercent, $"{phase}... {mappedPercent}%");
    }

    // ========== 杀掉残留的 git 进程 ==========
    public static void KillLingeringGitProcesses()
    {
        string[] processNames = {
            "git", "git-remote-https", "git-remote-http",
            "git-remote-ssh", "git-remote-gcrypt", "git-lfs"
        };

        foreach (var name in processNames)
        {
            try
            {
                var procs = System.Diagnostics.Process.GetProcessesByName(name);
                foreach (var p in procs)
                {
                    try
                    {
                        p.Kill();
                        p.WaitForExit(1000);
                        AppLogger.Info(TAG, $"已杀掉残留进程: {name} (PID={p.Id})");
                    }
                    catch { }
                }
            }
            catch { }
        }
    }

    // ========== 强制删除目录（处理临时文件锁定） ==========
    public static bool ForceDeleteDirectory(string path, int maxRetries = 5)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                if (!Directory.Exists(path))
                    return true;

                foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    try { File.SetAttributes(file, FileAttributes.Normal); } catch { }
                }

                Directory.Delete(path, true);
                AppLogger.OK(TAG, $"目录已删除: {path} (第 {attempt} 次尝试)");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Warning(TAG, $"删除目录失败 (第 {attempt}/{maxRetries} 次): {ex.Message}");
                if (attempt < maxRetries)
                    System.Threading.Thread.Sleep(500);
            }
        }
        return false;
    }
}