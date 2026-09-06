using System;
using System.IO;
using System.Text.Json;

namespace FluentGit.Services;

public class AppSettings
{
    public string? GitPath { get; set; }
    public string? GitHash { get; set; }   // 新增：存储 git.exe 的 SHA-256 哈希
    public string? AppTheme { get; set; }
}

public static class SettingsService
{
    private static readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FluentGit",
        "settings.json"
    );

    static SettingsService()
    {
        var dir = Path.GetDirectoryName(_settingsPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir!);
    }

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                string json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }
        return new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch { }
    }
}