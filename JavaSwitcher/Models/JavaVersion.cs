using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JavaSwitcher.Models
{
    /// <summary>
    /// Java 版本模型类
    /// </summary>
    public partial class JavaVersion : ObservableObject
    {
        /// <summary>
        /// 版本名称 (如 "Java 8", "Java 21")
        /// </summary>
        [ObservableProperty]
        [property: JsonPropertyName("name")]
        private string _name = string.Empty;

        /// <summary>
        /// Java 安装路径
        /// </summary>
        [ObservableProperty]
        [property: JsonPropertyName("path")]
        private string _path = string.Empty;

        /// <summary>
        /// Java 版本号 (如 "1.8.0_462", "21.0.7")
        /// </summary>
        [ObservableProperty]
        [property: JsonPropertyName("version")]
        private string _version = string.Empty;

        /// <summary>
        /// 是否为当前激活版本
        /// </summary>
        [ObservableProperty]
        [property: JsonPropertyName("isActive")]
        private bool _isActive;

        /// <summary>
        /// 获取显示文本
        /// </summary>
        [JsonIgnore]
        public string DisplayText => $"{Name} ({Version})";
    }
}
