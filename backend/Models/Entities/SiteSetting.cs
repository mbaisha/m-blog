namespace Mblog.API.Models.Entities;

/// <summary>
/// 站点设置表，存储博客站点基础配置信息（单行记录）
/// </summary>
public class SiteSetting : BaseEntity
{
    /// <summary>站点名称</summary>
    public string SiteName { get; set; } = string.Empty;

    /// <summary>站点描述/Slogan</summary>
    public string? SiteDescription { get; set; }

    /// <summary>站点 Logo 媒体 ID</summary>
    public Guid? LogoImageId { get; set; }

    /// <summary>Favicon 媒体 ID</summary>
    public Guid? FaviconImageId { get; set; }

    /// <summary>评论审核开关（true=需要审核，false=直接发布）</summary>
    public bool CommentModerationEnabled { get; set; } = true;

    /// <summary>访问记录保留天数（默认 30 天）</summary>
    public int VisitRetentionDays { get; set; } = 30;

    /// <summary>是否启用邮件订阅功能</summary>
    public bool SubscriptionEnabled { get; set; }

    /// <summary>站点 Logo 导航属性</summary>
    public Media? LogoImage { get; set; }

    /// <summary>Favicon 导航属性</summary>
    public Media? FaviconImage { get; set; }
}