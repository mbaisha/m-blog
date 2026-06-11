using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 主题配置控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/theme")]
[Authorize]
public class ThemeController : ControllerBase
{
    private readonly IThemeService _themeService;

    public ThemeController(IThemeService themeService) => _themeService = themeService;

    /// <summary>获取所有主题</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var themes = await _themeService.GetAllAsync();
        return Ok(ApiResponse.Ok(themes));
    }

    /// <summary>获取当前激活主题</summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var theme = await _themeService.GetActiveAsync();
        if (theme == null) return Ok(ApiResponse.Ok(new { }));
        return Ok(ApiResponse.Ok(theme));
    }

    /// <summary>保存指定主题</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Save(Guid id, [FromBody] UpdateThemeSettingRequest request)
    {
        try
        {
            var theme = await _themeService.SaveAsync(id, request);
            return Ok(ApiResponse.Ok(theme, "主题配置已更新"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message, 404));
        }
    }

    /// <summary>创建新主题</summary>
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromQuery] string name = "新主题")
    {
        var theme = await _themeService.CreateAsync(name);
        return Ok(ApiResponse.Ok(theme, "主题已创建"));
    }

    /// <summary>删除主题</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _themeService.DeleteAsync(id);
        if (!result) return BadRequest(ApiResponse.Fail("删除失败：无法删除最后一个主题", 400));
        return Ok(ApiResponse.Ok("主题已删除"));
    }

    /// <summary>复制主题</summary>
    [HttpPost("{id:guid}/duplicate")]
    public async Task<IActionResult> Duplicate(Guid id, [FromQuery] string name = "")
    {
        try
        {
            var theme = await _themeService.DuplicateAsync(id, name);
            return Ok(ApiResponse.Ok(theme, "主题已复制"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message, 404));
        }
    }

    /// <summary>当前激活主题恢复默认</summary>
    [HttpPost("reset")]
    public async Task<IActionResult> Reset()
    {
        var theme = await _themeService.ResetAsync();
        return Ok(ApiResponse.Ok(theme, "主题已恢复默认"));
    }

    /// <summary>激活指定主题</summary>
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _themeService.ActivateAsync(id);
        if (!result) return NotFound(ApiResponse.Fail("主题不存在", 404));
        return Ok(ApiResponse.Ok("主题已激活"));
    }
}

/// <summary>
/// 主题配置公开控制器（前台）
/// </summary>
[ApiController]
[Route("api/theme")]
public class PublicThemeController : ControllerBase
{
    private readonly IThemeService _themeService;

    public PublicThemeController(IThemeService themeService) => _themeService = themeService;

    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var theme = await _themeService.GetActiveAsync();
        if (theme == null) return Ok(ApiResponse.Ok(new { }));
        return Ok(ApiResponse.Ok(theme));
    }
}