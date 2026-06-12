using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Models.Entities;

namespace Mblog.API.Controllers;

/// <summary>
/// 数据库备份与导出控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/backup")]
[Authorize]
public class BackupController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<BackupController> _logger;

    public BackupController(AppDbContext db, ILogger<BackupController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 导出全量数据为 JSON 文件（包含所有表的所有行所有列）
    /// </summary>
    /// <param name="zip">是否压缩为 .zip，默认 false</param>
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] bool zip = false)
    {
        try
        {
            var backup = new BackupData
            {
                ExportedAt = DateTimeOffset.UtcNow,
                Version = "2.0",

                // ===== 所有 30 张表，全量导出（忽略软删除过滤器）=====

                // 用户与认证
                Users = await _db.Users.IgnoreQueryFilters().AsNoTracking().ToListAsync(),
                RefreshTokens = await _db.RefreshTokens.AsNoTracking().ToListAsync(),

                // 内容核心
                Articles = await _db.Articles.IgnoreQueryFilters().AsNoTracking()
                    .Include(x => x.ArticleCategories)
                    .Include(x => x.ArticleTags)
                    .ToListAsync(),
                ArticleTags = await _db.ArticleTags.AsNoTracking().ToListAsync(),
                ArticleCategories = await _db.ArticleCategories.AsNoTracking().ToListAsync(),
                Categories = await _db.Categories.IgnoreQueryFilters().AsNoTracking().ToListAsync(),
                Tags = await _db.Tags.AsNoTracking().ToListAsync(),

                // 项目与友链
                Projects = await _db.Projects.IgnoreQueryFilters().AsNoTracking().ToListAsync(),
                Friends = await _db.Friends.IgnoreQueryFilters().AsNoTracking().ToListAsync(),

                // 页面与导航
                Pages = await _db.Pages.IgnoreQueryFilters().AsNoTracking().ToListAsync(),
                NavigationItems = await _db.NavigationItems.IgnoreQueryFilters().AsNoTracking().ToListAsync(),
                FooterConfigs = await _db.FooterConfigs.AsNoTracking().ToListAsync(),

                // 主题与布局
                ThemeSettings = await _db.ThemeSettings.AsNoTracking().ToListAsync(),
                ModuleLayouts = await _db.ModuleLayouts.IgnoreQueryFilters().AsNoTracking().ToListAsync(),

                // 站点与 SEO
                SiteSettings = await _db.SiteSettings.AsNoTracking().ToListAsync(),
                SeoSettings = await _db.SeoSettings.AsNoTracking().ToListAsync(),

                // 个人页面
                ProfileSections = await _db.ProfileSections.IgnoreQueryFilters().AsNoTracking().ToListAsync(),

                // 互动
                Comments = await _db.Comments.IgnoreQueryFilters().AsNoTracking().ToListAsync(),
                Messages = await _db.Messages.AsNoTracking().ToListAsync(),
                Likes = await _db.Likes.AsNoTracking().ToListAsync(),
                Visits = await _db.Visits.AsNoTracking().ToListAsync(),

                // 订阅与验证码
                Subscribers = await _db.Subscribers.AsNoTracking().ToListAsync(),
                CaptchaSessions = await _db.CaptchaSessions.AsNoTracking().ToListAsync(),

                // 邮件
                EmailSettings = await _db.EmailSettings.AsNoTracking().ToListAsync(),
                EmailTemplates = await _db.EmailTemplates.AsNoTracking().ToListAsync(),
                EmailLogs = await _db.EmailLogs.AsNoTracking().ToListAsync(),

                // 审计日志
                AuditLogs = await _db.AuditLogs.AsNoTracking().ToListAsync(),

                // 媒体
                Media = await _db.Media.IgnoreQueryFilters().AsNoTracking().ToListAsync(),

                // AI 配置
                LlmConfigs = await _db.LlmConfigs.AsNoTracking().ToListAsync(),
                ImageGenConfigs = await _db.ImageGenConfigs.AsNoTracking().ToListAsync(),
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            var json = JsonSerializer.Serialize(backup, jsonOptions);

            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

            if (zip)
            {
                // 压缩为 .json.zip
                using var memoryStream = new MemoryStream();
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    var entry = archive.CreateEntry($"mblog-backup-{DateTime.Now:yyyyMMdd-HHmmss}.json");
                    using var entryStream = entry.Open();
                    entryStream.Write(jsonBytes, 0, jsonBytes.Length);
                }

                var zipBytes = memoryStream.ToArray();
                var zipFileName = $"mblog-backup-{DateTime.Now:yyyyMMdd-HHmmss}.json.zip";

                _logger.LogInformation("数据备份导出成功(ZIP)，JSON: {JsonSize} bytes, ZIP: {ZipSize} bytes",
                    jsonBytes.Length, zipBytes.Length);

                return File(zipBytes, "application/zip", zipFileName);
            }
            else
            {
                // 直接导出 .json
                var fileName = $"mblog-backup-{DateTime.Now:yyyyMMdd-HHmmss}.json";

                _logger.LogInformation("数据备份导出成功(JSON)，大小: {Size} bytes", jsonBytes.Length);

                return File(jsonBytes, "application/json; charset=utf-8", fileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据备份导出失败");
            return StatusCode(500, ApiResponse.Fail("导出失败: " + ex.Message));
        }
    }

    /// <summary>
    /// 获取备份概览（全部 30 张表的数据量统计）
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = new
        {
            // 用户与认证
            users = await _db.Users.IgnoreQueryFilters().CountAsync(),
            refreshTokens = await _db.RefreshTokens.CountAsync(),

            // 内容核心
            articles = await _db.Articles.IgnoreQueryFilters().CountAsync(),
            articleTags = await _db.ArticleTags.CountAsync(),
            articleCategories = await _db.ArticleCategories.CountAsync(),
            categories = await _db.Categories.IgnoreQueryFilters().CountAsync(),
            tags = await _db.Tags.CountAsync(),

            // 项目与友链
            projects = await _db.Projects.IgnoreQueryFilters().CountAsync(),
            friends = await _db.Friends.IgnoreQueryFilters().CountAsync(),

            // 页面与导航
            pages = await _db.Pages.IgnoreQueryFilters().CountAsync(),
            navigationItems = await _db.NavigationItems.IgnoreQueryFilters().CountAsync(),
            footerConfigs = await _db.FooterConfigs.CountAsync(),

            // 主题与布局
            themeSettings = await _db.ThemeSettings.CountAsync(),
            moduleLayouts = await _db.ModuleLayouts.IgnoreQueryFilters().CountAsync(),

            // 站点与 SEO
            siteSettings = await _db.SiteSettings.CountAsync(),
            seoSettings = await _db.SeoSettings.CountAsync(),

            // 个人页面
            profileSections = await _db.ProfileSections.IgnoreQueryFilters().CountAsync(),

            // 互动
            comments = await _db.Comments.IgnoreQueryFilters().CountAsync(),
            messages = await _db.Messages.CountAsync(),
            likes = await _db.Likes.CountAsync(),
            visits = await _db.Visits.CountAsync(),

            // 订阅与验证码
            subscribers = await _db.Subscribers.CountAsync(),
            captchaSessions = await _db.CaptchaSessions.CountAsync(),

            // 邮件
            emailSettings = await _db.EmailSettings.CountAsync(),
            emailTemplates = await _db.EmailTemplates.CountAsync(),
            emailLogs = await _db.EmailLogs.CountAsync(),

            // 审计日志
            auditLogs = await _db.AuditLogs.CountAsync(),

            // 媒体
            media = await _db.Media.IgnoreQueryFilters().CountAsync(),

            // AI 配置
            llmConfigs = await _db.LlmConfigs.CountAsync(),
            imageGenConfigs = await _db.ImageGenConfigs.CountAsync(),
        };

        return Ok(ApiResponse.Ok(stats));
    }
}

