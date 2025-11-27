# JavaSwitcher

一个基于 Avalonia UI 框架开发的跨平台 Java 版本管理工具。

## 📋 项目简介

JavaSwitcher 是一款用于快速切换和管理多个 Java 版本的桌面应用程序。通过直观的图形界面,帮助开发者轻松管理不同项目所需的 Java 环境。

## ✨ 主要特性

- 🖥️ **跨平台支持** - 基于 Avalonia UI,支持 Windows、macOS 和 Linux
- 🎯 **简洁界面** - 采用 Fluent Design 设计语言,提供现代化的用户体验
- ⚡ **快速切换** - 一键切换不同的 Java 版本
- 📦 **版本管理** - 统一管理系统中安装的所有 Java 版本
- 🔍 **自动检测** - 自动扫描并识别系统中已安装的 Java 环境

## 🛠️ 技术栈

- **.NET 9.0** - 最新的 .NET 运行时
- **Avalonia UI 11.3.9** - 跨平台 XAML UI 框架
- **CommunityToolkit.Mvvm 8.4.0** - MVVM 模式支持
- **C#** - 主要开发语言

## 📦 项目结构

```
JavaSwitcher/
├── Assets/              # 资源文件(图标、图片等)
├── Models/              # 数据模型
├── Services/            # 业务逻辑服务
├── ViewModels/          # 视图模型(MVVM)
├── Views/               # 视图界面(AXAML)
├── App.axaml            # 应用程序主入口
├── App.axaml.cs         # 应用程序逻辑
├── Program.cs           # 程序启动入口
└── ViewLocator.cs       # 视图定位器
```

## 🚀 快速开始

### 前置要求

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) 或更高版本
- Visual Studio 2022 / JetBrains Rider / VS Code

### 克隆项目

```bash
git clone https://github.com/jifeilong9/JavaSwitcher.git
cd JavaSwitcher
```

### 构建项目

```bash
dotnet restore
dotnet build
```

### 运行项目

```bash
dotnet run --project JavaSwitcher/JavaSwitcher.csproj
```

## 🔧 开发

### 调试模式

在 Visual Studio 中打开 `JavaSwitcher.slnx` 解决方案文件,按 F5 启动调试。

### 发布应用

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained
```

## 📝 依赖包

| 包名                       | 版本   | 说明                   |
| -------------------------- | ------ | ---------------------- |
| Avalonia                   | 11.3.9 | 核心 UI 框架           |
| Avalonia.Desktop           | 11.3.9 | 桌面平台支持           |
| Avalonia.Themes.Fluent     | 11.3.9 | Fluent 主题            |
| Avalonia.Fonts.Inter       | 11.3.9 | Inter 字体             |
| Avalonia.Controls.DataGrid | 11.3.9 | 数据表格控件           |
| Avalonia.Diagnostics       | 11.3.9 | 开发诊断工具(仅 Debug) |
| CommunityToolkit.Mvvm      | 8.4.0  | MVVM 工具包            |

## 🤝 贡献

欢迎提交 Issue 和 Pull Request!

### 贡献步骤

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📄 许可证

本项目采用 [MIT License](LICENSE) 许可证。

## 📧 联系方式

如有问题或建议,请通过以下方式联系:

- 提交 [Issue](../../issues)
- 发送邮件至: [your-email@example.com]

## 🙏 致谢

- [Avalonia UI](https://avaloniaui.net/) - 优秀的跨平台 UI 框架
- [.NET Community](https://dotnet.microsoft.com/) - 强大的开发平台
- 所有为本项目做出贡献的开发者

---

⭐ 如果这个项目对你有帮助,请给它一个 Star!
