using System;
using System.IO;

namespace FluentGit.Services;

public static class AppState
{
    public static string? CurrentRepoPath { get; private set; }
    public static string? GitPath { get; private set; }

    public static bool IsRepoOpen =>
        !string.IsNullOrEmpty(CurrentRepoPath) && Directory.Exists(CurrentRepoPath);

    public static bool IsGitReady =>
        !string.IsNullOrEmpty(GitPath) && File.Exists(GitPath);

    public static event Action? RepoChanged;

    public static void SetRepository(string? path)
    {
        CurrentRepoPath = path;
        RepoChanged?.Invoke();
    }

    public static void SetGitPath(string? path)
    {
        GitPath = path;
    }

    public static void LoadFromSettings()
    {
        var s = SettingsService.Load();
        GitPath = s.GitPath;
    }
}