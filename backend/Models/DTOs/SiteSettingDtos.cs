namespace Mblog.API.Models.DTOs;

/// <summary>
/// 站点设置响应
/// </summary>
public class SiteSettingResponse
{
    public Guid Id { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string? SiteDescription { get; set; }
    public Guid? LogoImageId { get; set; }
    public string? LogoImageUrl { get; set; }
    public Guid? FaviconImageId { get; set; }
    public string? FaviconImageUrl { get; set; }
    public bool CommentModerationEnabled { get; set; } = true;
    public bool ReplyNotificationEnabled { get; set; }
    public bool LoginCaptchaEnabled { get; set; } = true;
    public bool IpBanEnabled { get; set; } = true;
    public int IpBanThreshold { get; set; } = 200;
    public int IpBanWindowSeconds { get; set; } = 10;
    public int IpBanDurationMinutes { get; set; } = 5;
    public int VisitRetentionDays { get; set; } = 30;
}

/// <summary>
/// 更新站点设置请求
/// </summary>
public class UpdateSiteSettingRequest
{
    public string? SiteName { get; set; }
    public string? SiteDescription { get; set; }
    public Guid? LogoImageId { get; set; }
    public Guid? FaviconImageId { get; set; }
    public bool? CommentModerationEnabled { get; set; }
    public bool? ReplyNotificationEnabled { get; set; }
    public bool? LoginCaptchaEnabled { get; set; }
    public bool? IpBanEnabled { get; set; }
    public int? IpBanThreshold { get; set; }
    public int? IpBanWindowSeconds { get; set; }
    public int? IpBanDurationMinutes { get; set; }
    public int? VisitRetentionDays { get; set; }
}

/// <summary>
/// 前台公开站点设置
/// </summary>
public class PublicSiteSettingResponse
{
    public string SiteName { get; set; } = string.Empty;
    public string? SiteDescription { get; set; }
    public string? LogoImageUrl { get; set; }
    public string? FaviconImageUrl { get; set; }
    /// <summary>是否启用登录验证码（首次失败后出现滑块）</summary>
    public bool LoginCaptchaEnabled { get; set; } = true;
}