using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace FluentGit.Services
{
    public static class GitPathHelper
    {
        private const string TAG = "GitPathHelper";

        // 保留常量（兼容旧代码），签名验证下不再使用
        public const string ExpectedGitHash = "";

        // 支持的 Git 相关 exe 文件名
        public static readonly string[] AcceptedGitExecutables =
        {
            "git.exe", "git-bash.exe", "git-cmd.exe", "sh.exe", "bash.exe"
        };

        // Git for Windows 官方签名者关键字
        private static readonly string[] TrustedSignerKeywords =
        {
            "Johannes Schindelin",
            "Git for Windows",
            "Open Source Developer"
        };

        // ========== 主入口：自动查找 ==========
        public static string? FindGitPath()
        {
            try
            {
                string? installPath = FindGitFromRegistry();
                if (!string.IsNullOrEmpty(installPath))
                {
                    var resolved = ResolveToGitExe(installPath);
                    if (!string.IsNullOrEmpty(resolved) && ValidateGitPath(resolved))
                    {
                        AppLogger.OK(TAG, $"从注册表找到 Git: {resolved}");
                        return resolved;
                    }
                }

                string[] defaultPaths = new string[]
                {
                    @"C:\Program Files\Git\bin\git.exe",
                    @"C:\Program Files (x86)\Git\bin\git.exe",
                };

                foreach (var path in defaultPaths)
                {
                    if (File.Exists(path) && ValidateGitPath(path))
                    {
                        AppLogger.OK(TAG, $"从默认路径找到 Git: {path}");
                        return path;
                    }
                }

                var fromPath = FindGitInPath();
                if (!string.IsNullOrEmpty(fromPath) && ValidateGitPath(fromPath))
                {
                    AppLogger.OK(TAG, $"从 PATH 找到 Git: {fromPath}");
                    return fromPath;
                }

                AppLogger.Warning(TAG, "未找到有效的 Git");
                return null;
            }
            catch (Exception ex)
            {
                AppLogger.Error(TAG, $"FindGitPath 异常: {ex.Message}");
                return null;
            }
        }

        // ========== 签名验证（主入口） ==========
        public static bool ValidateGitPath(string gitPath, string? expectedHash = null)
        {
            if (string.IsNullOrEmpty(gitPath) || !File.Exists(gitPath))
                return false;

            return ValidateGitSignature(gitPath);
        }

        /// <summary>
        /// 验证文件是否由 Git for Windows 官方签名
        /// 分两步：
        /// 1. WinVerifyTrust 验证签名有效性（不联网）
        /// 2. 读取签名者 Subject，确认是 Git for Windows 官方
        /// </summary>
        /// <summary>
        /// 验证文件是否由 Git for Windows 官方签名
        /// 分两步：
        /// 1. WinVerifyTrust 验证签名有效性（不联网）
        /// 2. 读取签名者 Subject，确认签名者是 Johannes Schindelin
        /// </summary>
        private static bool ValidateGitSignature(string exePath)
        {
            try
            {
                // Step 1: 文件名必须在白名单
                string fileName = Path.GetFileName(exePath);
                bool acceptedName = false;
                foreach (var accepted in AcceptedGitExecutables)
                {
                    if (string.Equals(fileName, accepted, StringComparison.OrdinalIgnoreCase))
                    {
                        acceptedName = true;
                        break;
                    }
                }
                if (!acceptedName)
                {
                    AppLogger.Warning(TAG, $"文件名不在白名单: {fileName}");
                    return false;
                }

                // Step 2: 用 WinVerifyTrust 验证签名有效性
                if (!WinVerifyTrustCheck(exePath))
                {
                    AppLogger.Warning(TAG, $"WinVerifyTrust 验证失败: {exePath}");
                    return false;
                }
                AppLogger.OK(TAG, $"WinVerifyTrust 通过: {fileName}");

                // Step 3: 确认签名者就是 Johannes Schindelin
                string subject = GetSignerSubject(exePath);
                if (string.IsNullOrEmpty(subject))
                {
                    AppLogger.Warning(TAG, $"无法读取签名者信息: {exePath}");
                    return false;
                }

                if (!subject.Contains("Johannes Schindelin", StringComparison.OrdinalIgnoreCase))
                {
                    AppLogger.Warning(TAG, $"签名者不是 Johannes Schindelin: {subject}");
                    return false;
                }

                AppLogger.OK(TAG, $"签名验证通过: {fileName} / {subject}");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error(TAG, $"ValidateGitSignature 异常: {ex.Message}");
                return false;
            }
        }

        // ========== WinVerifyTrust（不联网的签名验证） ==========
        private static readonly Guid WINTRUST_ACTION_GENERIC_VERIFY_V2 =
            new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

        [DllImport("wintrust.dll", PreserveSig = true, SetLastError = false, ExactSpelling = true)]
        private static extern uint WinVerifyTrust(
            IntPtr hwnd,
            [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID,
            IntPtr pWVTData);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WINTRUST_FILE_INFO
        {
            public uint cbStruct;
            public string pcwszFilePath;
            public IntPtr hFile;
            public IntPtr pgKnownSubject;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WINTRUST_DATA
        {
            public uint cbStruct;
            public IntPtr pPolicyCallbackData;
            public IntPtr pSIPClientData;
            public uint dwUIChoice;
            public uint fdwRevocationChecks;
            public uint dwUnionChoice;
            public IntPtr pFile;
            public uint dwStateAction;
            public IntPtr hWVTStateData;
            public IntPtr pwszURLReference;
            public uint dwProvFlags;
            public uint dwUIContext;
            public IntPtr pSignatureSettings;
        }

        private static bool WinVerifyTrustCheck(string filePath)
        {
            IntPtr pFileInfo = IntPtr.Zero;
            IntPtr pWvtData = IntPtr.Zero;
            try
            {
                var fileInfo = new WINTRUST_FILE_INFO
                {
                    cbStruct = (uint)Marshal.SizeOf<WINTRUST_FILE_INFO>(),
                    pcwszFilePath = filePath,
                    hFile = IntPtr.Zero,
                    pgKnownSubject = IntPtr.Zero
                };
                pFileInfo = Marshal.AllocHGlobal(Marshal.SizeOf<WINTRUST_FILE_INFO>());
                Marshal.StructureToPtr(fileInfo, pFileInfo, false);

                var wvtData = new WINTRUST_DATA
                {
                    cbStruct = (uint)Marshal.SizeOf<WINTRUST_DATA>(),
                    dwUIChoice = 2,            // WTD_UI_NONE
                    fdwRevocationChecks = 0,   // WTD_REVOKE_NONE - 不做吊销检查（关键！）
                    dwUnionChoice = 1,         // WTD_CHOICE_FILE
                    pFile = pFileInfo,
                    dwStateAction = 0,         // WTD_STATEACTION_IGNORE
                    dwProvFlags = 0x00000010   // WTD_CACHE_ONLY_URL_RETRIEVAL - 不联网
                };
                pWvtData = Marshal.AllocHGlobal(Marshal.SizeOf<WINTRUST_DATA>());
                Marshal.StructureToPtr(wvtData, pWvtData, false);

                uint result = WinVerifyTrust(IntPtr.Zero, WINTRUST_ACTION_GENERIC_VERIFY_V2, pWvtData);
                return result == 0;  // ERROR_SUCCESS
            }
            catch
            {
                return false;
            }
            finally
            {
                if (pWvtData != IntPtr.Zero) Marshal.FreeHGlobal(pWvtData);
                if (pFileInfo != IntPtr.Zero) Marshal.FreeHGlobal(pFileInfo);
            }
        }

        // ========== 读取签名者 Subject ==========
        private static string GetSignerSubject(string exePath)
        {
            try
            {
#pragma warning disable SYSLIB0057
                using var baseCert = X509Certificate.CreateFromSignedFile(exePath);
#pragma warning restore SYSLIB0057
                return baseCert.Subject ?? "";
            }
            catch (Exception ex)
            {
                AppLogger.Warning(TAG, $"读取证书 Subject 失败: {ex.Message}");
                return "";
            }
        }

        // ========== 从任意 Git exe 推导 git.exe ==========
        public static string? ResolveToGitExe(string anyGitExe)
        {
            if (string.IsNullOrEmpty(anyGitExe) || !File.Exists(anyGitExe))
                return null;

            string fileName = Path.GetFileName(anyGitExe);
            string? dir = Path.GetDirectoryName(anyGitExe);
            if (string.IsNullOrEmpty(dir)) return null;

            if (string.Equals(fileName, "git.exe", StringComparison.OrdinalIgnoreCase))
                return anyGitExe;

            if (string.Equals(fileName, "bash.exe", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fileName, "sh.exe", StringComparison.OrdinalIgnoreCase))
            {
                string candidate = Path.Combine(dir, "git.exe");
                if (File.Exists(candidate)) return candidate;
            }

            if (string.Equals(fileName, "git-bash.exe", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fileName, "git-cmd.exe", StringComparison.OrdinalIgnoreCase))
            {
                string[] candidates = new[]
                {
                    Path.Combine(dir, "bin", "git.exe"),
                    Path.Combine(dir, "cmd", "git.exe"),
                };
                foreach (var c in candidates)
                {
                    if (File.Exists(c)) return c;
                }
            }

            string? parent = Path.GetDirectoryName(dir);
            if (!string.IsNullOrEmpty(parent))
            {
                string candidate = Path.Combine(parent, "bin", "git.exe");
                if (File.Exists(candidate)) return candidate;
            }

            return null;
        }

        // ========== 注册表查找 ==========
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

                fullPath = Path.Combine(path, "git-bash.exe");
                if (File.Exists(fullPath)) return fullPath;

                fullPath = Path.Combine(path, "git.exe");
                if (File.Exists(fullPath)) return fullPath;
            }

            if (File.Exists(path))
            {
                string fileName = Path.GetFileName(path);
                foreach (var accepted in AcceptedGitExecutables)
                {
                    if (string.Equals(fileName, accepted, StringComparison.OrdinalIgnoreCase))
                        return path;
                }
            }

            return null;
        }

        // ========== PATH 环境变量查找 ==========
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

        // ========== 兼容旧接口 ==========
        public static string? ComputeFileHash(string filePath)
        {
            return null;
        }
    }
}