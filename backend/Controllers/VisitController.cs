using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 访问统计公开控制器（前台上报 + 阅读量展示）
/// </summary>
[ApiController]
[Route("api/visits")]
public class PublicVisitController : ControllerBase
{
    private readonly IVisitService _visitService;

    public PublicVisitController(IVisitService visitService) => _visitService = visitService;

    /// <summary>
    /// 8.1 访问上报接口（无需登录）
    /// </summary>
    [HttpPost("track")]
    public async Task<IActionResult> Track([FromBody] TrackVisitRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        var xForwardedFor = Request.Headers["X-Forwarded-For"].ToString();
        var xRealIp = Request.Headers["X-Real-IP"].ToString();
        await _visitService.TrackAsync(request, ip, userAgent, xForwardedFor, xRealIp);
        return Ok(ApiResponse.Ok("OK"));
    }

    /// <summary>
    /// 获取阅读数（公开）
    /// GET /api/visits/count?pageType=article&pageId=uuid
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetCount([FromQuery] string pageType, [FromQuery] Guid? pageId)
    {
        var count = await _visitService.GetViewCountAsync(pageType, pageId);
        return Ok(ApiResponse.Ok(new { viewCount = count }));
    }
}

/// <summary>
/// 访问统计管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/visits")]
[Authorize]
public class VisitController : ControllerBase
{
    private readonly IVisitService _visitService;

    public VisitController(IVisitService visitService) => _visitService = visitService;

    /// <summary>
    /// 8.3 分页查询访客记录
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] VisitQueryParams query)
    {
        var result = await _visitService.GetPagedAsync(query);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 8.4 访问统计聚合数据
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _visitService.GetStatsAsync();
        return Ok(ApiResponse.Ok(stats));
    }

    /// <summary>
    /// 8.6 手动清理访问记录
    /// </summary>
    [HttpDelete("cleanup")]
    public async Task<IActionResult> Cleanup([FromBody] CleanupRequest request)
    {
        var result = await _visitService.CleanupAsync(request.RetentionDays);
        return Ok(ApiResponse.Ok(result, $"已清理 {result.DeletedCount} 条记录"));
    }
}