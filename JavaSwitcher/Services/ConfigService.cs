using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using JavaSwitcher.Models;

namespace JavaSwitcher.Services
{
    /// <summary>
    /// 配置持久化服务
    /// </summary>
    public class ConfigService
    {
        private readonly string _configDirectory;
        private readonly string _configFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public ConfigService()
        {
            // 配置文件存储在 %APPDATA%\JavaSwitcher\config.json
            _configDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JavaSwitcher"
            );
            _configFilePath = Path.Combine(_configDirectory, "config.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// 加载 Java 版本列表
        /// </summary>
        public async Task<List<JavaVersion>> LoadJavaVersionsAsync()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    return new List<JavaVersion>();
                }

                var json = await File.ReadAllTextAsync(_configFilePath);
                var config = JsonSerializer.Deserialize<ConfigData>(json, _jsonOptions);
                return config?.JavaVersions ?? new List<JavaVersion>();
            }
            catch (Exception ex)
            {
                // 如果读取失败,返回空列表
                Console.WriteLine($"加载配置失败: {ex.Message}");
                return new List<JavaVersion>();
            }
        }

        /// <summary>
        /// 保存 Java 版本列表
        /// </summary>
        public async Task SaveJavaVersionsAsync(List<JavaVersion> versions)
        {
            try
            {
                // 确保配置目录存在
                if (!Directory.Exists(_configDirectory))
                {
                    Directory.CreateDirectory(_configDirectory);
                }

                var config = new ConfigData { JavaVersions = versions };
                var json = JsonSerializer.Serialize(config, _jsonOptions);
                await File.WriteAllTextAsync(_configFilePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"保存配置失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 配置数据结构
        /// </summary>
        private class ConfigData
        {
            public List<JavaVersion> JavaVersions { get; set; } = new();
        }
    }
}
