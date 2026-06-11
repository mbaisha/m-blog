using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 自定义页面管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/pages")]
[Authorize]
public class PageController : ControllerBase
{
    private readonly IPageService _pageService;

    public PageController(IPageService pageService) => _pageService = pageService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pages = await _pageService.GetAllAsync();
        return Ok(ApiResponse.Ok(pages));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var page = await _pageService.GetByIdAsync(id);
        if (page == null) return NotFound(ApiResponse.Fail("页面不存在", 404));
        return Ok(ApiResponse.Ok(page));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePageRequest request)
    {
        try
        {
            var page = await _pageService.CreateAsync(request);
            return Ok(ApiResponse.Ok(page, "页面创建成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePageRequest request)
    {
        try
        {
            var page = await _pageService.UpdateAsync(id, request);
            if (page == null) return NotFound(ApiResponse.Fail("页面不存在", 404));
            return Ok(ApiResponse.Ok(page, "页面更新成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _pageService.DeleteAsync(id);
        if (!result) return NotFound(ApiResponse.Fail("页面不存在", 404));
        return Ok(ApiResponse.Ok("页面已删除"));
    }

    /// <summary>
    /// 发布页面
    /// POST /api/admin/pages/{id}/publish
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var page = await _pageService.PublishAsync(id);
        if (page == null) return NotFound(ApiResponse.Fail("页面不存在", 404));
        return Ok(ApiResponse.Ok(page, "页面已发布"));
    }

    /// <summary>
    /// 下架页面
    /// POST /api/admin/pages/{id}/unpublish
    /// </summary>
    [HttpPost("{id:guid}/unpublish")]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        var page = await _pageService.UnpublishAsync(id);
        if (page == null) return NotFound(ApiResponse.Fail("页面不存在", 404));
        return Ok(ApiResponse.Ok(page, "页面已下架"));
    }
}

/// <summary>
/// 自定义页面公开控制器（前台）
/// </summary>
[ApiController]
[Route("api/pages")]
public class PublicPageController : ControllerBase
{
    private readonly IPageService _pageService;

    public PublicPageController(IPageService pageService) => _pageService = pageService;

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var pages = await _pageService.GetPublishedListAsync();
        return Ok(ApiResponse.Ok(pages));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var page = await _pageService.GetPublishedBySlugAsync(slug);
        if (page == null) return NotFound(ApiResponse.Fail("页面不存在", 404));
        return Ok(ApiResponse.Ok(page));
    }
}