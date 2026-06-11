using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Mblog.API.Common;
using Mblog.API.Models.DTOs.Comment;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 前台评论控制器（公开接口，无需登录）
/// </summary>
[ApiController]
[EnableRateLimiting("Global")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// 获取文章评论列表（GET /api/articles/{articleId}/comments）
    /// 只返回已审核通过的评论
    /// </summary>
    [HttpGet("api/articles/{articleId:guid}/comments")]
    public async Task<IActionResult> GetList(Guid articleId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _commentService.GetApprovedAsync(articleId, page, pageSize);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 提交评论（POST /api/comments）
    /// </summary>
    [HttpPost("api/comments")]
    [EnableRateLimiting("CommentPerMinute")]
    public async Task<IActionResult> Create([FromBody] CreateCommentRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        var xForwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].ToString();
        var xRealIp = HttpContext.Request.Headers["X-Real-IP"].ToString();

        var result = await _commentService.CreateAsync(request, ip, userAgent, xForwardedFor, xRealIp);
        if (result.Status == "error")
            return UnprocessableEntity(ApiResponse.Fail(result.Message, 422));

        return Ok(ApiResponse.Ok(new CreateCommentResponse
        {
            Status = result.Status,
            Message = result.Message,
        }));
    }
}