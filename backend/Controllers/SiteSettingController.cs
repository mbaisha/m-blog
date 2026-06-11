using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 站点设置管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/settings")]
[Authorize]
public class SiteSettingController : ControllerBase
{
    private readonly ISiteSettingService _siteSettingService;

    public SiteSettingController(ISiteSettingService siteSettingService) => _siteSettingService = siteSettingService;

    /// <summary>获取站点设置</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var setting = await _siteSettingService.GetAsync();
        if (setting == null) return Ok(ApiResponse.Ok(new { }));
        return Ok(ApiResponse.Ok(setting));
    }

    /// <summary>更新站点设置</summary>
    [HttpPut]
    public async Task<IActionResult> Save([FromBody] UpdateSiteSettingRequest request)
    {
        var setting = await _siteSettingService.SaveAsync(request);
        return Ok(ApiResponse.Ok(setting, "站点设置已更新"));
    }
}

/// <summary>
/// 站点设置公开控制器（前台）
/// </summary>
[ApiController]
[Route("api/settings")]
public class PublicSiteSettingController : ControllerBase
{
    private readonly ISiteSettingService _siteSettingService;

    public PublicSiteSettingController(ISiteSettingService siteSettingService) => _siteSettingService = siteSettingService;

    /// <summary>获取公开站点信息</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var setting = await _siteSettingService.GetPublicAsync();
        if (setting == null) return Ok(ApiResponse.Ok(new { }));
        return Ok(ApiResponse.Ok(setting));
    }
}