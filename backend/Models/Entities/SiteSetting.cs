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

    /// <summary>是否启用评论回复邮件通知</summary>
    public bool ReplyNotificationEnabled { get; set; }

    /// <summary>是否启用登录验证码（首次失败后出现滑块）</summary>
    public bool LoginCaptchaEnabled { get; set; } = true;

    /// <summary>是否启用 IP 黑名单</summary>
    public bool IpBanEnabled { get; set; } = true;

    /// <summary>IP 黑名单：统计窗口内请求次数阈值（默认 200）</summary>
    public int IpBanThreshold { get; set; } = 200;

    /// <summary>IP 黑名单：统计窗口时长（秒，默认 10）</summary>
    public int IpBanWindowSeconds { get; set; } = 10;

    /// <summary>IP 黑名单：拉黑时长（分钟，默认 5）</summary>
    public int IpBanDurationMinutes { get; set; } = 5;

    /// <summary>访问记录保留天数（默认 30 天）</summary>
    public int VisitRetentionDays { get; set; } = 30;

    /// <summary>站点 Logo 导航属性</summary>
    public Media? LogoImage { get; set; }

    /// <summary>Favicon 导航属性</summary>
    public Media? FaviconImage { get; set; }
}