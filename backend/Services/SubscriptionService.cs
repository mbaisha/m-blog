using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mblog.API.Data;
using Mblog.API.Models.DTOs.Subscription;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

public interface ISubscriptionService
{
    /// <summary>提交订阅</summary>
    Task<SubscribeResponse> SubscribeAsync(SubscribeRequest request);

    /// <summary>确认订阅（通过令牌）</summary>
    Task<(bool Success, string Message)> ConfirmSubscriptionAsync(string token);

    /// <summary>获取订阅者列表（后台）</summary>
    Task<List<SubscriberListItem>> GetSubscribersAsync();

    /// <summary>获取指定订阅者</summary>
    Task<SubscriberListItem?> GetByIdAsync(Guid id);

    /// <summary>删除订阅记录（后台）</summary>
    Task<bool> DeleteSubscriberAsync(Guid id);

    /// <summary>获取订阅总数</summary>
    Task<int> GetActiveCountAsync();

    /// <summary>发送确认订阅邮件</summary>
    Task<(bool Success, string? Error)> SendConfirmationEmailAsync(string email);

    /// <summary>发送订阅内容邮件</summary>
    Task<(bool Success, string? Error)> SendSubscriptionDetailAsync(string email, string content, string unsubscribeLink);

    /// <summary>请求退订验证码</summary>
    Task<UnsubscribeCaptchaResponse?> RequestUnsubscribeCaptchaAsync(string email);

    /// <summary>确认退订（验证码）</summary>
    Task<(bool Success, string Message)> ConfirmUnsubscribeAsync(string sessionId, string code);

