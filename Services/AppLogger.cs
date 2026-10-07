using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FluentGit.Services;

/// <summary>
/// 统一日志输出。同时写 Debug 输出和文件，方便 Run.ps1 tail。
/// 日志文件：%LocalAppData%\FluentGit\logs\app.log
/// </summary>
public static class AppLogger
{
    private const string Prefix = "[FluentGit]";

    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FluentGit", "logs");

    private static readonly string LogFile = Path.Combine(LogDir, "app.log");

    private static readonly object _lock = new();
    private static bool _fileLoggingEnabled = true;

    static AppLogger()
    {
        try
        {
            Directory.CreateDirectory(LogDir);
        }
        catch
        {
            _fileLoggingEnabled = false;
        }
    }

    public static void OK(string tag, string message)      => Write("OK     ", tag, message);
    public static void Warning(string tag, string message) => Write("Warning", tag, message);
    public static void Error(string tag, string message)   => Write("Error  ", tag, message);
    public static void Info(string tag, string message)    => Write("Info   ", tag, message);

    private static void Write(string level, string tag, string message)
    {
        string line = $"{Prefix} [{level}] [{tag}] {message}";

        // 1. Debug 输出（VS / DebugView 可见）
        Debug.WriteLine(line);

        // 2. 文件输出（Run.ps1 tail 可见）
        if (!_fileLoggingEnabled) return;

        try
        {
            string timestamped = $"[{DateTime.Now:HH:mm:ss.fff}] {line}\n";
            lock (_lock)
            {
                File.AppendAllText(LogFile, timestamped, Encoding.UTF8);
            }
        }
        catch
        {
            // 日志写入失败不再抛，避免死循环
        }
    }
}