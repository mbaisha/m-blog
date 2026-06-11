using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 模块布局配置控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/layout")]
[Authorize]
public class LayoutController : ControllerBase
{
    private readonly ILayoutService _layoutService;

    public LayoutController(ILayoutService layoutService) => _layoutService = layoutService;

    [HttpGet("{pageKey}")]
    public async Task<IActionResult> GetByPageKey(string pageKey)
    {
        var modules = await _layoutService.GetByPageKeyAsync(pageKey);
        return Ok(ApiResponse.Ok(modules));
    }

    [HttpPut]
    public async Task<IActionResult> BatchSave([FromBody] BatchSaveLayoutRequest request)
    {
        var modules = await _layoutService.BatchSaveAsync(request);
        return Ok(ApiResponse.Ok(modules, "布局配置已更新"));
    }

    [HttpDelete("{pageKey}")]
    public async Task<IActionResult> Reset(string pageKey)
    {
        var modules = await _layoutService.ResetAsync(pageKey);
        return Ok(ApiResponse.Ok(modules, "布局已恢复默认"));
    }

    /// <summary>
    /// 批量添加默认模块
    /// POST /api/admin/layout/defaults?pageKey=home
    /// </summary>
    [HttpPost("defaults")]
    public async Task<IActionResult> AddDefaults([FromQuery] string pageKey)
    {
        var modules = await _layoutService.AddDefaultsAsync(pageKey);
        return Ok(ApiResponse.Ok(modules, "默认模块已添加"));
    }
}

/// <summary>
/// 模块布局公开控制器（前台）
/// </summary>
[ApiController]
[Route("api/layout")]
public class PublicLayoutController : ControllerBase
{
    private readonly ILayoutService _layoutService;

    public PublicLayoutController(ILayoutService layoutService) => _layoutService = layoutService;

    [HttpGet("{pageKey}")]
    public async Task<IActionResult> GetByPageKey(string pageKey)
    {
        var modules = await _layoutService.GetByPageKeyAsync(pageKey);
        return Ok(ApiResponse.Ok(modules));
    }
}