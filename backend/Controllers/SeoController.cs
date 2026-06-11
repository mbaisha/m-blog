using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// SEO 公开控制器（前台）
/// </summary>
[ApiController]
[Route("api/seo")]
public class SeoController : ControllerBase
{
    private readonly ISeoService _seoService;

    public SeoController(ISeoService seoService)
    {
        _seoService = seoService;
    }

    /// <summary>
    /// 获取指定页面的 SEO 配置
    /// GET /api/seo?key=home
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByPageKey([FromQuery] string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return BadRequest(ApiResponse.Fail("pageKey 不能为空"));

        var seo = await _seoService.GetByPageKeyAsync(key);
        if (seo == null)
            return Ok(ApiResponse.Ok(new { }));

        return Ok(ApiResponse.Ok(seo));
    }
}

/// <summary>
/// SEO 管理控制器（后台）
/// </summary>
[ApiController]
[Route("api/admin/seo")]
[Authorize]
public class AdminSeoController : ControllerBase
{
    private readonly ISeoService _seoService;

    public AdminSeoController(ISeoService seoService)
    {
        _seoService = seoService;
    }

    /// <summary>
    /// 获取所有 SEO 配置
    /// GET /api/admin/seo
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var seos = await _seoService.GetAllAsync();
        return Ok(ApiResponse.Ok(seos));
    }

    /// <summary>
    /// 创建或更新 SEO 配置
    /// PUT /api/admin/seo
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> Save([FromBody] UpdateSeoSettingRequest request)
    {
        var seo = await _seoService.SaveAsync(request);
        return Ok(ApiResponse.Ok(seo, "SEO 配置已保存"));
    }

    /// <summary>
    /// 生成 Sitemap
    /// POST /api/admin/seo/sitemap/generate
    /// </summary>
    [HttpPost("sitemap/generate")]
    public async Task<IActionResult> GenerateSitemap()
    {
        var xml = await _seoService.GenerateSitemapXmlAsync();
        return Ok(ApiResponse.Ok(new { content = xml }, "Sitemap 已生成"));
    }
}

/// <summary>
/// Sitemap & Robots 控制器（前台，返回 XML/Text）
/// </summary>
[ApiController]
public class SitemapController : ControllerBase
{
    private readonly ISeoService _seoService;

    public SitemapController(ISeoService seoService)
    {
        _seoService = seoService;
    }

    /// <summary>
    /// 获取 Sitemap.xml
    /// GET /api/sitemap.xml
    /// </summary>
    [HttpGet("/api/sitemap.xml")]
    public async Task<IActionResult> GetSitemap()
    {
        var xml = await _seoService.GenerateSitemapXmlAsync();
        return Content(xml, "application/xml", System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// 获取 Robots.txt
    /// GET /api/robots.txt
    /// </summary>
    [HttpGet("/api/robots.txt")]
    public async Task<IActionResult> GetRobots()
    {
        var txt = await _seoService.GetRobotsTxtAsync();
        return Content(txt, "text/plain", System.Text.Encoding.UTF8);
    }
}