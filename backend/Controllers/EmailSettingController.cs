using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 邮件设置管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/email-settings")]
[Authorize(Roles = "super_admin,admin")]
public class EmailSettingController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailSettingController(IEmailService emailService) => _emailService = emailService;

    /// <summary>获取邮件设置</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var setting = await _emailService.GetSettingAsync();
        if (setting == null) return Ok(ApiResponse.Ok(new { }));
        return Ok(ApiResponse.Ok(setting));
    }

    /// <summary>保存邮件设置</summary>
    [HttpPut]
    public async Task<IActionResult> Save([FromBody] UpdateEmailSettingRequest request)
    {
        var setting = await _emailService.SaveSettingAsync(request);
        return Ok(ApiResponse.Ok(setting, "邮件设置已保存"));
    }

    /// <summary>测试邮件发送</summary>
    [HttpPost("test")]
    public async Task<IActionResult> TestSend([FromBody] TestEmailRequest request)
    {
        var success = await _emailService.TestSendAsync(request.TestEmail);
        if (success)
            return Ok(ApiResponse.Ok<object>(null!, "测试邮件发送成功"));
        return StatusCode(500, ApiResponse.Fail("测试邮件发送失败，请检查配置"));
    }
}

/// <summary>
/// 邮件模板管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/email-templates")]
[Authorize(Roles = "super_admin,admin")]
public class EmailTemplateController : ControllerBase
{
    private readonly IEmailTemplateService _templateService;

    public EmailTemplateController(IEmailTemplateService templateService) => _templateService = templateService;

    /// <summary>获取所有模板</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var templates = await _templateService.GetAllAsync();
        return Ok(ApiResponse.Ok(templates));
    }

    /// <summary>获取单个模板</summary>
    [HttpGet("{templateKey}")]
    public async Task<IActionResult> GetByKey(string templateKey)
    {
        var template = await _templateService.GetByKeyAsync(templateKey);
        if (template == null)
            return NotFound(ApiResponse.Fail("未找到该模板"));
        return Ok(ApiResponse.Ok(template));
    }

    /// <summary>更新模板</summary>
    [HttpPut("{templateKey}")]
    public async Task<IActionResult> Update(string templateKey, [FromBody] UpdateEmailTemplateRequest request)
    {
        var template = await _templateService.UpdateAsync(templateKey, request);
        return Ok(ApiResponse.Ok(template, "模板已更新"));
    }

    /// <summary>重置模板到默认值</summary>
    [HttpPost("{templateKey}/reset")]
    public async Task<IActionResult> Reset(string templateKey)
    {
        try
        {
            var template = await _templateService.ResetAsync(templateKey);
            return Ok(ApiResponse.Ok(template, "模板已重置"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}