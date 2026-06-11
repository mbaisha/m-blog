using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 页脚配置控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/footer")]
[Authorize]
public class FooterController : ControllerBase
{
    private readonly IFooterService _footerService;

    public FooterController(IFooterService footerService) => _footerService = footerService;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var config = await _footerService.GetAsync();
        if (config == null) return Ok(ApiResponse.Ok(new { }));
        return Ok(ApiResponse.Ok(config));
    }

    [HttpPut]
    public async Task<IActionResult> Save([FromBody] UpdateFooterConfigRequest request)
    {
        var config = await _footerService.SaveAsync(request);
        return Ok(ApiResponse.Ok(config, "页脚配置已更新"));
    }
}

/// <summary>
/// 页脚配置公开控制器（前台）
/// </summary>
[ApiController]
[Route("api/footer")]
public class PublicFooterController : ControllerBase
{
    private readonly IFooterService _footerService;

    public PublicFooterController(IFooterService footerService) => _footerService = footerService;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var config = await _footerService.GetAsync();
        if (config == null) return Ok(ApiResponse.Ok(new { }));
        return Ok(ApiResponse.Ok(config));
    }
}