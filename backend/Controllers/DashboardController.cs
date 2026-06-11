using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 仪表盘统计控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IVisitService _visitService;

    public DashboardController(IDashboardService dashboardService, IVisitService visitService)
    {
        _dashboardService = dashboardService;
        _visitService = visitService;
    }

    /// <summary>
    /// 获取仪表盘概览统计
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _dashboardService.GetStatsAsync();
        return Ok(ApiResponse.Ok(stats));
    }
}