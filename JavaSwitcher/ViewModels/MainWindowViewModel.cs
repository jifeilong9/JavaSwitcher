using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JavaSwitcher.Models;
using JavaSwitcher.Services;

namespace JavaSwitcher.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly ConfigService _configService;
        private readonly EnvironmentService _environmentService;

        [ObservableProperty]
        private ObservableCollection<JavaVersion> _javaVersions = new();

        [ObservableProperty]
        private JavaVersion? _selectedVersion;

        [ObservableProperty]
        private string _currentJavaHome = "未设置";

        [ObservableProperty]
        private string _statusMessage = "就绪";

        [ObservableProperty]
        private bool _isLoading;

        public MainWindowViewModel()
        {
            _configService = new ConfigService();
            _environmentService = new EnvironmentService();

            // 异步加载数据
            _ = InitializeAsync();
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private async Task InitializeAsync()
        {
            await LoadJavaVersionsAsync();
            RefreshCurrentVersion();
        }

        /// <summary>
        /// 加载 Java 版本列表
        /// </summary>
        [RelayCommand]
        private async Task LoadJavaVersionsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "正在加载配置...";

                var versions = await _configService.LoadJavaVersionsAsync();
                JavaVersions.Clear();
                foreach (var version in versions)
                {
                    JavaVersions.Add(version);
                }

                StatusMessage = $"已加载 {JavaVersions.Count} 个 Java 版本";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 刷新当前激活的 Java 版本
        /// </summary>
        [RelayCommand]
        private void RefreshCurrentVersion()
        {
            try
            {
                var currentHome = _environmentService.GetCurrentJavaHome();
                CurrentJavaHome = string.IsNullOrEmpty(currentHome) ? "未设置" : currentHome;

                // 更新激活状态
                foreach (var version in JavaVersions)
                {
                    version.IsActive = version.Path.Equals(currentHome, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"刷新失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 切换到选中的 Java 版本
        /// </summary>
        [RelayCommand]
        private async Task SwitchJavaVersionAsync()
        {
            if (SelectedVersion == null)
            {
                StatusMessage = "请先选择一个 Java 版本";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"正在切换到 {SelectedVersion.Name}...";

                // 验证路径
                if (!_environmentService.ValidateJavaPath(SelectedVersion.Path))
                {
                    StatusMessage = $"Java 路径无效: {SelectedVersion.Path}";
                    return;
                }

                // 设置环境变量
                _environmentService.SetJavaHome(SelectedVersion.Path);
                _environmentService.UpdatePathVariable(SelectedVersion.Path);

                // 更新激活状态
                foreach (var version in JavaVersions)
                {
                    version.IsActive = version == SelectedVersion;
                }

                // 保存配置
                await _configService.SaveJavaVersionsAsync(JavaVersions.ToList());

                RefreshCurrentVersion();
                StatusMessage = $"已切换到 {SelectedVersion.Name}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"切换失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 添加新的 Java 版本
        /// </summary>
        [RelayCommand]
        private async Task AddJavaVersionAsync(string? javaPath)
        {
            if (string.IsNullOrWhiteSpace(javaPath))
            {
                StatusMessage = "请提供 Java 路径";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "正在添加 Java 版本...";

                // 验证路径
                if (!_environmentService.ValidateJavaPath(javaPath))
                {
                    StatusMessage = $"无效的 Java 路径: {javaPath}";
                    return;
                }

                // 检查是否已存在
                if (JavaVersions.Any(v => v.Path.Equals(javaPath, StringComparison.OrdinalIgnoreCase)))
                {
                    StatusMessage = "该 Java 版本已存在";
                    return;
                }

                // 获取版本信息
                var version = _environmentService.GetJavaVersion(javaPath);
                var name = System.IO.Path.GetFileName(javaPath);

                var newVersion = new JavaVersion
                {
                    Name = name,
                    Path = javaPath,
                    Version = version,
                    IsActive = false
                };

                JavaVersions.Add(newVersion);
                await _configService.SaveJavaVersionsAsync(JavaVersions.ToList());
                RefreshCurrentVersion();

                StatusMessage = $"已添加 {name}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"添加失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 删除选中的 Java 版本
        /// </summary>
        [RelayCommand]
        private async Task RemoveJavaVersionAsync()
        {
            if (SelectedVersion == null)
            {
                StatusMessage = "请先选择一个 Java 版本";
                return;
            }

            try
            {
                IsLoading = true;
                var name = SelectedVersion.Name;
                JavaVersions.Remove(SelectedVersion);
                await _configService.SaveJavaVersionsAsync(JavaVersions.ToList());

                SelectedVersion = null;
                StatusMessage = $"已删除 {name}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"删除失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 扫描目录查找 Java 安装
        /// </summary>
        [RelayCommand]
        private async Task ScanDirectoryAsync(string? directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                StatusMessage = "请提供扫描目录";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "正在扫描目录...";

                var foundPaths = _environmentService.ScanDirectory(directoryPath);
                var addedCount = 0;

                foreach (var path in foundPaths)
                {
                    // 检查是否已存在
                    if (JavaVersions.Any(v => v.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    var version = _environmentService.GetJavaVersion(path);
                    var name = System.IO.Path.GetFileName(path);

                    JavaVersions.Add(new JavaVersion
                    {
                        Name = name,
                        Path = path,
                        Version = version,
                        IsActive = false
                    });
                    addedCount++;
                }

                if (addedCount > 0)
                {
                    await _configService.SaveJavaVersionsAsync(JavaVersions.ToList());
                    RefreshCurrentVersion();
                    StatusMessage = $"扫描完成,添加了 {addedCount} 个 Java 版本";
                }
                else
                {
                    StatusMessage = "未发现新的 Java 安装";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"扫描失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 自动检测 Java 安装
        /// </summary>
        [RelayCommand]
        private async Task AutoDetectJavaAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "正在自动检测 Java 安装...";

                var foundPaths = _environmentService.AutoDetectJavaInstallations();
                StatusMessage = $"检测到 {foundPaths.Count} 个路径";
                
                var addedCount = 0;

                foreach (var path in foundPaths)
                {
                    // 检查是否已存在
                    if (JavaVersions.Any(v => v.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    var version = _environmentService.GetJavaVersion(path);
                    var name = System.IO.Path.GetFileName(path);

                    var newVersion = new JavaVersion
                    {
                        Name = name,
                        Path = path,
                        Version = version,
                        IsActive = false
                    };
                    
                    JavaVersions.Add(newVersion);
                    addedCount++;
                    StatusMessage = $"已添加 {addedCount}/{foundPaths.Count}: {name}";
                }

                if (addedCount > 0)
                {
                    await _configService.SaveJavaVersionsAsync(JavaVersions.ToList());
                    RefreshCurrentVersion();
                    StatusMessage = $"自动检测完成,添加了 {addedCount} 个 Java 版本,当前总数: {JavaVersions.Count}";
                }
                else
                {
                    StatusMessage = $"未发现新的 Java 安装,当前总数: {JavaVersions.Count}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"自动检测失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
