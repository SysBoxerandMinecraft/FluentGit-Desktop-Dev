using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Cryptography;

namespace FluentGit.Services
{
    public static class GitPathHelper
    {
        private const string TAG = "GitPathHelper";
        public const string ExpectedGitHash = "c470d205517c7a53ceca321df16a6e4549fcd52b576ab4d09536d36f26fda5a9";

        public static string? FindGitPath()
        {
            try
            {
                string? installPath = FindGitFromRegistry();
                if (!string.IsNullOrEmpty(installPath))
                {
                    AppLogger.OK(TAG, $"从注册表找到 Git: {installPath}");
                    return installPath;
                }

                string[] defaultPaths = new string[]
                {
                    @"C:\Program Files\Git\bin\git.exe",
                    @"C:\Program Files (x86)\Git\bin\git.exe"
                };

                foreach (var path in defaultPaths)
                {
                    if (File.Exists(path))
                    {
                        AppLogger.OK(TAG, $"从默认路径找到 Git: {path}");
                        return path;
                    }
                }

                var fromPath = FindGitInPath();
                if (!string.IsNullOrEmpty(fromPath))
                    AppLogger.OK(TAG, $"从 PATH 找到 Git: {fromPath}");
                else
                    AppLogger.Warning(TAG, "未找到 Git");

                return fromPath;
            }
            catch (Exception ex)
            {
                AppLogger.Error(TAG, $"FindGitPath 异常: {ex.Message}");
                return null;
            }
        }

        private static string? FindGitFromRegistry()
        {
            string[] registryPaths = new string[]
            {
                @"SOFTWARE\GitForWindows",
                @"SOFTWARE\Git-Cheetah",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Git_is1"
            };

            string? path = TryGetRegistryValue(RegistryView.Registry64, registryPaths);
            if (!string.IsNullOrEmpty(path)) return path;

            path = TryGetRegistryValue(RegistryView.Registry32, registryPaths);
            return path;
        }

        private static string? TryGetRegistryValue(RegistryView view, string[] registryPaths)
        {
            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                {
                    foreach (var path in registryPaths)
                    {
                        using (var key = baseKey.OpenSubKey(path))
                        {
                            if (key != null)
                            {
                                string? installPath = key.GetValue("InstallPath") as string;
                                if (!string.IsNullOrEmpty(installPath))
                                    return NormalizeGitPath(installPath);

                                string? installLocation = key.GetValue("InstallLocation") as string;
                                if (!string.IsNullOrEmpty(installLocation))
                                    return NormalizeGitPath(installLocation);
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private static string? NormalizeGitPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            if (Directory.Exists(path))
            {
                string fullPath = Path.Combine(path, "bin", "git.exe");
                if (File.Exists(fullPath)) return fullPath;
                fullPath = Path.Combine(path, "git.exe");
                if (File.Exists(fullPath)) return fullPath;
            }

            if (File.Exists(path) && Path.GetFileName(path).Equals("git.exe", StringComparison.OrdinalIgnoreCase))
                return path;

            return null;
        }

        private static string? FindGitInPath()
        {
            try
            {
                var pathEnv = Environment.GetEnvironmentVariable("PATH");
                if (string.IsNullOrEmpty(pathEnv)) return null;

                var paths = pathEnv.Split(Path.PathSeparator);
                foreach (var dir in paths)
                {
                    try
                    {
                        string gitPath = Path.Combine(dir, "git.exe");
                        if (File.Exists(gitPath)) return gitPath;
                    }
                    catch { }
                }
            }
            catch { }
            return null;
        }

        public static string? ComputeFileHash(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                byte[] hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch
            {
                return null;
            }
        }

        public static bool ValidateGitPath(string gitPath, string? expectedHash)
        {
            if (!File.Exists(gitPath)) return false;
            if (string.IsNullOrEmpty(expectedHash)) return true;
            var actualHash = ComputeFileHash(gitPath);
            return !string.IsNullOrEmpty(actualHash) &&
                   actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}