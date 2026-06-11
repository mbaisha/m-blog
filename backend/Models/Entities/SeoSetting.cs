namespace Mblog.API.Models.Entities;

/// <summary>
/// SEO 配置表，每个页面独立配置 SEO 元信息
/// </summary>
public class SeoSetting : BaseEntity
{
    /// <summary>页面标识：global / home / articles / article / category / tag / archive / search / about / project / friends / message</summary>
    public string PageKey { get; set; } = string.Empty;

    /// <summary>SEO 标题</summary>
    public string? Title { get; set; }

    /// <summary>SEO 描述</summary>
    public string? Description { get; set; }

    /// <summary>SEO 关键词</summary>
    public string? Keywords { get; set; }

    /// <summary>OG 分享图媒体 ID</summary>
    public Guid? OgImageId { get; set; }

    /// <summary>权威链接 URL</summary>
    public string? CanonicalUrl { get; set; }

    /// <summary>OG 分享图导航属性</summary>
    public Media? OgImage { get; set; }
}