using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.DTOs.Article;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 文章管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/articles")]
[Authorize]
public class ArticleController : ControllerBase
{
    private readonly IArticleService _articleService;
    private readonly IArticleAiService _articleAiService;

    public ArticleController(IArticleService articleService, IArticleAiService articleAiService)
    {
        _articleService = articleService;
        _articleAiService = articleAiService;
    }

    /// <summary>
    /// 分页查询文章列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] ArticleQueryParams query)
    {
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1) query.PageSize = 10;
        if (query.PageSize > 100) query.PageSize = 100;

        var result = await _articleService.GetPagedAsync(query);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 根据 ID 获取文章详情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var article = await _articleService.GetByIdAsync(id);
        if (article == null)
            return NotFound(ApiResponse.Fail("文章不存在", 404));

        return Ok(ApiResponse.Ok(article));
    }

    /// <summary>
    /// 创建文章
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateArticleRequest request)
    {
        var authorId = GetCurrentUserId();

        try
        {
            var article = await _articleService.CreateAsync(request, authorId);
            return Ok(ApiResponse.Ok(article, "文章创建成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 更新文章
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArticleRequest request)
    {
        try
        {
            var article = await _articleService.UpdateAsync(id, request);
            if (article == null)
                return NotFound(ApiResponse.Fail("文章不存在", 404));

            return Ok(ApiResponse.Ok(article, "文章更新成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 更新文章状态（发布/下架/归档）
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateArticleStatusRequest request)
    {
        try
        {
            var article = await _articleService.UpdateStatusAsync(id, request.Status);
            if (article == null)
                return NotFound(ApiResponse.Fail("文章不存在", 404));

            return Ok(ApiResponse.Ok(article, "文章状态更新成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 切换置顶状态
    /// </summary>
    [HttpPut("{id:guid}/toggle-top")]
    public async Task<IActionResult> ToggleTop(Guid id)
    {
        var result = await _articleService.ToggleTopAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail("文章不存在", 404));

        return Ok(ApiResponse.Ok("置顶状态已切换"));
    }

    /// <summary>
    /// 切换推荐状态
    /// </summary>
    [HttpPut("{id:guid}/toggle-recommend")]
    public async Task<IActionResult> ToggleRecommend(Guid id)
    {
        var result = await _articleService.ToggleRecommendAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail("文章不存在", 404));

        return Ok(ApiResponse.Ok("推荐状态已切换"));
    }

    /// <summary>
    /// 删除文章（软删除）
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _articleService.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail("文章不存在", 404));

        return Ok(ApiResponse.Ok("文章删除成功"));
    }

    /// <summary>
    /// 彻底删除文章（硬删除，从数据库移除）
    /// </summary>
    [HttpDelete("{id:guid}/force")]
    public async Task<IActionResult> HardDelete(Guid id)
    {
        var result = await _articleService.HardDeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail("文章不存在", 404));

        return Ok(ApiResponse.Ok("文章已彻底删除"));
    }

    /// <summary>
    /// 恢复文章（从回收站还原）
    /// </summary>
    [HttpPut("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _articleService.RestoreAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail("文章不存在或未被删除", 404));

        return Ok(ApiResponse.Ok("文章已恢复"));
    }

    /// <summary>
    /// AI 提取文章信息（标题、摘要、SEO）
    /// </summary>
    [HttpPost("ai/extract")]
    public async Task<IActionResult> AiExtract([FromBody] AiExtractRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(ApiResponse.Fail("文章内容不能为空"));

        var result = await _articleAiService.ExtractInfoAsync(request.Content, request.CurrentTitle);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// AI 生成文章封面图
    /// </summary>
    [HttpPost("ai/generate-cover")]
    public async Task<IActionResult> AiGenerateCover([FromBody] AiGenerateCoverRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(ApiResponse.Fail("文章内容不能为空"));

        var userId = GetCurrentUserId();
        var result = await _articleAiService.GenerateCoverAsync(request.Title, request.Content, userId);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// AI 润色文章内容（可选自动配图）
    /// </summary>
    [HttpPost("ai/polish")]
    public async Task<IActionResult> AiPolish([FromBody] AiPolishRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(ApiResponse.Fail("文章内容不能为空"));

        var userId = GetCurrentUserId();
        var result = await _articleAiService.PolishAsync(request.Content, request.GenerateImages, request.AspectRatio, userId);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// AI 一键写文（灵感 -> 完整文章）
    /// </summary>
    [HttpPost("ai/write")]
    public async Task<IActionResult> AiWrite([FromBody] AiWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Inspiration))
            return BadRequest(ApiResponse.Fail("请输入灵感或想法"));

        var userId = GetCurrentUserId();
        var result = await _articleAiService.WriteAsync(request.Inspiration, request.GenerateCover, request.GenerateArticleImages, request.ArticleImageAspectRatio, userId);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// AI 生成文章内配图（插入编辑器光标位置）
    /// </summary>
    [HttpPost("ai/generate-article-image")]
    public async Task<IActionResult> AiGenerateArticleImage([FromBody] AiGenerateArticleImageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(ApiResponse.Fail("提示词不能为空"));

        var userId = GetCurrentUserId();
        var result = await _articleAiService.GenerateArticleImageAsync(request.Prompt, request.AspectRatio, userId);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 从 JWT Claims 中获取当前用户 ID
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("无法获取当前用户信息");

        return userId;
    }
}