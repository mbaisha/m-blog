using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs.Article;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 前台公开文章控制器
/// </summary>
[ApiController]
[Route("api/articles")]
public class PublicArticleController : ControllerBase
{
    private readonly IArticleService _articleService;

    public PublicArticleController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    /// <summary>
    /// 前台分页获取已发布的文章列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PublicArticleQueryParams query)
    {
        var result = await _articleService.GetPublicPagedAsync(query);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 根据 Slug 获取已发布的文章详情
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var article = await _articleService.GetPublicBySlugAsync(slug);
        if (article == null)
            return NotFound(ApiResponse.Fail("文章不存在", 404));

        return Ok(ApiResponse.Ok(article));
    }

    /// <summary>
    /// 获取相关文章
    /// </summary>
    [HttpGet("{slug}/related")]
    public async Task<IActionResult> GetRelated(string slug, [FromQuery] int count = 3)
    {
        var articles = await _articleService.GetRelatedAsync(slug, count);
        return Ok(ApiResponse.Ok(articles));
    }

    /// <summary>
    /// 增加文章阅读数
    /// </summary>
    [HttpPost("{slug}/view")]
    public async Task<IActionResult> IncrementView(string slug)
    {
        var result = await _articleService.IncrementViewCountAsync(slug);
        if (!result)
            return NotFound(ApiResponse.Fail("文章不存在", 404));

        return Ok(ApiResponse.Ok("阅读数已更新"));
    }
}