    /// <summary>取消订阅（直接使用完整邮箱，管理端使用）</summary>
    Task<bool> UnsubscribeByEmailAsync(string email);
}

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(AppDbContext db, IEmailService emailService, IMemoryCache cache, ILogger<SubscriptionService> logger)
    {
        _db = db;
        _emailService = emailService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<SubscribeResponse> SubscribeAsync(SubscribeRequest request)
    {
        var email = request.Email.Trim().ToLower();

        // 检查是否已订阅
        var existing = await _db.Subscribers
            .FirstOrDefaultAsync(s => s.Email == email);

        if (existing != null)
        {
            if (existing.IsActive && existing.ConfirmedAt != null)
                throw new InvalidOperationException("该邮箱已订阅并确认");

            if (existing.IsActive && existing.ConfirmedAt == null)
            {
                // 已提交但未确认，重新发送确认邮件
                await SendConfirmationEmailAsync(email);
                return new SubscribeResponse
                {
                    Id = existing.Id,
                    Email = existing.Email,
                    SubscribedAt = existing.SubscribedAt,
                    NeedsConfirmation = true,
                    Message = "确认邮件已重新发送，请查收并确认订阅"
                };
            }

            // 重新激活
            existing.IsActive = true;
            existing.UnsubscribedAt = null;
            existing.ConfirmedAt = null;
            existing.SubscribedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            // 发送确认邮件
            await SendConfirmationEmailAsync(email);

            return new SubscribeResponse
            {
                Id = existing.Id,
                Email = existing.Email,
                SubscribedAt = existing.SubscribedAt,
                NeedsConfirmation = true,
                Message = "请查收确认邮件并点击链接完成订阅"
            };
        }

        var subscriber = new Subscriber
        {
            Email = email,
            SubscribedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };

        _db.Subscribers.Add(subscriber);
        await _db.SaveChangesAsync();

        // 发送确认邮件
        await SendConfirmationEmailAsync(email);

        return new SubscribeResponse
        {
            Id = subscriber.Id,
            Email = subscriber.Email,
            SubscribedAt = subscriber.SubscribedAt,
            NeedsConfirmation = true,
            Message = "请查收确认邮件并点击链接完成订阅"
        };
    }

    public async Task<(bool Success, string Message)> ConfirmSubscriptionAsync(string token)
    {
        var subscriber = await _db.Subscribers
            .FirstOrDefaultAsync(s => s.ConfirmationToken == token && s.IsActive);

        if (subscriber == null)
            return (false, "无效的确认链接");

        if (subscriber.ConfirmedAt != null)
            return (false, "该邮箱已经确认过订阅");

        if (subscriber.TokenExpiresAt == null || subscriber.TokenExpiresAt < DateTimeOffset.UtcNow)
            return (false, "确认链接已过期，请重新订阅");

        subscriber.ConfirmedAt = DateTimeOffset.UtcNow;
        subscriber.ConfirmationToken = null;
        subscriber.TokenExpiresAt = null;
        await _db.SaveChangesAsync();

        return (true, "订阅确认成功！感谢您的支持");
    }

    public async Task<List<SubscriberListItem>> GetSubscribersAsync()
    {
        return await _db.Subscribers
            .OrderByDescending(s => s.SubscribedAt)
            .Select(s => new SubscriberListItem
            {
                Id = s.Id,
                Email = s.Email,
                SubscribedAt = s.SubscribedAt,
                IsActive = s.IsActive,
                UnsubscribedAt = s.UnsubscribedAt,
                ConfirmedAt = s.ConfirmedAt
            })
            .ToListAsync();
    }

    public async Task<SubscriberListItem?> GetByIdAsync(Guid id)
    {
        return await _db.Subscribers
            .Where(s => s.Id == id)
            .Select(s => new SubscriberListItem
            {
                Id = s.Id,
                Email = s.Email,
                SubscribedAt = s.SubscribedAt,
                IsActive = s.IsActive,
                UnsubscribedAt = s.UnsubscribedAt,
                ConfirmedAt = s.ConfirmedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteSubscriberAsync(Guid id)
    {
        var subscriber = await _db.Subscribers.FindAsync(id);
        if (subscriber == null) return false;

        _db.Subscribers.Remove(subscriber);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _db.Subscribers.CountAsync(s => s.IsActive && s.ConfirmedAt != null);
    }

    public async Task<(bool Success, string? Error)> SendConfirmationEmailAsync(string email)
    {
        try
        {
            var subscriber = await _db.Subscribers
                .FirstOrDefaultAsync(s => s.Email == email && s.IsActive);

            if (subscriber == null)
                return (false, "未找到该订阅者");

            // 生成确认令牌
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            subscriber.ConfirmationToken = token;
            subscriber.TokenExpiresAt = DateTimeOffset.UtcNow.AddDays(3);
            await _db.SaveChangesAsync();

            // 获取站点设置
            var siteSetting = await _db.SiteSettings.FirstOrDefaultAsync();
            var siteName = siteSetting?.SiteName ?? "个人博客";
            var baseUrl = Environment.GetEnvironmentVariable("PUBLIC_SITE_URL") ?? "http://localhost:3000";

            var confirmationLink = $"{baseUrl}/subscribe/confirm?token={token}";

            return await _emailService.SendConfirmationEmailAsync(email, confirmationLink, siteName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送确认订阅邮件失败: {Email}", email);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> SendSubscriptionDetailAsync(string email, string content, string unsubscribeLink)
    {
        try
        {
            // 获取订阅详情模板
            var template = await _db.Set<EmailTemplate>()
                .FirstOrDefaultAsync(t => t.TemplateKey == "subscription_detail");

            var siteSetting = await _db.SiteSettings.Include(s => s.LogoImage).FirstOrDefaultAsync();
            var siteName = siteSetting?.SiteName ?? "个人博客";
            var baseUrl = Environment.GetEnvironmentVariable("PUBLIC_SITE_URL") ?? "http://localhost:3000";
            var now = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm");
            var logoUrl = siteSetting?.LogoImage?.Url ?? "";
            var headerBlock = WeeklyDigestService.BuildHeaderBlockHtml(logoUrl, siteName);

            string subject;
            string htmlBody;

            if (template != null)
            {
                subject = template.Subject.Replace("{siteName}", siteName);
                htmlBody = template.HtmlContent
                    .Replace("{siteName}", siteName)
                    .Replace("{siteUrl}", baseUrl)
                    .Replace("{content}", content)
                    .Replace("{unsubscribeLink}", unsubscribeLink)
                    .Replace("{currentDate}", now)
                    .Replace("{siteHeaderBlock}", headerBlock)
                    .Replace("{siteIcon}", headerBlock)
                    .Replace("{logoUrl}", logoUrl);
            }
            else
            {
                subject = $"{siteName} - 最新内容";
                htmlBody = $"""
                <!DOCTYPE html>
                <html>
                <head><meta charset="utf-8"></head>
                <body style="margin:0;padding:0;background-color:#f5f5f5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;">
                    <div style="max-width:600px;margin:40px auto;padding:40px 30px;background-color:#ffffff;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,0.08);">
                        <div style="text-align:center;margin-bottom:30px;">
                            <h1 style="font-size:24px;color:#1a1a2e;margin:0;">{siteName}</h1>
                            <p style="font-size:14px;color:#666;margin:8px 0 0;">最新内容推送</p>
                        </div>
                        {content}
                        <hr style="border:none;border-top:1px solid #eee;margin:30px 0;">
                        <div style="text-align:center;">
                            <p style="font-size:12px;color:#999;line-height:1.6;">
                                如果您不想再收到此类邮件，<br>
                                <a href="{unsubscribeLink}" target="_blank" style="color:#999;text-decoration:underline;">点击这里退订</a>
                            </p>
                            <p style="font-size:12px;color:#999;margin-top:8px;">{siteName} · 用心分享每一篇内容</p>
                        </div>
                    </div>
                </body>
                </html>
                """;
            }

            return await _emailService.SendEmailAsync(email, subject, htmlBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送订阅详情邮件失败: {Email}", email);
            return (false, ex.Message);
        }
    }

    public async Task<UnsubscribeCaptchaResponse?> RequestUnsubscribeCaptchaAsync(string email)
    {
        // 查找匹配邮箱的已确认活跃订阅者
        var subscriber = await _db.Subscribers
            .Where(s => s.Email == email && s.IsActive && s.ConfirmedAt != null)
            .FirstOrDefaultAsync();

        if (subscriber == null) return null;

        // 生成 masked email: example@***.com
        var atIndex = email.IndexOf('@');
        var maskedEmail = atIndex > 2
            ? email[..2] + "***" + email[atIndex..]
            : email[..1] + "***" + email[atIndex..];

        // 生成验证码
        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var sessionId = Guid.NewGuid().ToString();

        // 缓存会话（5分钟有效）
        _cache.Set(sessionId, new UnsubscribeSession
        {
            Email = email,
            Code = code,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
        }, TimeSpan.FromMinutes(5));

        _logger.LogInformation("退订验证码已生成: {Email} -> {Code}", email, code);

        // 实际生产环境中应该发送邮件
        try
        {
            // 尝试发送验证码到用户邮箱
            var siteSetting = await _db.SiteSettings.FirstOrDefaultAsync();
            var siteName = siteSetting?.SiteName ?? "个人博客";
            var subject = $"退订验证码 - {siteName}";
            var htmlBody = $"""
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8"></head>
            <body style="margin:0;padding:0;background-color:#f5f5f5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;">
                <div style="max-width:600px;margin:40px auto;padding:40px 30px;background-color:#ffffff;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,0.08);">
                    <h2 style="color:#1a1a2e;margin:0 0 20px;">退订验证</h2>
                    <p style="font-size:15px;color:#333;line-height:1.8;">您好，</p>
                    <p style="font-size:15px;color:#333;line-height:1.8;">您的退订验证码为：</p>
                    <div style="text-align:center;margin:30px 0;">
                        <span style="display:inline-block;padding:16px 40px;background-color:#f0f0f5;border-radius:8px;font-size:32px;font-weight:700;letter-spacing:8px;color:#1a1a2e;">{code}</span>
                    </div>
                    <p style="font-size:13px;color:#999;line-height:1.6;text-align:center;">验证码 5 分钟内有效，请勿泄露给他人。</p>
                    <hr style="border:none;border-top:1px solid #eee;margin:30px 0;">
                    <p style="font-size:12px;color:#999;text-align:center;">{siteName}</p>
                </div>
            </body>
            </html>
            """;
            await _emailService.SendEmailAsync(email, subject, htmlBody);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "发送退订验证码邮件失败，但仍返回会话信息");
        }

        return new UnsubscribeCaptchaResponse
        {
            SessionId = sessionId,
            MaskedEmail = maskedEmail
        };
    }

    public async Task<(bool Success, string Message)> ConfirmUnsubscribeAsync(string sessionId, string code)
    {
        // 从缓存获取会话
        if (!_cache.TryGetValue(sessionId, out UnsubscribeSession? session) || session == null)
            return (false, "验证码会话已过期，请重新获取");

        if (session.ExpiresAt < DateTimeOffset.UtcNow)
        {
            _cache.Remove(sessionId);
            return (false, "验证码已过期，请重新获取");
        }

        if (session.Code != code)
            return (false, "验证码不正确");

        // 执行退订
        var subscriber = await _db.Subscribers
            .FirstOrDefaultAsync(s => s.Email == session.Email && s.IsActive);

        if (subscriber == null)
            return (false, "未找到该订阅者");

        subscriber.IsActive = false;
        subscriber.UnsubscribedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        // 清除缓存
        _cache.Remove(sessionId);

        // 发送退订确认邮件
        try
        {
            var siteSetting = await _db.SiteSettings.Include(s => s.LogoImage).FirstOrDefaultAsync();
            var siteName = siteSetting?.SiteName ?? "个人博客";
            var logoUrl = siteSetting?.LogoImage?.Url ?? "";
            var headerBlock = WeeklyDigestService.BuildHeaderBlockHtml(logoUrl, siteName);

            // 获取退订模板
            var template = await _db.Set<EmailTemplate>()
                .FirstOrDefaultAsync(t => t.TemplateKey == "unsubscribed");

            var baseUrl = Environment.GetEnvironmentVariable("PUBLIC_SITE_URL") ?? "http://localhost:3000";
            var subscribeLink = $"{baseUrl}/";

            string subject;
            string htmlBody;

            if (template != null)
            {
                subject = template.Subject.Replace("{siteName}", siteName);
                htmlBody = template.HtmlContent
                    .Replace("{siteName}", siteName)
                    .Replace("{siteUrl}", baseUrl)
                    .Replace("{subscribeLink}", subscribeLink)
                    .Replace("{siteHeaderBlock}", headerBlock)
                    .Replace("{siteIcon}", headerBlock)
                    .Replace("{logoUrl}", logoUrl);
            }
            else
            {
                subject = $"已退订 - {siteName}";
                htmlBody = $"""
                <!DOCTYPE html>
                <html>
                <head><meta charset="utf-8"></head>
                <body style="margin:0;padding:0;background-color:#f5f5f5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;">
                    <div style="max-width:600px;margin:40px auto;padding:40px 30px;background-color:#ffffff;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,0.08);">
                        <div style="text-align:center;margin-bottom:30px;">
                            <h1 style="font-size:24px;color:#1a1a2e;margin:0;">{siteName}</h1>
                            <p style="font-size:14px;color:#666;margin:8px 0 0;">您已成功退订</p>
                        </div>
                        <p style="font-size:15px;color:#333;line-height:1.8;">您好，</p>
                        <p style="font-size:15px;color:#333;line-height:1.8;">您已成功取消订阅 <strong>{siteName}</strong> 的邮件推送服务。</p>
                        <p style="font-size:15px;color:#333;line-height:1.8;">如果您改变主意，随时欢迎您再次订阅我们的内容。</p>
                        <div style="text-align:center;margin:35px 0;">
                            <a href="{subscribeLink}" target="_blank" style="display:inline-block;padding:12px 36px;background-color:#6366f1;color:#ffffff;text-decoration:none;border-radius:8px;font-size:15px;font-weight:500;">重新订阅</a>
                        </div>
                        <hr style="border:none;border-top:1px solid #eee;margin:30px 0;">
                        <p style="font-size:12px;color:#999;text-align:center;">{siteName} · 用心分享每一篇内容</p>
                    </div>
                </body>
                </html>
                """;
            }

            await _emailService.SendEmailAsync(session.Email, subject, htmlBody);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "发送退订确认邮件失败");
        }

        return (true, "已成功退订，退订确认邮件已发送至您的邮箱");
    }

    public async Task<bool> UnsubscribeByEmailAsync(string email)
    {
        var subscriber = await _db.Subscribers
            .FirstOrDefaultAsync(s => s.Email == email && s.IsActive);

        if (subscriber == null) return false;

        subscriber.IsActive = false;
        subscriber.UnsubscribedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}