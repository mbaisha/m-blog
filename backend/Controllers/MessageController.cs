using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

[ApiController]
[Route("api/messages")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ICaptchaService _captchaService;

    public MessageController(IMessageService messageService, ICaptchaService captchaService)
    {
        _messageService = messageService;
        _captchaService = captchaService;
    }

    /// <summary>提交留言（需验证码，提交后等待审核）</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<MessageResponse>>> Create([FromBody] CreateMessageRequest request)
    {
        var captchaResult = await _captchaService.VerifyAsync(request.CaptchaSessionId, request.CaptchaAnswer, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");
        if (!captchaResult.Valid)
            return BadRequest(ApiResponse<MessageResponse>.Fail(captchaResult.ErrorMessage ?? "验证码错误"));

        if (string.IsNullOrWhiteSpace(request.Nickname))
            return BadRequest(ApiResponse<MessageResponse>.Fail("昵称不能为空"));

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(ApiResponse<MessageResponse>.Fail("留言内容不能为空"));

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _messageService.CreateAsync(request, ipAddress, userAgent);

        return Ok(ApiResponse<MessageResponse>.Ok(result, "留言已提交，等待审核"));
    }

    /// <summary>获取已审核的留言列表（树形，仅前端使用）</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<MessageResponse>>>> GetApproved(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _messageService.GetApprovedAsync(page, pageSize);
        return Ok(ApiResponse<PagedResponse<MessageResponse>>.Ok(result));
    }
}

/// <summary>后台留言管理 API</summary>
[ApiController]
[Route("api/admin/messages")]
public class AdminMessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public AdminMessageController(IMessageService messageService) => _messageService = messageService;

    /// <summary>获取留言列表（后台扁平列表，含子回复）</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<MessageFlatResponse>>>> GetList(
        [FromQuery] AdminMessageQueryRequest query)
    {
        var result = await _messageService.GetAdminListAsync(query);
        return Ok(ApiResponse<PagedResponse<MessageFlatResponse>>.Ok(result));
    }

    /// <summary>获取留言详情（树形，含所有子回复，用于详情弹窗）</summary>
    [HttpGet("{id}/detail")]
    public async Task<ActionResult<ApiResponse<MessageResponse>>> GetDetail(Guid id)
    {
        try
        {
            var result = await _messageService.GetAdminDetailAsync(id);
            return Ok(ApiResponse<MessageResponse>.Ok(result));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<MessageResponse>.Fail("留言不存在"));
        }
    }

    /// <summary>审核/回复留言</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<MessageResponse>>> Update(
        Guid id, [FromBody] AdminUpdateMessageRequest request)
    {
        try
        {
            var result = await _messageService.AdminUpdateAsync(id, request);
            return Ok(ApiResponse<MessageResponse>.Ok(result));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<MessageResponse>.Fail("留言不存在"));
        }
    }

    /// <summary>删除留言</summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var ok = await _messageService.AdminDeleteAsync(id);
        if (!ok)
            return NotFound(ApiResponse<object>.Fail("留言不存在"));

        return Ok(ApiResponse<object>.Ok(null!));
    }
}