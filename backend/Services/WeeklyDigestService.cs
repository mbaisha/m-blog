using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mblog.API.Data;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

/// <summary>
/// 每周摘要发送结果
/// </summary>
public class WeeklyDigestResult
{
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public int TotalCount { get; set; }
    public bool HasContent { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// 每周邮件摘要服务
/// </summary>
public interface IWeeklyDigestService
{
    /// <summary>执行周报发送（全部订阅者）</summary>
    Task<WeeklyDigestResult> ExecuteDigestAsync(CancellationToken ct = default);

    /// <summary>向指定邮箱补发周报</summary>
    Task<WeeklyDigestResult> ExecuteDigestForSubscriberAsync(string email, CancellationToken ct = default);
}

public class WeeklyDigestService : IWeeklyDigestService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<WeeklyDigestService> _logger;

    public WeeklyDigestService(AppDbContext db, IEmailService emailService, ILogger<WeeklyDigestService> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<WeeklyDigestResult> ExecuteDigestAsync(CancellationToken ct = default)
    {
        var subscribers = await _db.Subscribers
            .Where(s => s.IsActive && s.ConfirmedAt != null)
            .Select(s => new { s.Id, s.Email })
            .ToListAsync(ct);

        if (subscribers.Count == 0)
        {
            _logger.LogInformation("暂无活跃订阅者，跳过周报发送");
            return new WeeklyDigestResult { Message = "暂无活跃订阅者" };
        }

        var (digestHtml, template, siteName, baseUrl, logoUrl, hasContent) = await LoadDigestContentAsync(ct);
        if (!hasContent)
        {
            _logger.LogInformation("本周暂无内容更新，跳过周报发送");
            return new WeeklyDigestResult { Message = "本周暂无内容更新" };
        }

        var successCount = 0;
        var failCount = 0;

        foreach (var sub in subscribers)
        {
            if (ct.IsCancellationRequested) break;
            var ok = await SendToSubscriberAsync(sub.Email, digestHtml, template, siteName, baseUrl, logoUrl, ct);
            if (ok) successCount++; else failCount++;
            if (subscribers.Count > 1) await Task.Delay(200, ct);
        }

        _logger.LogInformation("每周摘要发送完成：成功 {SuccessCount}，失败 {FailCount}，共 {TotalCount} 人",
            successCount, failCount, subscribers.Count);

        return new WeeklyDigestResult
        {
            SuccessCount = successCount,
            FailCount = failCount,
            TotalCount = subscribers.Count,
            HasContent = true,
            Message = $"发送完成：成功 {successCount}，失败 {failCount}，共 {subscribers.Count} 人"
        };
    }

    public async Task<WeeklyDigestResult> ExecuteDigestForSubscriberAsync(string email, CancellationToken ct = default)
    {
        var subscriber = await _db.Subscribers
            .Where(s => s.Email == email && s.IsActive && s.ConfirmedAt != null)
            .Select(s => new { s.Email })
            .FirstOrDefaultAsync(ct);

        if (subscriber == null)
        {
            _logger.LogWarning("订阅者不存在或未确认: {Email}", email);
            return new WeeklyDigestResult { Message = "该订阅者不存在或尚未确认" };
        }

        var (digestHtml, template, siteName, baseUrl, logoUrl, hasContent) = await LoadDigestContentAsync(ct);
        if (!hasContent)
        {
            _logger.LogInformation("本周暂无内容更新，跳过周报发送");
            return new WeeklyDigestResult { Message = "本周暂无内容更新" };
        }

        var ok = await SendToSubscriberAsync(subscriber.Email, digestHtml, template, siteName, baseUrl, logoUrl, ct);

        return new WeeklyDigestResult
        {
            SuccessCount = ok ? 1 : 0,
            FailCount = ok ? 0 : 1,
            TotalCount = 1,
            HasContent = true,
            Message = ok ? $"已向 {subscriber.Email} 发送周报" : $"向 {subscriber.Email} 发送失败"
        };
    }

    /// <summary>加载周报内容（文章数据 + 模板 + 站点信息）</summary>
    private async Task<(string DigestHtml, EmailTemplate? Template, string SiteName, string BaseUrl, string LogoUrl, bool HasContent)>
        LoadDigestContentAsync(CancellationToken ct)
    {
        var siteSetting = await _db.SiteSettings.Include(s => s.LogoImage).FirstOrDefaultAsync(ct);
        var siteName = siteSetting?.SiteName ?? "个人博客";
        var baseUrl = Environment.GetEnvironmentVariable("PUBLIC_SITE_URL") ?? "http://localhost:3000";
        var logoUrl = siteSetting?.LogoImage?.Url ?? "";
        var weekStart = DateTimeOffset.UtcNow.AddDays(-7);

        var newArticles = await _db.Articles.AsNoTracking()
            .Where(a => a.DeletedAt == null && a.Status == "published" && a.PublishedAt >= weekStart)
            .OrderByDescending(a => a.PublishedAt).Take(10)
            .Select(a => new { a.Title, a.Slug, a.Summary, a.ViewCount, a.PublishedAt }).ToListAsync(ct);

        var hotArticles = await _db.Articles.AsNoTracking()
            .Where(a => a.DeletedAt == null && a.Status == "published" && a.PublishedAt >= weekStart)
            .OrderByDescending(a => a.ViewCount).Take(5)
            .Select(a => new { a.Title, a.Slug, a.ViewCount }).ToListAsync(ct);

        var recommendedArticles = await _db.Articles.AsNoTracking()
            .Where(a => a.DeletedAt == null && a.Status == "published" && a.IsRecommend)
            .OrderByDescending(a => a.PublishedAt).Take(5)
            .Select(a => new { a.Title, a.Slug, a.Summary, a.ViewCount }).ToListAsync(ct);

        var hasContent = newArticles.Count > 0 || hotArticles.Count > 0 || recommendedArticles.Count > 0;
        if (!hasContent) return ("", null, siteName, baseUrl, "", false);

        var digestHtml = BuildDigestHtml(siteName, baseUrl, newArticles, hotArticles, recommendedArticles);
        var template = await _db.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.TemplateKey == "subscription_detail", ct);

        return (digestHtml, template, siteName, baseUrl, logoUrl, true);
    }

    /// <summary>向单个订阅者发送周报</summary>
    private async Task<bool> SendToSubscriberAsync(
        string email, string digestHtml, EmailTemplate? template,
        string siteName, string baseUrl, string logoUrl, CancellationToken ct)
    {
        var unsubscribeLink = $"{baseUrl}/subscribe/unsubscribe?email={Uri.EscapeDataString(email)}";

        var now = DateTimeOffset.UtcNow.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        var headerBlock = BuildHeaderBlockHtml(logoUrl, siteName);

        string subject, htmlBody;

        if (template != null)
        {
            subject = template.Subject.Replace("{siteName}", siteName).Replace("{siteUrl}", baseUrl);
            htmlBody = template.HtmlContent
                .Replace("{siteName}", siteName)
                .Replace("{siteUrl}", baseUrl)
                .Replace("{content}", digestHtml)
                .Replace("{unsubscribeLink}", unsubscribeLink)
                .Replace("{currentDate}", now)
                .Replace("{siteHeaderBlock}", headerBlock)
                .Replace("{siteIcon}", string.IsNullOrEmpty(logoUrl) ? "" : headerBlock)
                .Replace("{logoUrl}", logoUrl);
        }
        else
        {
            subject = $"{siteName} - 本周内容精选";
            htmlBody = BuildFallbackHtml(siteName, baseUrl, digestHtml, unsubscribeLink, now, headerBlock);
        }

        var (success, error) = await _emailService.SendEmailAsync(email, subject, htmlBody);
        if (!success)
            _logger.LogWarning("发送周报失败: {Email}, 错误: {Error}", email, error);

        return success;
    }

    #region HTML 构建

    /// <summary>构建邮件头部图标区块（有 LOGO 显示图片，无 LOGO 返回空）</summary>
    internal static string BuildHeaderBlockHtml(string logoUrl, string siteName)
    {
        if (string.IsNullOrEmpty(logoUrl))
            return "";

        return $"""
            <div style="width:48px;height:48px;border-radius:12px;overflow:hidden;display:inline-flex;align-items:center;justify-content:center;margin-bottom:10px;">
                <img src="{logoUrl}" style="width:48px;height:48px;object-fit:cover;" alt="{siteName}" />
            </div>
        """.Trim();
    }

    internal static string BuildDigestHtml(
        string siteName, string baseUrl,
        IReadOnlyList<dynamic> newArticles,
        IReadOnlyList<dynamic> hotArticles,
        IReadOnlyList<dynamic> recommendedArticles)
    {
        var html = new System.Text.StringBuilder();

        if (newArticles.Count > 0)
        {
            html.Append("""
                <div style="margin-bottom:32px;">
                    <div style="display:flex;align-items:center;gap:10px;margin-bottom:16px;">
                        <div style="width:4px;height:20px;background:linear-gradient(180deg,#6366f1,#8b5cf6);border-radius:2px;"></div>
                        <h2 style="font-size:17px;color:#1a1a2e;margin:0;font-weight:600;">🆕 本周新文章</h2>
                    </div>
                """);
            foreach (var article in newArticles)
            {
                var summary = (string?)article.Summary;
                var desc = !string.IsNullOrWhiteSpace(summary) ? (summary.Length > 80 ? summary[..80] + "..." : summary) : "点击阅读全文";
                var date = ((DateTimeOffset)article.PublishedAt).ToLocalTime().ToString("MM-dd");
                html.Append($"""<a href="{baseUrl}/article/{article.Slug}" target="_blank" style="display:block;text-decoration:none;padding:12px 16px;margin-bottom:8px;background:#f8f9ff;border-radius:10px;"><div style="display:flex;justify-content:space-between;align-items:flex-start;"><div style="flex:1;"><span style="font-size:15px;color:#1a1a2e;font-weight:500;line-height:1.5;">{article.Title}</span><p style="font-size:12px;color:#999;margin:4px 0 0;line-height:1.5;">{desc}</p></div><span style="font-size:11px;color:#bbb;white-space:nowrap;margin-left:12px;padding-top:2px;">{date}</span></div></a>""");
            }
            html.Append("</div>");
        }

        if (hotArticles.Count > 0)
        {
            html.Append("""
                <div style="margin-bottom:32px;">
                    <div style="display:flex;align-items:center;gap:10px;margin-bottom:16px;">
                        <div style="width:4px;height:20px;background:linear-gradient(180deg,#f59e0b,#ef4444);border-radius:2px;"></div>
                        <h2 style="font-size:17px;color:#1a1a2e;margin:0;font-weight:600;">🔥 本周热门文章</h2>
                    </div>
                """);
            var rank = 1;
            foreach (var article in hotArticles)
            {
                var medal = rank switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => $"{rank}." };
                html.Append($"""<a href="{baseUrl}/article/{article.Slug}" target="_blank" style="display:flex;align-items:center;text-decoration:none;padding:10px 16px;margin-bottom:6px;background:#fff;border:1px solid #f0f0f0;border-radius:8px;"><span style="font-size:14px;color:#666;width:28px;text-align:center;flex-shrink:0;">{medal}</span><span style="flex:1;font-size:14px;color:#1a1a2e;margin:0 12px;line-height:1.4;">{article.Title}</span><span style="font-size:11px;color:#bbb;white-space:nowrap;">👁 {article.ViewCount}</span></a>""");
                rank++;
            }
            html.Append("</div>");
        }

        if (recommendedArticles.Count > 0)
        {
            html.Append("""
                <div style="margin-bottom:8px;">
                    <div style="display:flex;align-items:center;gap:10px;margin-bottom:16px;">
                        <div style="width:4px;height:20px;background:linear-gradient(180deg,#10b981,#059669);border-radius:2px;"></div>
                        <h2 style="font-size:17px;color:#1a1a2e;margin:0;font-weight:600;">⭐ 精选推荐</h2>
                    </div>
                    <div style="display:flex;flex-wrap:wrap;gap:10px;">
                """);
            foreach (var article in recommendedArticles)
            {
                var summary = (string?)article.Summary;
                var desc = !string.IsNullOrWhiteSpace(summary) ? (summary.Length > 60 ? summary[..60] + "..." : summary) : "点击阅读";
                html.Append($"""<a href="{baseUrl}/article/{article.Slug}" target="_blank" style="flex:1;min-width:200px;display:block;text-decoration:none;padding:14px 16px;background:linear-gradient(135deg,#f0fdf4,#ecfdf5);border-radius:10px;border:1px solid #d1fae5;"><span style="font-size:14px;color:#065f46;font-weight:500;line-height:1.5;display:block;">{article.Title}</span><span style="font-size:12px;color:#6ee7b7;margin-top:4px;display:block;">{desc}</span></a>""");
            }
            html.Append("</div></div>");
        }

        if (html.Length == 0)
            html.Append("""<div style="text-align:center;padding:32px 0;"><p style="font-size:15px;color:#999;">本周暂无内容更新，期待下周与您分享更多精彩！</p></div>""");

        return html.ToString();
    }

