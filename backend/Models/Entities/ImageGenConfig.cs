namespace Mblog.API.Models.Entities;

/// <summary>
/// 文生图模型配置表，存储图像生成 API 连接和参数配置（单行记录）
/// </summary>
public class ImageGenConfig : BaseEntity
{
    /// <summary>API 基础地址（如 https://api.openai.com/v1）</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>API Key / Token</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>当前使用的模型名称</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>可用模型列表（JSON 数组字符串）</summary>
    public string AvailableModels { get; set; } = "[]";

    /// <summary>默认图片尺寸</summary>
    public string DefaultSize { get; set; } = "2048x2048";

    /// <summary>默认图片质量（standard / hd）</summary>
    public string DefaultQuality { get; set; } = "standard";

    /// <summary>默认生成图片数量</summary>
    public int DefaultN { get; set; } = 1;
}