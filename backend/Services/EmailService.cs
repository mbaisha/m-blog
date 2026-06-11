using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mblog.API.Data;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

public interface IEmailService
{
    /// <summary>获取邮件设置</summary>
    Task<EmailSettingResponse?> GetSettingAsync();

    /// <summary>保存邮件设置</summary>
    Task<EmailSettingResponse> SaveSettingAsync(UpdateEmailSettingRequest request);

    /// <summary>测试邮件发送</summary>
    Task<bool> TestSendAsync(string testEmail);

    /// <summary>发送邮件（使用当前设置）</summary>
    Task<(bool Success, string? Error)> SendEmailAsync(string toEmail, string subject, string htmlBody);

    /// <summary>发送确认订阅邮件</summary>
    Task<(bool Success, string? Error)> SendConfirmationEmailAsync(string toEmail, string confirmationLink, string siteName);
}

public class EmailService : IEmailService
{
    private readonly AppDbContext _db;
    private readonly ILogger<EmailService> _logger;

    public EmailService(AppDbContext db, ILogger<EmailService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<EmailSettingResponse?> GetSettingAsync()
    {
        var setting = await _db.Set<EmailSetting>().FirstOrDefaultAsync();
        if (setting == null) return null;

        return new EmailSettingResponse
        {
            Id = setting.Id,
            SmtpServer = setting.SmtpServer,
            SmtpPort = setting.SmtpPort,
            SmtpUsername = setting.SmtpUsername,
            SmtpPasswordMasked = "********",
            SenderEmail = setting.SenderEmail,
            SenderName = setting.SenderName,
            UseSsl = setting.UseSsl
        };
    }

    public async Task<EmailSettingResponse> SaveSettingAsync(UpdateEmailSettingRequest request)
    {
        var setting = await _db.Set<EmailSetting>().FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new EmailSetting
            {
                SmtpServer = request.SmtpServer,
                SmtpPort = request.SmtpPort,
                SmtpUsername = request.SmtpUsername,
                SmtpPassword = request.SmtpPassword ?? string.Empty,
                SenderEmail = request.SenderEmail,
                SenderName = request.SenderName,
                UseSsl = request.UseSsl
            };
            _db.Set<EmailSetting>().Add(setting);
        }
        else
        {
            setting.SmtpServer = request.SmtpServer;
            setting.SmtpPort = request.SmtpPort;
            setting.SmtpUsername = request.SmtpUsername;
            if (!string.IsNullOrEmpty(request.SmtpPassword))
                setting.SmtpPassword = request.SmtpPassword;
            setting.SenderEmail = request.SenderEmail;
            setting.SenderName = request.SenderName;
            setting.UseSsl = request.UseSsl;
        }

        await _db.SaveChangesAsync();

        return new EmailSettingResponse
        {
            Id = setting.Id,
            SmtpServer = setting.SmtpServer,
            SmtpPort = setting.SmtpPort,
            SmtpUsername = setting.SmtpUsername,
            SmtpPasswordMasked = "********",
            SenderEmail = setting.SenderEmail,
            SenderName = setting.SenderName,
            UseSsl = setting.UseSsl
        };
    }

