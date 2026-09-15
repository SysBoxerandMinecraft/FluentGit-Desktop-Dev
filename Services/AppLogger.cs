using System.Diagnostics;

namespace FluentGit.Services;

/// <summary>
/// 统一日志输出，用 OK/Warning/Error 前缀代替 emoji，方便在调试控制台过滤
/// </summary>
public static class AppLogger
{
    private const string Prefix = "[FluentGit]";

    public static void OK(string tag, string message)
        => Debug.WriteLine($"{Prefix} [OK]      [{tag}] {message}");

    public static void Warning(string tag, string message)
        => Debug.WriteLine($"{Prefix} [Warning] [{tag}] {message}");

    public static void Error(string tag, string message)
        => Debug.WriteLine($"{Prefix} [Error]   [{tag}] {message}");

    public static void Info(string tag, string message)
        => Debug.WriteLine($"{Prefix} [Info]    [{tag}] {message}");
}