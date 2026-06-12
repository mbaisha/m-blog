using Microsoft.EntityFrameworkCore;
using Mblog.API.Data;
using Mblog.API.Middleware;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

public interface ISiteSettingService
{
    /// <summary>获取站点设置（管理端，含全部字段）</summary>
    Task<SiteSettingResponse?> GetAsync();

    /// <summary>获取站点设置（前台公开，仅基础信息）</summary>
    Task<PublicSiteSettingResponse?> GetPublicAsync();

    /// <summary>保存站点设置（单行 upsert）</summary>
    Task<SiteSettingResponse> SaveAsync(UpdateSiteSettingRequest request);
}

public class SiteSettingService : ISiteSettingService
{
    private readonly AppDbContext _db;

    public SiteSettingService(AppDbContext db) => _db = db;

    public async Task<SiteSettingResponse?> GetAsync()
    {
        var setting = await _db.SiteSettings
            .Include(s => s.LogoImage)
            .Include(s => s.FaviconImage)
            .FirstOrDefaultAsync();

        if (setting == null) return null;

        return MapToResponse(setting);
    }

    public async Task<PublicSiteSettingResponse?> GetPublicAsync()
    {
        var setting = await _db.SiteSettings
            .Include(s => s.LogoImage)
            .Include(s => s.FaviconImage)
            .FirstOrDefaultAsync();

        if (setting == null) return null;

        return new PublicSiteSettingResponse
        {
            SiteName = setting.SiteName,
            SiteDescription = setting.SiteDescription,
            LogoImageUrl = setting.LogoImage?.Url,
            FaviconImageUrl = setting.FaviconImage?.Url,
            LoginCaptchaEnabled = setting.LoginCaptchaEnabled
        };
    }

    public async Task<SiteSettingResponse> SaveAsync(UpdateSiteSettingRequest request)
    {
        var setting = await _db.SiteSettings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new SiteSetting
            {
                SiteName = request.SiteName ?? "My Blog",
                SiteDescription = request.SiteDescription,
                LogoImageId = request.LogoImageId,
                FaviconImageId = request.FaviconImageId,
                CommentModerationEnabled = request.CommentModerationEnabled ?? true,
                ReplyNotificationEnabled = request.ReplyNotificationEnabled ?? false,
                LoginCaptchaEnabled = request.LoginCaptchaEnabled ?? true,
                IpBanEnabled = request.IpBanEnabled ?? true,
                IpBanThreshold = request.IpBanThreshold ?? 200,
                IpBanWindowSeconds = request.IpBanWindowSeconds ?? 10,
                IpBanDurationMinutes = request.IpBanDurationMinutes ?? 5,
                VisitRetentionDays = request.VisitRetentionDays ?? 30
            };
            _db.SiteSettings.Add(setting);
        }
        else
        {
            if (request.SiteName != null) setting.SiteName = request.SiteName;
            if (request.SiteDescription != null) setting.SiteDescription = request.SiteDescription;
            if (request.LogoImageId != null) setting.LogoImageId = request.LogoImageId;
            if (request.FaviconImageId != null) setting.FaviconImageId = request.FaviconImageId;
            if (request.CommentModerationEnabled != null) setting.CommentModerationEnabled = request.CommentModerationEnabled.Value;
            if (request.ReplyNotificationEnabled != null) setting.ReplyNotificationEnabled = request.ReplyNotificationEnabled.Value;
            if (request.LoginCaptchaEnabled != null) setting.LoginCaptchaEnabled = request.LoginCaptchaEnabled.Value;
            if (request.IpBanEnabled != null) setting.IpBanEnabled = request.IpBanEnabled.Value;
            if (request.IpBanThreshold != null) setting.IpBanThreshold = request.IpBanThreshold.Value;
            if (request.IpBanWindowSeconds != null) setting.IpBanWindowSeconds = request.IpBanWindowSeconds.Value;
            if (request.IpBanDurationMinutes != null) setting.IpBanDurationMinutes = request.IpBanDurationMinutes.Value;
            if (request.VisitRetentionDays != null) setting.VisitRetentionDays = request.VisitRetentionDays.Value;
        }

        // 更新 IP 黑名单中间件配置
        IpBanMiddleware.UpdateOptions(setting.IpBanThreshold, setting.IpBanWindowSeconds, setting.IpBanDurationMinutes, setting.IpBanEnabled);

        await _db.SaveChangesAsync();

        // 重新加载导航属性
        await _db.Entry(setting).Reference(s => s.LogoImage).LoadAsync();
        await _db.Entry(setting).Reference(s => s.FaviconImage).LoadAsync();

        return MapToResponse(setting);
    }

    private static SiteSettingResponse MapToResponse(SiteSetting setting)
    {
        return new SiteSettingResponse
        {
            Id = setting.Id,
            SiteName = setting.SiteName,
            SiteDescription = setting.SiteDescription,
            LogoImageId = setting.LogoImageId,
            LogoImageUrl = setting.LogoImage?.Url,
            FaviconImageId = setting.FaviconImageId,
            FaviconImageUrl = setting.FaviconImage?.Url,
            CommentModerationEnabled = setting.CommentModerationEnabled,
            ReplyNotificationEnabled = setting.ReplyNotificationEnabled,
            LoginCaptchaEnabled = setting.LoginCaptchaEnabled,
            IpBanEnabled = setting.IpBanEnabled,
            IpBanThreshold = setting.IpBanThreshold,
            IpBanWindowSeconds = setting.IpBanWindowSeconds,
            IpBanDurationMinutes = setting.IpBanDurationMinutes,
            VisitRetentionDays = setting.VisitRetentionDays
        };
    }
}