using System;
using System.IO;
using System.Text;

namespace FluentGit.Services;

/// <summary>
/// 崩溃日志落盘。只在未处理异常时调用，正常流程不写文件。
/// 输出目录：%LocalAppData%\FluentGit\logs\crash-yyyyMMdd-HHmmss.log
/// </summary>
public static class CrashLogger
{
    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FluentGit",
        "logs");

    private static readonly object _lock = new();

    public static void Log(string source, Exception? ex)
    {
        try
        {
            Directory.CreateDirectory(LogDir);
            var file = Path.Combine(LogDir, $"crash-{DateTime.Now:yyyyMMdd-HHmmss}.log");

            var sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] source={source}");
            sb.AppendLine($"OS: {Environment.OSVersion}");
            sb.AppendLine($"Runtime: {Environment.Version}");
            sb.AppendLine($"ProcessArch: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
            sb.AppendLine($"Exception: {ex?.GetType().FullName ?? "(null)"}");
            sb.AppendLine($"Message: {ex?.Message ?? "(null)"}");
            sb.AppendLine("---------- StackTrace ----------");
            sb.AppendLine(ex?.ToString() ?? "(null)");
            sb.AppendLine();

            lock (_lock)
            {
                File.AppendAllText(file, sb.ToString(), Encoding.UTF8);
            }
        }
        catch
        {
            // 崩溃日志器自己崩溃了，无路可走，只能吞掉
        }
    }
}