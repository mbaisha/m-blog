using Microsoft.EntityFrameworkCore;
using Mblog.API.Data;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

/// <summary>
/// SEO 配置服务接口
/// </summary>
public interface ISeoService
{
    /// <summary>根据 PageKey 获取公开 SEO 配置</summary>
    Task<SeoSettingResponse?> GetByPageKeyAsync(string pageKey);

    /// <summary>获取所有 SEO 配置（后台）</summary>
    Task<List<SeoSettingResponse>> GetAllAsync();

    /// <summary>创建或更新 SEO 配置</summary>
    Task<SeoSettingResponse> SaveAsync(UpdateSeoSettingRequest request);

    /// <summary>生成 Sitemap XML 内容</summary>
    Task<string> GenerateSitemapXmlAsync();

    /// <summary>生成 Robots.txt 内容</summary>
    Task<string> GetRobotsTxtAsync();
}

/// <summary>
/// SEO 配置服务，管理页面 SEO 元信息、Sitemap、Robots.txt
/// </summary>
public class SeoService : ISeoService
{
    private readonly AppDbContext _db;
    private readonly ISiteSettingService _siteSettingService;

    public SeoService(AppDbContext db, ISiteSettingService siteSettingService)
    {
        _db = db;
        _siteSettingService = siteSettingService;
    }

    public async Task<SeoSettingResponse?> GetByPageKeyAsync(string pageKey)
    {
        return await _db.SeoSettings
            .AsNoTracking()
            .Include(s => s.OgImage)
            .Where(s => s.PageKey == pageKey)
            .Select(s => new SeoSettingResponse
            {
                Id = s.Id,
                PageKey = s.PageKey,
                Title = s.Title,
                Description = s.Description,
                Keywords = s.Keywords,
                OgImageUrl = s.OgImage != null ? s.OgImage.Url : null,
                CanonicalUrl = s.CanonicalUrl
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<SeoSettingResponse>> GetAllAsync()
    {
        return await _db.SeoSettings
            .AsNoTracking()
            .Include(s => s.OgImage)
            .OrderBy(s => s.PageKey)
            .Select(s => new SeoSettingResponse
            {
                Id = s.Id,
                PageKey = s.PageKey,
                Title = s.Title,
                Description = s.Description,
                Keywords = s.Keywords,
                OgImageUrl = s.OgImage != null ? s.OgImage.Url : null,
                CanonicalUrl = s.CanonicalUrl
            })
            .ToListAsync();
    }

    public async Task<SeoSettingResponse> SaveAsync(UpdateSeoSettingRequest request)
    {
        var setting = await _db.SeoSettings
            .FirstOrDefaultAsync(s => s.PageKey == request.PageKey);

        if (setting == null)
        {
            setting = new SeoSetting
            {
                PageKey = request.PageKey,
                Title = request.Title,
                Description = request.Description,
                Keywords = request.Keywords,
                OgImageId = request.OgImageId,
                CanonicalUrl = request.CanonicalUrl
            };
            _db.SeoSettings.Add(setting);
        }
        else
        {
            setting.Title = request.Title;
            setting.Description = request.Description;
            setting.Keywords = request.Keywords;
            setting.OgImageId = request.OgImageId;
            setting.CanonicalUrl = request.CanonicalUrl;
            _db.SeoSettings.Update(setting);
        }

        await _db.SaveChangesAsync();

        // 重新查询以获取导航属性
        return (await GetByPageKeyAsync(request.PageKey))!;
    }

    public async Task<string> GenerateSitemapXmlAsync()
    {
        var siteSetting = await _siteSettingService.GetPublicAsync();
        var baseUrl = "https://example.com"; // 默认值，实际应来自配置

        var urls = new List<string>
        {
            $"  <url><loc>{baseUrl}/</loc><priority>1.0</priority></url>",
            $"  <url><loc>{baseUrl}/articles</loc><priority>0.9</priority></url>",
            $"  <url><loc>{baseUrl}/archive</loc><priority>0.7</priority></url>",
            $"  <url><loc>{baseUrl}/categories</loc><priority>0.6</priority></url>",
            $"  <url><loc>{baseUrl}/tags</loc><priority>0.6</priority></url>",
            $"  <url><loc>{baseUrl}/projects</loc><priority>0.8</priority></url>",
        };

        // 已发布文章
        var articles = await _db.Articles
            .Where(a => a.DeletedAt == null && a.Status == "published")
            .Select(a => new { a.Slug, a.UpdatedAt })
            .ToListAsync();

        foreach (var article in articles)
        {
            var lastmod = article.UpdatedAt.ToString("yyyy-MM-dd");
            urls.Add($"  <url><loc>{baseUrl}/articles/{article.Slug}</loc><lastmod>{lastmod}</lastmod><priority>0.8</priority></url>");
        }

        // 已发布页面
        var pages = await _db.Pages
            .Where(p => p.Status == "published" && p.IsVisible)
            .Select(p => new { p.Slug, p.UpdatedAt })
            .ToListAsync();

        foreach (var page in pages)
        {
            var lastmod = page.UpdatedAt.ToString("yyyy-MM-dd");
            urls.Add($"  <url><loc>{baseUrl}/{page.Slug}</loc><lastmod>{lastmod}</lastmod><priority>0.5</priority></url>");
        }

        var xml = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n{string.Join("\n", urls)}\n</urlset>";
        return xml;
    }

    public async Task<string> GetRobotsTxtAsync()
    {
        var siteSetting = await _siteSettingService.GetPublicAsync();
        var baseUrl = "https://example.com"; // 默认值

        return $@"User-agent: *
Allow: /
Disallow: /admin/
Disallow: /api/

Sitemap: {baseUrl}/api/sitemap.xml
";
    }
}