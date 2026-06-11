using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Models.DTOs.Article;
using Mblog.API.Models.DTOs.Public;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 前台公开标签控制器
/// </summary>
[ApiController]
[Route("api/tags")]
public class PublicTagController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IArticleService _articleService;

    public PublicTagController(AppDbContext db, IArticleService articleService)
    {
        _db = db;
        _articleService = articleService;
    }

    /// <summary>
    /// 获取所有公开标签（按名称排序）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tags = await _db.Tags
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Name)
            .Select(x => new PublicTagInfo
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    Color = x.Color,
                    BgColor = x.BgColor,
                    ArticleCount = x.ArticleTags.Count(at => at.Article.DeletedAt == null && at.Article.Status == "published")
                })
                .ToListAsync();

        return Ok(ApiResponse.Ok(tags));
    }

    /// <summary>
    /// 根据 Slug 获取标签详情及其下的文章
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var tag = await _db.Tags
            .AsNoTracking()
            .Where(x => x.Slug == slug && x.DeletedAt == null)
            .Select(x => new PublicTagInfo
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    Color = x.Color,
                    BgColor = x.BgColor,
                    ArticleCount = x.ArticleTags.Count(at => at.Article.DeletedAt == null && at.Article.Status == "published")
                })
            .FirstOrDefaultAsync();

        if (tag == null)
            return NotFound(ApiResponse.Fail("标签不存在", 404));

        // 获取该标签下的文章
        var articles = await _articleService.GetPublicPagedAsync(new PublicArticleQueryParams
        {
            Page = page,
            PageSize = pageSize,
            TagId = tag.Id
        });

        return Ok(ApiResponse.Ok(new
        {
            tag,
            articles
        }));
    }
}