using Avalonia.Controls;
using Avalonia.Platform.Storage;
using JavaSwitcher.ViewModels;
using System.Linq;

namespace JavaSwitcher.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 手动添加 Java 版本
        /// </summary>
        private async void OnAddVersionClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;
            if (viewModel == null) return;

            // 打开文件夹选择对话框
            var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "选择 Java 安装目录",
                AllowMultiple = false
            });

            if (folder.Count > 0)
            {
                var selectedPath = folder[0].Path.LocalPath;
                await viewModel.AddJavaVersionCommand.ExecuteAsync(selectedPath);
            }
        }
    }
}