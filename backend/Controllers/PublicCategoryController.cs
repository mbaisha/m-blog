using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Models.DTOs.Article;
using Mblog.API.Models.DTOs.Public;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 前台公开分类控制器
/// </summary>
[ApiController]
[Route("api/categories")]
public class PublicCategoryController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IArticleService _articleService;

    public PublicCategoryController(AppDbContext db, IArticleService articleService)
    {
        _db = db;
        _articleService = articleService;
    }

    /// <summary>
    /// 获取所有公开分类（按排序序号升序）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .Include(x => x.CoverImage)
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new PublicCategoryInfo
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                CoverImageUrl = x.CoverImage != null ? x.CoverImage.Url : null,
                ArticleCount = x.Articles.Count(a => a.DeletedAt == null && a.Status == "published")
            })
            .ToListAsync();

        return Ok(ApiResponse.Ok(categories));
    }

    /// <summary>
    /// 根据 Slug 获取分类详情及其下的文章
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var category = await _db.Categories
            .AsNoTracking()
            .Include(x => x.CoverImage)
            .Where(x => x.Slug == slug && x.DeletedAt == null)
            .Select(x => new PublicCategoryInfo
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                CoverImageUrl = x.CoverImage != null ? x.CoverImage.Url : null,
                ArticleCount = x.Articles.Count(a => a.DeletedAt == null && a.Status == "published")
            })
            .FirstOrDefaultAsync();

        if (category == null)
            return NotFound(ApiResponse.Fail("分类不存在", 404));

        // 获取该分类下的文章
        var articles = await _articleService.GetPublicPagedAsync(new PublicArticleQueryParams
        {
            Page = page,
            PageSize = pageSize,
            CategoryId = category.Id
        });

        return Ok(ApiResponse.Ok(new
        {
            category,
            articles
        }));
    }
}