using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace JavaSwitcher.Services
{
    /// <summary>
    /// 环境变量管理服务
    /// </summary>
    public class EnvironmentService
    {
        private const string JAVA_HOME = "JAVA_HOME";
        private const string PATH = "Path";

        /// <summary>
        /// 获取当前 JAVA_HOME 环境变量
        /// </summary>
        public string? GetCurrentJavaHome()
        {
            try
            {
                // 优先从系统环境变量读取
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");
                return key?.GetValue(JAVA_HOME)?.ToString();
            }
            catch
            {
                // 如果无法访问注册表,从进程环境变量读取
                return Environment.GetEnvironmentVariable(JAVA_HOME, EnvironmentVariableTarget.Machine);
            }
        }

        /// <summary>
        /// 设置 JAVA_HOME 环境变量
        /// </summary>
        public void SetJavaHome(string javaPath)
        {
            try
            {
                // 设置系统环境变量
                Environment.SetEnvironmentVariable(JAVA_HOME, javaPath, EnvironmentVariableTarget.Machine);
                
                // 同时更新注册表
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", true);
                if (key != null)
                {
                    key.SetValue(JAVA_HOME, javaPath, RegistryValueKind.ExpandString);
                }

                // 广播环境变量更改消息
                BroadcastEnvironmentChange();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"设置 JAVA_HOME 失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更新 Path 环境变量中的 Java bin 目录
        /// </summary>
        public void UpdatePathVariable(string javaPath)
        {
            try
            {
                var javaBinPath = Path.Combine(javaPath, "bin");
                
                // 获取当前 Path
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", true);
                if (key == null)
                {
                    throw new InvalidOperationException("无法访问系统环境变量");
                }

                var currentPath = key.GetValue(PATH)?.ToString() ?? string.Empty;
                var pathEntries = currentPath.Split(';').ToList();

                // 移除所有旧的 Java bin 路径
                pathEntries = pathEntries.Where(p => 
                    !p.Contains(@"\bin", StringComparison.OrdinalIgnoreCase) || 
                    !IsJavaPath(p)).ToList();

                // 添加新的 Java bin 路径到开头
                pathEntries.Insert(0, javaBinPath);

                // 更新 Path
                var newPath = string.Join(";", pathEntries);
                key.SetValue(PATH, newPath, RegistryValueKind.ExpandString);
                Environment.SetEnvironmentVariable(PATH, newPath, EnvironmentVariableTarget.Machine);

                // 广播环境变量更改消息
                BroadcastEnvironmentChange();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"更新 Path 失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 验证 Java 路径是否有效
        /// </summary>
        public bool ValidateJavaPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                return false;
            }

            // 检查是否存在 java.exe
            var javaExePath = Path.Combine(path, "bin", "java.exe");
            return File.Exists(javaExePath);
        }

        /// <summary>
        /// 获取指定路径的 Java 版本信息
        /// </summary>
        public string GetJavaVersion(string javaPath)
        {
            try
            {
                var javaExePath = Path.Combine(javaPath, "bin", "java.exe");
                if (!File.Exists(javaExePath))
                {
                    return "未知版本";
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = javaExePath,
                    Arguments = "-version",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    return "未知版本";
                }

                var output = process.StandardError.ReadToEnd();
                process.WaitForExit();

                // 解析版本号,例如: java version "1.8.0_462" 或 java version "21.0.7"
                var match = Regex.Match(output, @"version ""(.+?)""");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }

                return "未知版本";
            }
            catch
            {
                return "未知版本";
            }
        }

        /// <summary>
        /// 自动检测常见位置的 Java 安装
        /// </summary>
        public List<string> AutoDetectJavaInstallations()
        {
            var javaPaths = new List<string>();
            var searchPaths = new[]
            {
                @"C:\Program Files\Eclipse Adoptium",
                @"C:\Program Files\Java",
                @"C:\Program Files (x86)\Java",
                @"C:\Program Files\AdoptOpenJDK",
                @"C:\Program Files\Zulu"
            };

            foreach (var searchPath in searchPaths)
            {
                if (!Directory.Exists(searchPath))
                {
                    continue;
                }

                try
                {
                    var directories = Directory.GetDirectories(searchPath);
                    foreach (var dir in directories)
                    {
                        if (ValidateJavaPath(dir))
                        {
                            javaPaths.Add(dir);
                        }
                    }
                }
                catch
                {
                    // 忽略访问错误
                }
            }

            return javaPaths;
        }

        /// <summary>
        /// 扫描指定目录查找 Java 安装
        /// </summary>
        public List<string> ScanDirectory(string directoryPath)
        {
            var javaPaths = new List<string>();

            if (!Directory.Exists(directoryPath))
            {
                return javaPaths;
            }

            try
            {
                var directories = Directory.GetDirectories(directoryPath);
                foreach (var dir in directories)
                {
                    if (ValidateJavaPath(dir))
                    {
                        javaPaths.Add(dir);
                    }
                }
            }
            catch
            {
                // 忽略访问错误
            }

            return javaPaths;
        }

        /// <summary>
        /// 判断路径是否为 Java 路径
        /// </summary>
        private bool IsJavaPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var lowerPath = path.ToLowerInvariant();
            return lowerPath.Contains("java") || 
                   lowerPath.Contains("jdk") || 
                   lowerPath.Contains("jre");
        }

        /// <summary>
        /// 广播环境变量更改消息
        /// </summary>
        private void BroadcastEnvironmentChange()
        {
            // 通知系统环境变量已更改
            // 注意: 这不会影响已经运行的程序,只影响新启动的程序
            const int HWND_BROADCAST = 0xffff;
            const uint WM_SETTINGCHANGE = 0x001a;
            
            NativeMethods.SendMessageTimeout(
                (IntPtr)HWND_BROADCAST,
                WM_SETTINGCHANGE,
                IntPtr.Zero,
                "Environment",
                2,
                5000,
                out _);
        }
    }

    /// <summary>
    /// Windows API 调用
    /// </summary>
    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            string lParam,
            uint fuFlags,
            uint uTimeout,
            out IntPtr lpdwResult);
    }
}
