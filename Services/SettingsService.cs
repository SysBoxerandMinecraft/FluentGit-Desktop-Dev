using System;
using System.IO;
using System.Text.Json;

namespace FluentGit.Services;

public class AppSettings
{
    public string? GitPath { get; set; }
    public string? GitHash { get; set; }
    public string? AppTheme { get; set; }
    public bool FilterBuildArtifacts { get; set; } = true;
    public string? BackdropType { get; set; }
}

public static class SettingsService
{
    private const string TAG = "SettingsService";

    private static readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FluentGit",
        "settings.json"
    );

    static SettingsService()
    {
        try
        {
            var dir = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"创建设置目录失败: {ex.Message}");
        }
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
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"读取设置失败: {ex.Message}");
        }
        return new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });

            var dir = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(_settingsPath))
            {
                string tmp = _settingsPath + ".tmp";
                File.WriteAllText(tmp, json);
                File.Replace(tmp, _settingsPath, null, ignoreMetadataErrors: true);
            }
            else
            {
                File.WriteAllText(_settingsPath, json);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error(TAG, $"保存设置失败: {ex.Message}");
        }
    }
}