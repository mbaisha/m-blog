namespace Mblog.API.Models.DTOs;

/// <summary>
/// SEO 配置响应 DTO
/// </summary>
public class SeoSettingResponse
{
    public Guid Id { get; set; }
    public string PageKey { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Keywords { get; set; }
    public string? OgImageUrl { get; set; }
    public string? CanonicalUrl { get; set; }
}

/// <summary>
/// 更新 SEO 配置请求 DTO
/// </summary>
public class UpdateSeoSettingRequest
{
    public string PageKey { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Keywords { get; set; }
    public Guid? OgImageId { get; set; }
    public string? CanonicalUrl { get; set; }
}