    internal static string BuildFallbackHtml(string siteName, string baseUrl, string digestHtml, string unsubscribeLink, string currentDate, string headerBlock)
    {
        return $"""
        <!DOCTYPE html>
        <html>
        <head><meta charset="utf-8"></head>
        <body style="margin:0;padding:0;background-color:#f0f2f5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;">
            <div style="max-width:600px;margin:32px auto;padding:40px 36px;background-color:#ffffff;border-radius:16px;box-shadow:0 4px 12px rgba(0,0,0,0.06);">
                <div style="text-align:center;margin-bottom:28px;">
                    {headerBlock}
                    <h1 style="font-size:22px;color:#1a1a2e;margin:0;font-weight:700;">{siteName}</h1>
                    <p style="font-size:13px;color:#8b8fa3;margin:4px 0 0;">最新内容推送 · {currentDate}</p>
                </div>
                <p style="font-size:15px;color:#333;line-height:1.8;">亲爱的读者，您好！</p>
                <p style="font-size:14px;color:#555;line-height:1.8;">以下是 <strong>{siteName}</strong> 本周的内容精选，希望您喜欢。</p>
                {digestHtml}
                <div style="text-align:center;margin:28px 0;">
                    <a href="{baseUrl}/articles" target="_blank" style="display:inline-block;padding:12px 36px;background:linear-gradient(135deg,#6366f1,#8b5cf6);color:#fff;text-decoration:none;border-radius:10px;font-size:15px;font-weight:600;">📖 浏览更多文章</a>
                </div>
                <hr style="border:none;border-top:1px solid #eee;margin:24px 0;">
                <div style="text-align:center;">
                    <p style="font-size:12px;color:#999;line-height:1.8;">
                        <a href="{unsubscribeLink}" target="_blank" style="color:#999;text-decoration:underline;">退订推送</a>
                    </p>
                </div>
            </div>
        </body>
        </html>
        """;
    }

    #endregion
}