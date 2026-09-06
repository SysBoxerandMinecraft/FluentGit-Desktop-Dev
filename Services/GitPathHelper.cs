using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Cryptography;

namespace FluentGit.Services
{
    public static class GitPathHelper
    {
        /// <summary>
        /// 自动查找 Git 的安装路径
        /// </summary>
        /// <returns>如果找到则返回 git.exe 的完整路径，否则返回 null</returns>
        public static string? FindGitPath()
        {
            // 1. 从常见的注册表位置查找
            string? installPath = FindGitFromRegistry();
            if (!string.IsNullOrEmpty(installPath))
            {
                return installPath;
            }

            // 2. 检查默认安装位置
            string[] defaultPaths = new string[]
            {
                @"C:\Program Files\Git\bin\git.exe",
                @"C:\Program Files (x86)\Git\bin\git.exe"
            };

            foreach (var path in defaultPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // 3. 从 PATH 环境变量中查找
            return FindGitInPath();
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
            if (!string.IsNullOrEmpty(path))
                return path;

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
            if (string.IsNullOrEmpty(path))
                return null;

            if (Directory.Exists(path))
            {
                string fullPath = Path.Combine(path, "bin", "git.exe");
                if (File.Exists(fullPath))
                    return fullPath;
                fullPath = Path.Combine(path, "git.exe");
                if (File.Exists(fullPath))
                    return fullPath;
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
                if (string.IsNullOrEmpty(pathEnv))
                    return null;

                var paths = pathEnv.Split(Path.PathSeparator);
                foreach (var dir in paths)
                {
                    try
                    {
                        string gitPath = Path.Combine(dir, "git.exe");
                        if (File.Exists(gitPath))
                            return gitPath;
                    }
                    catch { }
                }
            }
            catch { }
            return null;
        }

        // ========== 新增：哈希相关方法 ==========

        /// <summary>
        /// 计算文件的 SHA-256 哈希值（十六进制小写字符串）
        /// </summary>
        public static string? ComputeFileHash(string filePath)
        {
            if (!File.Exists(filePath))
                return null;
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

        /// <summary>
        /// 验证路径存在且哈希值匹配
        /// </summary>
        public static bool ValidateGitPath(string gitPath, string? expectedHash)
        {
            if (!File.Exists(gitPath))
                return false;
            if (string.IsNullOrEmpty(expectedHash))
                return true; // 没有预期哈希，只校验存在性
            var actualHash = ComputeFileHash(gitPath);
            return !string.IsNullOrEmpty(actualHash) &&
                   actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}