    public async Task<bool> TestSendAsync(string testEmail)
    {
        try
        {
            var (success, error) = await SendEmailAsync(testEmail, "测试邮件 - 您的邮件配置正确", "<h2>邮件配置测试</h2><p>如果收到此邮件，说明您的 SMTP 配置正确，邮件发送功能正常。</p>");
            return success;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool Success, string? Error)> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var setting = await _db.Set<EmailSetting>().FirstOrDefaultAsync();
            if (setting == null)
                return (false, "邮件发送设置未配置");

            using var client = new SmtpClient(setting.SmtpServer, setting.SmtpPort)
            {
                Credentials = new NetworkCredential(setting.SmtpUsername, setting.SmtpPassword),
                EnableSsl = setting.UseSsl,
                Timeout = 15000
            };

            using var message = new MailMessage
            {
                From = new MailAddress(setting.SenderEmail, setting.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);

            // 记录发送日志
            _db.Set<EmailLog>().Add(new EmailLog
            {
                Email = toEmail,
                Subject = subject,
                TemplateKey = null,
                SentAt = DateTimeOffset.UtcNow,
                IsSuccess = true
            });
            await _db.SaveChangesAsync();

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送邮件失败: {Email}", toEmail);

            // 记录失败日志
            _db.Set<EmailLog>().Add(new EmailLog
            {
                Email = toEmail,
                Subject = subject,
                TemplateKey = null,
                SentAt = DateTimeOffset.UtcNow,
                IsSuccess = false,
                ErrorMessage = ex.Message
            });
            await _db.SaveChangesAsync();

            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> SendConfirmationEmailAsync(string toEmail, string confirmationLink, string siteName)
    {
        // 获取确认订阅模板
        var template = await _db.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.TemplateKey == "confirm_subscription");

        var siteSetting = await _db.SiteSettings.Include(s => s.LogoImage).AsNoTracking().FirstOrDefaultAsync();
        var baseUrl = Environment.GetEnvironmentVariable("PUBLIC_SITE_URL") ?? "http://localhost:3000";
        var now = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm");
        var logoUrl = siteSetting?.LogoImage?.Url ?? "";
        var headerBlock = WeeklyDigestService.BuildHeaderBlockHtml(logoUrl, siteName);

        string htmlBody;
        string subject;

        if (template != null)
        {
            subject = template.Subject.Replace("{siteName}", siteName);
            htmlBody = template.HtmlContent
                .Replace("{siteName}", siteName)
                .Replace("{siteUrl}", baseUrl)
                .Replace("{confirmationLink}", confirmationLink)
                .Replace("{currentDate}", now)
                .Replace("{siteHeaderBlock}", headerBlock)
                .Replace("{siteIcon}", headerBlock)
                .Replace("{logoUrl}", logoUrl);
        }
        else
        {
            // 默认模板
            subject = $"确认订阅 - {siteName}";
            htmlBody = GetDefaultConfirmationTemplate(siteName, confirmationLink);
        }

        var (success, error) = await SendEmailAsync(toEmail, subject, htmlBody);

        if (success)
        {
            // 更新日志记录模板 key
            var log = await _db.Set<EmailLog>()
                .OrderByDescending(l => l.SentAt)
                .FirstOrDefaultAsync(l => l.Email == toEmail && l.IsSuccess);
            if (log != null)
            {
                log.TemplateKey = "confirm_subscription";
                await _db.SaveChangesAsync();
            }
        }

        return (success, error);
    }

    private static string GetDefaultConfirmationTemplate(string siteName, string confirmationLink)
    {
        return $"""
        <!DOCTYPE html>
        <html>
        <head><meta charset="utf-8"></head>
        <body style="margin:0;padding:0;background-color:#f5f5f5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;">
            <div style="max-width:600px;margin:40px auto;padding:40px 30px;background-color:#ffffff;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,0.08);">
                <div style="text-align:center;margin-bottom:30px;">
                    <h1 style="font-size:24px;color:#1a1a2e;margin:0;">{siteName}</h1>
                    <p style="font-size:14px;color:#666;margin:8px 0 0;">感谢您的订阅</p>
                </div>
                <p style="font-size:15px;color:#333;line-height:1.8;">您好，</p>
                <p style="font-size:15px;color:#333;line-height:1.8;">
                    感谢您订阅 <strong>{siteName}</strong>！我们致力于分享优质的技术文章、产品思考和项目经验。
                </p>
                <p style="font-size:15px;color:#333;line-height:1.8;">
                    请点击下方按钮确认您的订阅：
                </p>
                <div style="text-align:center;margin:35px 0;">
                    <a href="{confirmationLink}" target="_blank" style="display:inline-block;padding:14px 40px;background-color:#6366f1;color:#ffffff;text-decoration:none;border-radius:8px;font-size:16px;font-weight:500;">确认订阅</a>
                </div>
                <p style="font-size:13px;color:#999;line-height:1.6;text-align:center;">
                    此链接 <strong>3天内有效</strong>，逾期请重新订阅。<br>
                    如果您没有订阅此邮件，请忽略本邮件。
                </p>
                <hr style="border:none;border-top:1px solid #eee;margin:30px 0;">
                <p style="font-size:12px;color:#999;text-align:center;">{siteName} · 用心分享每一篇内容</p>
            </div>
        </body>
        </html>
        """;
    }
}

/// <summary>
/// 邮件模板管理服务
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>获取所有模板</summary>
    Task<List<Models.DTOs.EmailTemplateResponse>> GetAllAsync();

