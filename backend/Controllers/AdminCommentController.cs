using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs.Comment;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 后台评论管理控制器（需要登录）
/// </summary>
[ApiController]
[Route("api/admin/comments")]
[Authorize]
public class AdminCommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public AdminCommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// 获取评论列表（GET /api/admin/comments）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] AdminCommentQueryParams query)
    {
        var result = await _commentService.GetAdminListAsync(query);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 审核通过评论（POST /api/admin/comments/{id}/approve）
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var userId = GetUserId();
        var result = await _commentService.ApproveAsync(id, userId);
        if (!result)
            return NotFound(ApiResponse.Fail("评论不存在", 404));

        return Ok(ApiResponse.Ok("审核通过"));
    }

    /// <summary>
    /// 拒绝评论（POST /api/admin/comments/{id}/reject）
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectCommentRequest? request)
    {
        var userId = GetUserId();
        var result = await _commentService.RejectAsync(id, userId, request?.Reason);
        if (!result)
            return NotFound(ApiResponse.Fail("评论不存在", 404));

        return Ok(ApiResponse.Ok("已拒绝"));
    }

    /// <summary>
    /// 标记为垃圾评论（POST /api/admin/comments/{id}/spam）
    /// </summary>
    [HttpPost("{id:guid}/spam")]
    public async Task<IActionResult> MarkSpam(Guid id)
    {
        var userId = GetUserId();
        var result = await _commentService.MarkSpamAsync(id, userId);
        if (!result)
            return NotFound(ApiResponse.Fail("评论不存在", 404));

        return Ok(ApiResponse.Ok("已标记为垃圾评论"));
    }

    /// <summary>
    /// 删除评论（DELETE /api/admin/comments/{id}）
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _commentService.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail("评论不存在", 404));

        return Ok(ApiResponse.Ok("已删除"));
    }

    /// <summary>
    /// 批量审核通过（POST /api/admin/comments/bulk-approve）
    /// </summary>
    [HttpPost("bulk-approve")]
    public async Task<IActionResult> BatchApprove([FromBody] BatchCommentRequest request)
    {
        var userId = GetUserId();
        var count = await _commentService.BatchApproveAsync(request.Ids, userId);
        return Ok(ApiResponse.Ok($"成功审核通过 {count} 条评论"));
    }

    /// <summary>
    /// 批量删除（POST /api/admin/comments/bulk-delete）
    /// </summary>
    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BatchDelete([FromBody] BatchCommentRequest request)
    {
        var count = await _commentService.BatchDeleteAsync(request.Ids);
        return Ok(ApiResponse.Ok($"成功删除 {count} 条评论"));
    }

    /// <summary>
    /// 管理员回复评论（POST /api/admin/comments/{id}/reply）
    /// </summary>
    [HttpPost("{id:guid}/reply")]
    public async Task<IActionResult> Reply(Guid id, [FromBody] ReplyCommentRequest request)
    {
        var userId = GetUserId();
        var result = await _commentService.ReplyAsync(id, request.Content, userId);
        if (!result)
            return NotFound(ApiResponse.Fail("评论不存在", 404));

        return Ok(ApiResponse.Ok("回复成功"));
    }

    /// <summary>
    /// 从 JWT Claims 中获取当前用户 ID
    /// </summary>
    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && Guid.TryParse(claim.Value, out var userId))
            return userId;
        throw new UnauthorizedAccessException("无法获取用户身份");
    }
}

/// <summary>
/// 拒绝评论请求
/// </summary>
public class RejectCommentRequest
{
    /// <summary>拒绝原因</summary>
    public string? Reason { get; set; }
}

/// <summary>
/// 管理员回复评论请求
/// </summary>
public class ReplyCommentRequest
{
    /// <summary>回复内容</summary>
    public string Content { get; set; } = string.Empty;
}