using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 文生图管理控制器（仅管理员和超级管理员可访问）
/// </summary>
[ApiController]
[Route("api/admin/image-gen")]
[Authorize(Roles = "super_admin,admin")]
public class ImageGenController : ControllerBase
{
    private readonly IImageGenService _imageGenService;

    public ImageGenController(IImageGenService imageGenService) => _imageGenService = imageGenService;

    /// <summary>获取文生图配置</summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetConfig()
    {
        var config = await _imageGenService.GetConfigAsync();
        if (config == null) return Ok(ApiResponse.Ok(new { }));
        return Ok(ApiResponse.Ok(new
        {
            config.BaseUrl,
            config.ApiKey,
            config.Model,
            config.AvailableModels,
            config.DefaultSize,
            config.AvailableSizes,
            config.DefaultQuality,
            config.DefaultN,
            config.TimeoutSeconds
        }));
    }

    /// <summary>保存文生图配置</summary>
    [HttpPut("config")]
    public async Task<IActionResult> SaveConfig([FromBody] SaveImageGenConfigRequest request)
    {
        var config = await _imageGenService.SaveConfigAsync(request);
        return Ok(ApiResponse.Ok("文生图配置已保存"));
    }

    /// <summary>执行文生图</summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] ImageGenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(ApiResponse.Fail("提示词不能为空"));

        var urls = await _imageGenService.GenerateAsync(request);
        return Ok(ApiResponse.Ok(new { images = urls }));
    }
}