    /// <summary>获取单个模板</summary>
    Task<Models.DTOs.EmailTemplateResponse?> GetByKeyAsync(string templateKey);

    /// <summary>更新模板</summary>
    Task<Models.DTOs.EmailTemplateResponse> UpdateAsync(string templateKey, Models.DTOs.UpdateEmailTemplateRequest request);

    /// <summary>重置模板到默认值</summary>
    Task<Models.DTOs.EmailTemplateResponse> ResetAsync(string templateKey);
}

public class EmailTemplateService : IEmailTemplateService
{
    private readonly AppDbContext _db;

    public EmailTemplateService(AppDbContext db) => _db = db;

    public async Task<List<Models.DTOs.EmailTemplateResponse>> GetAllAsync()
    {
        return await _db.Set<EmailTemplate>()
            .OrderBy(t => t.TemplateKey)
            .Select(t => new Models.DTOs.EmailTemplateResponse
            {
                Id = t.Id,
                TemplateKey = t.TemplateKey,
                Subject = t.Subject,
                HtmlContent = t.HtmlContent,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<Models.DTOs.EmailTemplateResponse?> GetByKeyAsync(string templateKey)
    {
        var template = await _db.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.TemplateKey == templateKey);

        if (template == null)
        {
            var defaults = GetDefaultTemplate(templateKey);
            if (defaults == null) return null;

            return new Models.DTOs.EmailTemplateResponse
            {
                TemplateKey = templateKey,
                Subject = defaults.Value.Subject,
                HtmlContent = defaults.Value.HtmlContent,
                Description = defaults.Value.Description
            };
        }

        return new Models.DTOs.EmailTemplateResponse
        {
            Id = template.Id,
            TemplateKey = template.TemplateKey,
            Subject = template.Subject,
            HtmlContent = template.HtmlContent,
            Description = template.Description,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }

    public async Task<Models.DTOs.EmailTemplateResponse> UpdateAsync(string templateKey, Models.DTOs.UpdateEmailTemplateRequest request)
    {
        var template = await _db.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.TemplateKey == templateKey);

        if (template == null)
        {
            template = new EmailTemplate
            {
                TemplateKey = templateKey,
                Subject = request.Subject,
                HtmlContent = request.HtmlContent,
                Description = request.Description
            };
            _db.Set<EmailTemplate>().Add(template);
        }
        else
        {
            template.Subject = request.Subject;
            template.HtmlContent = request.HtmlContent;
            if (request.Description != null)
                template.Description = request.Description;
        }

        await _db.SaveChangesAsync();

        return new Models.DTOs.EmailTemplateResponse
        {
            Id = template.Id,
            TemplateKey = template.TemplateKey,
            Subject = template.Subject,
            HtmlContent = template.HtmlContent,
            Description = template.Description,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }

    public async Task<Models.DTOs.EmailTemplateResponse> ResetAsync(string templateKey)
    {
        var defaults = GetDefaultTemplate(templateKey);
        if (defaults == null)
            throw new ArgumentException($"未知的模板标识: {templateKey}");

        var (subject, htmlContent, description) = defaults.Value;
        return await UpdateAsync(templateKey, new Models.DTOs.UpdateEmailTemplateRequest
        {
            Subject = subject,
            HtmlContent = htmlContent,
            Description = description
        });
    }

    private static (string Subject, string HtmlContent, string Description)? GetDefaultTemplate(string key)
    {
        return key switch
        {
            "confirm_subscription" => ("确认订阅 - {siteName}", GetDefaultConfirmHtml(), "确认订阅邮件：发送给新订阅者，包含确认链接（3天有效）"),
            "subscription_detail" => ("{siteName} - 本周内容精选", GetDefaultDetailHtml(), "订阅内容邮件：发送给已确认订阅的用户，包含周报（本周新文章/热门/推荐）+ 退订链接"),
            "unsubscribed" => ("已退订 - {siteName}", GetDefaultUnsubscribedHtml(), "退订确认邮件：用户成功退订后发送的通知"),
            _ => null
        };
    }

    private static string GetDefaultConfirmHtml()
    {
        return @"<!DOCTYPE html>
<html>
<head><meta charset=""utf-8"">
<style>
  @media only screen and (max-width: 480px) {
    .container { padding: 24px 16px !important; }
    .btn { padding: 14px 24px !important; font-size: 15px !important; width: 100% !important; box-sizing: border-box !important; }
    .logo-text { font-size: 22px !important; }
    .footer-links a { display: inline-block !important; margin: 4px 8px !important; }
    .quick-links a { display: inline-block !important; margin: 3px 8px !important; }
    .features-grid { display: block !important; }
    .feature-item { margin-bottom: 12px !important; }
  }
</style>
</head>
<body style=""margin:0;padding:0;background-color:#f0f2f5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI','PingFang SC','Hiragino Sans GB','Microsoft YaHei',sans-serif;"">
    <div class=""container"" style=""max-width:600px;margin:32px auto;padding:40px 36px;background-color:#ffffff;border-radius:16px;box-shadow:0 4px 12px rgba(0,0,0,0.06);"">
        
        <!-- 头部 Logo 区 -->
        <div style=""text-align:center;margin-bottom:32px;"">
            {siteHeaderBlock}
            <h1 class=""logo-text"" style=""font-size:26px;color:#1a1a2e;margin:0;font-weight:700;"">{siteName}</h1>
            <p style=""font-size:14px;color:#8b8fa3;margin:6px 0 0;"">发现 · 分享 · 成长</p>
        </div>

        <!-- 主体内容 -->
        <div style=""padding:0 4px;"">
            <p style=""font-size:15px;color:#333;line-height:1.8;margin:0 0 6px;"">尊敬的订阅者，您好！</p>
            
            <p style=""font-size:15px;color:#333;line-height:1.8;"">
                感谢您订阅 <strong style=""color:#6366f1;"">{siteName}</strong>！我们专注于分享高品质的技术文章、产品设计心得与开发实战经验，希望能为您带来价值。
            </p>

            <!-- 站点介绍卡片 -->
            <div style=""background:#f8f9ff;border-radius:12px;padding:20px 24px;margin:22px 0;border-left:4px solid #6366f1;"">
                <p style=""font-size:14px;color:#444;margin:0 0 10px;font-weight:600;"">✨ 在这里，您可以找到：</p>
                <table style=""font-size:13px;color:#555;line-height:1.8;border-collapse:collapse;"">
                    <tr><td style=""padding:2px 0;"">📝 原创技术文章与教程</td></tr>
                    <tr><td style=""padding:2px 0;"">💡 产品设计与开发经验分享</td></tr>
                    <tr><td style=""padding:2px 0;"">🚀 开源项目与工具推荐</td></tr>
                    <tr><td style=""padding:2px 0;"">🎯 行业前沿资讯与深度思考</td></tr>
                </table>
            </div>

            <p style=""font-size:15px;color:#333;line-height:1.8;"">
                请点击下方按钮确认您的订阅，开启精彩内容之旅：
            </p>

            <!-- 确认按钮 -->
            <div style=""text-align:center;margin:32px 0;"">
                <a class=""btn"" href=""{confirmationLink}"" target=""_blank"" style=""display:inline-block;padding:15px 48px;background:linear-gradient(135deg,#6366f1,#8b5cf6);color:#ffffff;text-decoration:none;border-radius:10px;font-size:16px;font-weight:600;box-shadow:0 4px 14px rgba(99,102,241,0.35);"">✅ 确认订阅</a>
            </div>

            <!-- 有效期提示 -->
            <div style=""background:#fff8e6;border-radius:8px;padding:12px 16px;margin:16px 0 24px;text-align:center;"">
                <p style=""font-size:13px;color:#b8860b;margin:0;line-height:1.6;"">
                    ⏰ 此确认链接 <strong>3天内有效</strong>，请及时点击确认。<br>
                    如果您没有申请订阅，请忽略此邮件，您不会收到后续推送。
                </p>
            </div>

            <!-- 快速访问链接 -->
            <div style=""text-align:center;margin:24px 0 20px;padding:16px 0;border-top:1px solid #eee;border-bottom:1px solid #eee;"">
                <p style=""font-size:14px;color:#666;margin:0 0 10px;font-weight:500;"">快速访问我们的网站</p>
                <div class=""quick-links"" style=""font-size:13px;"">
                    <a href=""{siteUrl}"" target=""_blank"" style=""color:#6366f1;text-decoration:none;margin:0 12px;font-weight:500;"">🏠 网站首页</a>
                    <a href=""{siteUrl}/articles"" target=""_blank"" style=""color:#6366f1;text-decoration:none;margin:0 12px;font-weight:500;"">📖 文章列表</a>
                    <a href=""{siteUrl}/about"" target=""_blank"" style=""color:#6366f1;text-decoration:none;margin:0 12px;font-weight:500;"">👤 关于我们</a>
                </div>
            </div>
        </div>

        <!-- 页脚 -->
        <div style=""text-align:center;padding-top:24px;"">
            <hr style=""border:none;border-top:1px solid #eee;margin:0 0 20px;"">
            <div class=""footer-links"" style=""font-size:12px;color:#999;line-height:1.8;"">
                <a href=""{siteUrl}"" target=""_blank"" style=""color:#999;text-decoration:none;"">网站首页</a>
                <span style=""color:#ddd;margin:0 6px;"">|</span>
                <a href=""{siteUrl}/articles"" target=""_blank"" style=""color:#999;text-decoration:none;"">文章归档</a>
                <span style=""color:#ddd;margin:0 6px;"">|</span>
                <a href=""{siteUrl}/projects"" target=""_blank"" style=""color:#999;text-decoration:none;"">项目展示</a>
                <span style=""color:#ddd;margin:0 6px;"">|</span>
                <a href=""{siteUrl}/about"" target=""_blank"" style=""color:#999;text-decoration:none;"">关于我们</a>
            </div>
            <p style=""font-size:12px;color:#bbb;margin:10px 0 0;"">{siteName} · 用心分享，伴你成长</p>
            <p style=""font-size:11px;color:#ccc;margin:4px 0 0;"">您收到此邮件是因为您订阅了 {siteName} 的内容推送</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string GetDefaultDetailHtml()
    {
        return @"<!DOCTYPE html>
<html>
<head><meta charset=""utf-8"">
<style>
  @media only screen and (max-width: 480px) {
    .container { padding: 24px 16px !important; }
    .btn { padding: 12px 20px !important; width: 100% !important; box-sizing: border-box !important; }
  }
</style>
</head>
<body style=""margin:0;padding:0;background-color:#f0f2f5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI','PingFang SC','Hiragino Sans GB','Microsoft YaHei',sans-serif;"">
    <div class=""container"" style=""max-width:600px;margin:32px auto;padding:40px 36px;background-color:#ffffff;border-radius:16px;box-shadow:0 4px 12px rgba(0,0,0,0.06);"">
        
        <!-- 头部 -->
        <div style=""text-align:center;margin-bottom:28px;"">
            {siteHeaderBlock}
            <h1 style=""font-size:22px;color:#1a1a2e;margin:0;font-weight:700;"">{siteName}</h1>
            <p style=""font-size:13px;color:#8b8fa3;margin:4px 0 0;"">最新内容推送 · {currentDate}</p>
        </div>

        <!-- 内容区域 -->
        <div style=""padding:0 4px;"">
            <p style=""font-size:15px;color:#333;line-height:1.8;margin:0 0 16px;"">亲爱的读者，您好！</p>
            
            {content}

            <!-- 阅读更多 -->
            <div style=""text-align:center;margin:28px 0;"">
                <a class=""btn"" href=""{siteUrl}/articles"" target=""_blank"" style=""display:inline-block;padding:12px 36px;background:linear-gradient(135deg,#6366f1,#8b5cf6);color:#ffffff;text-decoration:none;border-radius:10px;font-size:15px;font-weight:600;box-shadow:0 4px 14px rgba(99,102,241,0.3);"">📖 浏览更多文章</a>
            </div>
        </div>

        <!-- 站点链接区 -->
        <div style=""text-align:center;margin:24px 0 20px;padding:16px 0;border-top:1px solid #eee;border-bottom:1px solid #eee;"">
            <p style=""font-size:13px;color:#666;margin:0 0 8px;"">您可能还感兴趣：</p>
            <div style=""font-size:13px;"">
                <a href=""{siteUrl}/projects"" target=""_blank"" style=""display:inline-block;padding:6px 16px;background:#f0f0ff;color:#6366f1;text-decoration:none;border-radius:6px;margin:4px 6px;font-weight:500;"">🚀 开源项目</a>
                <a href=""{siteUrl}/categories"" target=""_blank"" style=""display:inline-block;padding:6px 16px;background:#f0f0ff;color:#6366f1;text-decoration:none;border-radius:6px;margin:4px 6px;font-weight:500;"">📂 分类浏览</a>
                <a href=""{siteUrl}/about"" target=""_blank"" style=""display:inline-block;padding:6px 16px;background:#f0f0ff;color:#6366f1;text-decoration:none;border-radius:6px;margin:4px 6px;font-weight:500;"">👤 关于我们</a>
            </div>
        </div>

        <!-- 页脚 -->
        <div style=""text-align:center;padding-top:20px;"">
            <hr style=""border:none;border-top:1px solid #eee;margin:0 0 16px;"">
            <p style=""font-size:12px;color:#999;line-height:1.8;"">
                <a href=""{siteUrl}"" target=""_blank"" style=""color:#6366f1;text-decoration:none;font-weight:500;"">{siteName}</a>
                <span style=""color:#ddd;margin:0 6px;"">|</span>
                <a href=""{unsubscribeLink}"" target=""_blank"" style=""color:#999;text-decoration:none;"">退订推送</a>
            </p>
            <p style=""font-size:11px;color:#bbb;margin:4px 0 0;"">如果您不想再收到此类邮件，可点击上方「退订推送」链接</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string GetDefaultUnsubscribedHtml()
    {
        return @"<!DOCTYPE html>
<html>
<head><meta charset=""utf-8"">
<style>
  @media only screen and (max-width: 480px) {
    .container { padding: 24px 16px !important; }
    .btn { padding: 12px 20px !important; width: 100% !important; box-sizing: border-box !important; }
    .quick-links a { display: inline-block !important; margin: 3px 8px !important; }
  }
</style>
</head>
<body style=""margin:0;padding:0;background-color:#f0f2f5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI','PingFang SC','Hiragino Sans GB','Microsoft YaHei',sans-serif;"">
    <div class=""container"" style=""max-width:600px;margin:32px auto;padding:40px 36px;background-color:#ffffff;border-radius:16px;box-shadow:0 4px 12px rgba(0,0,0,0.06);"">
        
        <!-- 头部 -->
        <div style=""text-align:center;margin-bottom:28px;"">
            {siteHeaderBlock}
            <h1 style=""font-size:24px;color:#1a1a2e;margin:0;font-weight:700;"">{siteName}</h1>
            <p style=""font-size:14px;color:#8b8fa3;margin:6px 0 0;"">感谢您一路相伴</p>
        </div>

        <!-- 主体 -->
        <div style=""padding:0 4px;text-align:center;"">
            <div style=""width:72px;height:72px;background:#f0fdf4;border-radius:50%;display:inline-flex;align-items:center;justify-content:center;margin-bottom:16px;"">
                <span style=""font-size:36px;"">👋</span>
            </div>

            <h2 style=""font-size:20px;color:#1a1a2e;margin:0 0 12px;font-weight:600;"">已成功退订</h2>
            
            <p style=""font-size:15px;color:#555;line-height:1.8;margin:0 0 6px;"">
                您已成功取消 <strong style=""color:#6366f1;"">{siteName}</strong> 的邮件订阅。
            </p>
            <p style=""font-size:14px;color:#777;line-height:1.8;margin:0 0 20px;"">
                我们尊重您的选择，今后将不再向您推送邮件通知。
            </p>

            <!-- 反馈区 -->
            <div style=""background:#f8f9ff;border-radius:12px;padding:16px 20px;margin:20px 0 24px;text-align:left;"">
                <p style=""font-size:13px;color:#666;margin:0;line-height:1.8;"">
                    💬 如果您有任何建议或反馈，欢迎随时<br>
                    通过我们的网站 <a href=""{siteUrl}/guestbook"" target=""_blank"" style=""color:#6366f1;text-decoration:underline;"">留言板</a> 或 <a href=""{siteUrl}/about"" target=""_blank"" style=""color:#6366f1;text-decoration:underline;"">关于页面</a> 联系我们。
                </p>
            </div>

            <!-- 重新订阅 + 访问网站 -->
            <div style=""margin:28px 0;"">
                <a class=""btn"" href=""{subscribeLink}"" target=""_blank"" style=""display:inline-block;padding:14px 40px;background:linear-gradient(135deg,#6366f1,#8b5cf6);color:#ffffff;text-decoration:none;border-radius:10px;font-size:15px;font-weight:600;box-shadow:0 4px 14px rgba(99,102,241,0.3);margin-bottom:12px;"">📩 重新订阅</a>
                <br>
                <a href=""{siteUrl}"" target=""_blank"" style=""display:inline-block;padding:8px 24px;color:#6366f1;text-decoration:none;font-size:14px;font-weight:500;border:1px solid #d0d0ff;border-radius:8px;margin-top:8px;"">🏠 访问网站首页</a>
            </div>

            <!-- 快速链接 -->
            <div class=""quick-links"" style=""padding:16px 0;border-top:1px solid #eee;border-bottom:1px solid #eee;"">
                <p style=""font-size:13px;color:#666;margin:0 0 10px;font-weight:500;"">您可能还想看看：</p>
                <div style=""font-size:13px;"">
                    <a href=""{siteUrl}/articles"" target=""_blank"" style=""color:#6366f1;text-decoration:none;margin:0 10px;font-weight:500;"">📖 最新文章</a>
                    <a href=""{siteUrl}/projects"" target=""_blank"" style=""color:#6366f1;text-decoration:none;margin:0 10px;font-weight:500;"">🚀 开源项目</a>
                    <a href=""{siteUrl}/about"" target=""_blank"" style=""color:#6366f1;text-decoration:none;margin:0 10px;font-weight:500;"">👤 关于我们</a>
                </div>
            </div>
        </div>

        <!-- 页脚 -->
        <div style=""text-align:center;padding-top:20px;"">
            <hr style=""border:none;border-top:1px solid #eee;margin:0 0 16px;"">
            <p style=""font-size:12px;color:#999;line-height:1.8;"">
                <a href=""{siteUrl}"" target=""_blank"" style=""color:#6366f1;text-decoration:none;font-weight:500;"">{siteName}</a>
                <span style=""color:#ddd;margin:0 6px;"">|</span>
                <a href=""{siteUrl}/articles"" target=""_blank"" style=""color:#999;text-decoration:none;"">文章</a>
                <span style=""color:#ddd;margin:0 6px;"">|</span>
                <a href=""{siteUrl}/projects"" target=""_blank"" style=""color:#999;text-decoration:none;"">项目</a>
                <span style=""color:#ddd;margin:0 6px;"">|</span>
                <a href=""{siteUrl}/about"" target=""_blank"" style=""color:#999;text-decoration:none;"">关于</a>
            </p>
            <p style=""font-size:11px;color:#bbb;margin:4px 0 0;"">{siteName} · 用心分享，伴你成长</p>
        </div>
    </div>
</body>
</html>";
    }
}