using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mblog.API.Controllers;
using Mblog.API.Data;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

/// <summary>
/// 数据导入服务 - 解析备份文件并批量导入数据库
/// </summary>
public class ImportService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ImportService> _logger;

    public ImportService(AppDbContext db, ILogger<ImportService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 从上传的文件流中解析并导入数据
    /// </summary>
    /// <param name="fileStream">上传的文件流</param>
    /// <param name="fileName">原始文件名（用于判断是 .json 还是 .zip）</param>
    /// <returns>导入报告</returns>
    public async Task<ImportReport> ImportAsync(Stream fileStream, string fileName)
    {
        var report = new ImportReport();
        var startedAt = DateTimeOffset.UtcNow;

        try
        {
            // 1. 解析文件 → JSON 字符串
            string json = await ReadJsonFromFileAsync(fileStream, fileName, report);

            // 2. 反序列化为 BackupData
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            var backup = JsonSerializer.Deserialize<BackupData>(json, options);
            if (backup == null)
            {
                report.Errors.Add("无法解析备份文件，JSON 反序列化失败");
                return report;
            }

            // 3. 开启事务，先删后插
            using var transaction = await _db.Database.BeginTransactionAsync();

            // ===== 按依赖顺序：先删子表/关联表，再删主表 =====
            await DeleteAllDataAsync(report);

            // ===== 按依赖顺序批量插入 =====
            await InsertAllDataAsync(backup, report);

            await transaction.CommitAsync();

            report.Success = report.Errors.Count == 0;
            report.ElapsedMs = (long)(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds;

            _logger.LogInformation("数据导入完成，成功: {SuccessCount}, 失败: {ErrorCount}, 耗时: {ElapsedMs}ms",
                report.SuccessCount, report.Errors.Count, report.ElapsedMs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据导入异常");
            report.Errors.Add($"导入异常: {ex.Message}");
        }

        return report;
    }

    /// <summary>
    /// 从文件流中读取 JSON 字符串（支持 .json 和 .zip）
    /// </summary>
    private static async Task<string> ReadJsonFromFileAsync(Stream fileStream, string fileName, ImportReport report)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        if (ext == ".zip")
        {
            using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read, leaveOpen: true);
            var jsonEntry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
            if (jsonEntry == null)
            {
                report.Errors.Add("ZIP 文件中未找到 .json 文件");
                return string.Empty;
            }

            await using var entryStream = jsonEntry.Open();
            using var reader = new StreamReader(entryStream);
            return await reader.ReadToEndAsync();
        }
        else
        {
            using var reader = new StreamReader(fileStream, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }
    }

    /// <summary>
    /// 删除所有表中数据（按外键依赖顺序，先删子表后删主表）
    /// </summary>
    private async Task DeleteAllDataAsync(ImportReport report)
    {
        var isSqlite = _db.Database.ProviderName?.Contains("Sqlite") == true;

        try
        {
            // 先删关联表 / 子表
            _db.ArticleTags.RemoveRange(await _db.ArticleTags.ToListAsync());
            _db.ArticleCategories.RemoveRange(await _db.ArticleCategories.ToListAsync());
            _db.Likes.RemoveRange(await _db.Likes.ToListAsync());
            _db.Comments.RemoveRange(await _db.Comments.IgnoreQueryFilters().ToListAsync());
            _db.Messages.RemoveRange(await _db.Messages.ToListAsync());
            _db.Visits.RemoveRange(await _db.Visits.ToListAsync());
            _db.AuditLogs.RemoveRange(await _db.AuditLogs.ToListAsync());
            _db.RefreshTokens.RemoveRange(await _db.RefreshTokens.ToListAsync());
            _db.CaptchaSessions.RemoveRange(await _db.CaptchaSessions.ToListAsync());
            _db.EmailLogs.RemoveRange(await _db.EmailLogs.ToListAsync());
            _db.EmailTemplates.RemoveRange(await _db.EmailTemplates.ToListAsync());
            _db.EmailSettings.RemoveRange(await _db.EmailSettings.ToListAsync());
            _db.Subscribers.RemoveRange(await _db.Subscribers.ToListAsync());

            await _db.SaveChangesAsync();

            // 再删主表
            _db.Media.RemoveRange(await _db.Media.IgnoreQueryFilters().ToListAsync());
            _db.Articles.RemoveRange(await _db.Articles.IgnoreQueryFilters().ToListAsync());
            _db.Categories.RemoveRange(await _db.Categories.IgnoreQueryFilters().ToListAsync());
            _db.Tags.RemoveRange(await _db.Tags.ToListAsync());
            _db.Projects.RemoveRange(await _db.Projects.IgnoreQueryFilters().ToListAsync());
            _db.Friends.RemoveRange(await _db.Friends.IgnoreQueryFilters().ToListAsync());
            _db.Pages.RemoveRange(await _db.Pages.IgnoreQueryFilters().ToListAsync());
            _db.NavigationItems.RemoveRange(await _db.NavigationItems.IgnoreQueryFilters().ToListAsync());
            _db.FooterConfigs.RemoveRange(await _db.FooterConfigs.ToListAsync());
            _db.ThemeSettings.RemoveRange(await _db.ThemeSettings.ToListAsync());
            _db.ModuleLayouts.RemoveRange(await _db.ModuleLayouts.IgnoreQueryFilters().ToListAsync());
            _db.SiteSettings.RemoveRange(await _db.SiteSettings.ToListAsync());
            _db.SeoSettings.RemoveRange(await _db.SeoSettings.ToListAsync());
            _db.ProfileSections.RemoveRange(await _db.ProfileSections.IgnoreQueryFilters().ToListAsync());
            _db.LlmConfigs.RemoveRange(await _db.LlmConfigs.ToListAsync());
            _db.ImageGenConfigs.RemoveRange(await _db.ImageGenConfigs.ToListAsync());
            _db.Users.RemoveRange(await _db.Users.IgnoreQueryFilters().ToListAsync());

            await _db.SaveChangesAsync();
            _logger.LogInformation("已清空所有表数据");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空数据失败");
            report.Errors.Add($"清空数据失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 按依赖顺序批量插入所有数据
    /// </summary>
    private async Task InsertAllDataAsync(BackupData backup, ImportReport report)
    {
        // ===== 阶段 1：插入所有主表 =====
        InsertBatch(report, "users", backup.Users, () => _db.Users.AddRange(backup.Users));
        InsertBatch(report, "site_settings", backup.SiteSettings, () => _db.SiteSettings.AddRange(backup.SiteSettings));
        InsertBatch(report, "theme_settings", backup.ThemeSettings, () => _db.ThemeSettings.AddRange(backup.ThemeSettings));
        InsertBatch(report, "media", backup.Media, () => _db.Media.AddRange(backup.Media));
        InsertBatch(report, "categories", backup.Categories, () => _db.Categories.AddRange(backup.Categories));
        InsertBatch(report, "tags", backup.Tags, () => _db.Tags.AddRange(backup.Tags));

        // Articles: 先清除导航属性，避免 EF Core 级联追踪 ArticleTags / ArticleCategories
        if (backup.Articles is { Count: > 0 })
        {
            foreach (var a in backup.Articles)
            {
                a.ArticleTags = null!;
                a.ArticleCategories = null!;
            }
            InsertBatch(report, "articles", backup.Articles, () => _db.Articles.AddRange(backup.Articles));
        }

        InsertBatch(report, "comments", backup.Comments, () => _db.Comments.AddRange(backup.Comments));
        InsertBatch(report, "messages", backup.Messages, () => _db.Messages.AddRange(backup.Messages));
        InsertBatch(report, "likes", backup.Likes, () => _db.Likes.AddRange(backup.Likes));
        InsertBatch(report, "visits", backup.Visits, () => _db.Visits.AddRange(backup.Visits));
        InsertBatch(report, "subscribers", backup.Subscribers, () => _db.Subscribers.AddRange(backup.Subscribers));
        InsertBatch(report, "captcha_sessions", backup.CaptchaSessions, () => _db.CaptchaSessions.AddRange(backup.CaptchaSessions));
        InsertBatch(report, "email_settings", backup.EmailSettings, () => _db.EmailSettings.AddRange(backup.EmailSettings));
        InsertBatch(report, "email_templates", backup.EmailTemplates, () => _db.EmailTemplates.AddRange(backup.EmailTemplates));
        InsertBatch(report, "email_logs", backup.EmailLogs, () => _db.EmailLogs.AddRange(backup.EmailLogs));
        InsertBatch(report, "audit_logs", backup.AuditLogs, () => _db.AuditLogs.AddRange(backup.AuditLogs));
        InsertBatch(report, "refresh_tokens", backup.RefreshTokens, () => _db.RefreshTokens.AddRange(backup.RefreshTokens));
        InsertBatch(report, "projects", backup.Projects, () => _db.Projects.AddRange(backup.Projects));
        InsertBatch(report, "friends", backup.Friends, () => _db.Friends.AddRange(backup.Friends));
        InsertBatch(report, "pages", backup.Pages, () => _db.Pages.AddRange(backup.Pages));
        InsertBatch(report, "navigation_items", backup.NavigationItems, () => _db.NavigationItems.AddRange(backup.NavigationItems));
        InsertBatch(report, "footer_configs", backup.FooterConfigs, () => _db.FooterConfigs.AddRange(backup.FooterConfigs));
        InsertBatch(report, "module_layouts", backup.ModuleLayouts, () => _db.ModuleLayouts.AddRange(backup.ModuleLayouts));
        InsertBatch(report, "seo_settings", backup.SeoSettings, () => _db.SeoSettings.AddRange(backup.SeoSettings));
        InsertBatch(report, "profile_sections", backup.ProfileSections, () => _db.ProfileSections.AddRange(backup.ProfileSections));
        InsertBatch(report, "llm_configs", backup.LlmConfigs, () => _db.LlmConfigs.AddRange(backup.LlmConfigs));
        InsertBatch(report, "image_gen_configs", backup.ImageGenConfigs, () => _db.ImageGenConfigs.AddRange(backup.ImageGenConfigs));

        // 主表先落库
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "主表批量保存失败");
            report.Errors.Add($"主表批量保存失败: {ex.Message}");
            throw;
        }

        // ===== 阶段 2：插入关联表 =====
        // ArticleTags / ArticleCategories 需要单独插入，避免与 Articles 导航属性冲突
        if (backup.ArticleTags is { Count: > 0 })
        {
            InsertBatchDirect(report, "article_tags", backup.ArticleTags);
        }
        if (backup.ArticleCategories is { Count: > 0 })
        {
            InsertBatchDirect(report, "article_categories", backup.ArticleCategories);
        }

        if (backup.ArticleTags is { Count: > 0 } or { Count: > 0 } || backup.ArticleCategories is { Count: > 0 })
        {
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "关联表批量保存失败");
                report.Errors.Add($"关联表批量保存失败: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// 直接通过 SQL 批量插入关联表实体（绕过 EF 追踪冲突）
    /// </summary>
    private void InsertBatchDirect(ImportReport report, string tableName, System.Collections.IList items)
    {
        try
        {
            foreach (var item in items)
            {
                _db.Entry(item).State = EntityState.Added;
            }
            report.TableResults.Add(new TableImportResult
            {
                TableName = tableName,
                SuccessCount = items.Count,
            });
            report.SuccessCount += items.Count;
        }
        catch (Exception ex)
        {
            report.TableResults.Add(new TableImportResult
            {
                TableName = tableName,
                SuccessCount = 0,
                ErrorCount = items.Count,
                Error = ex.Message,
            });
            report.Errors.Add($"表 [{tableName}] 导入失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 执行单表批量插入并记录结果
    /// </summary>
    private void InsertBatch(ImportReport report, string tableName, System.Collections.IList? items, Action addAction)
    {
        if (items == null || items.Count == 0) return;

        try
        {
            addAction();
            report.TableResults.Add(new TableImportResult
            {
                TableName = tableName,
                SuccessCount = items.Count,
            });
            report.SuccessCount += items.Count;
        }
        catch (Exception ex)
        {
            report.TableResults.Add(new TableImportResult
            {
                TableName = tableName,
                SuccessCount = 0,
                ErrorCount = items.Count,
                Error = ex.Message,
            });
            report.Errors.Add($"表 [{tableName}] 导入失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 导入报告
/// </summary>
public class ImportReport
{
    /// <summary>是否全部成功</summary>
    public bool Success { get; set; }

    /// <summary>成功导入记录总数</summary>
    public int SuccessCount { get; set; }

    /// <summary>耗时（毫秒）</summary>
    public long ElapsedMs { get; set; }

    /// <summary>各表导入明细</summary>
    public List<TableImportResult> TableResults { get; set; } = new();

    /// <summary>全局错误信息</summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// 单表导入结果
/// </summary>
public class TableImportResult
{
    public string TableName { get; set; } = string.Empty;
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public string? Error { get; set; }
}
