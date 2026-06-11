namespace Mblog.API.Models.DTOs;

/// <summary>
/// 保存文生图配置请求
/// </summary>
public class SaveImageGenConfigRequest
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string AvailableModels { get; set; } = "[]";
    public string DefaultSize { get; set; } = "1024x1024";
    public string DefaultQuality { get; set; } = "standard";
    public int DefaultN { get; set; } = 1;
}

/// <summary>
/// 文生图请求
/// </summary>
public class ImageGenRequest
{
    /// <summary>提示词</summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>模型（为空则使用配置中的默认模型）</summary>
    public string? Model { get; set; }

    /// <summary>图片尺寸</summary>
    public string? Size { get; set; }

    /// <summary>图片质量</summary>
    public string? Quality { get; set; }

    /// <summary>生成数量</summary>
    public int N { get; set; } = 1;
}