/// <summary>
/// 备份数据结构 - 包含数据库所有 30 张表
/// </summary>
public class BackupData
{
    public DateTimeOffset ExportedAt { get; set; }
    public string Version { get; set; } = string.Empty;

    // 用户与认证
    public List<User> Users { get; set; } = new();
    public List<RefreshToken> RefreshTokens { get; set; } = new();

    // 内容核心
    public List<Article> Articles { get; set; } = new();
    public List<ArticleTag> ArticleTags { get; set; } = new();
    public List<ArticleCategory> ArticleCategories { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Tag> Tags { get; set; } = new();

    // 项目与友链
    public List<Project> Projects { get; set; } = new();
    public List<Friend> Friends { get; set; } = new();

    // 页面与导航
    public List<Page> Pages { get; set; } = new();
    public List<NavigationItem> NavigationItems { get; set; } = new();
    public List<FooterConfig> FooterConfigs { get; set; } = new();

    // 主题与布局
    public List<ThemeSetting> ThemeSettings { get; set; } = new();
    public List<ModuleLayout> ModuleLayouts { get; set; } = new();

    // 站点与 SEO
    public List<SiteSetting> SiteSettings { get; set; } = new();
    public List<SeoSetting> SeoSettings { get; set; } = new();

    // 个人页面
    public List<ProfileSection> ProfileSections { get; set; } = new();

    // 互动
    public List<Comment> Comments { get; set; } = new();
    public List<Message> Messages { get; set; } = new();
    public List<Like> Likes { get; set; } = new();
    public List<Visit> Visits { get; set; } = new();

    // 订阅与验证码
    public List<Subscriber> Subscribers { get; set; } = new();
    public List<CaptchaSession> CaptchaSessions { get; set; } = new();

    // 邮件
    public List<EmailSetting> EmailSettings { get; set; } = new();
    public List<EmailTemplate> EmailTemplates { get; set; } = new();
    public List<EmailLog> EmailLogs { get; set; } = new();

    // 审计日志
    public List<AuditLog> AuditLogs { get; set; } = new();

    // 媒体
    public List<Media> Media { get; set; } = new();

    // AI 配置
    public List<LlmConfig> LlmConfigs { get; set; } = new();
    public List<ImageGenConfig> ImageGenConfigs { get; set; } = new();
}
