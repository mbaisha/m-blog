using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Mblog.API.Common;
using Mblog.API.Models.DTOs.Comment;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 验证码控制器
/// </summary>
[ApiController]
[Route("api/captcha")]
[EnableRateLimiting("CaptchaPerMinute")]
public class CaptchaController : ControllerBase
{
    private readonly ICaptchaService _captchaService;

    public CaptchaController(ICaptchaService captchaService)
    {
        _captchaService = captchaService;
    }

    /// <summary>
    /// 获取滑块验证码（GET /api/captcha/slider）
    /// 返回目标百分比位置，前端滑块需拖到该位置
    /// </summary>
    [HttpGet("slider")]
    public async Task<IActionResult> GetSlider()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _captchaService.GenerateSliderAsync(ip);
        return Ok(ApiResponse.Ok(new
        {
            sessionId = result.SessionId,
            targetPercent = result.TargetPercent,
        }));
    }

    /// <summary>
    /// 校验滑块验证码（POST /api/captcha/verify-slider）
    /// </summary>
    [HttpPost("verify-slider")]
    public async Task<IActionResult> VerifySlider([FromBody] SliderVerifyRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _captchaService.VerifySliderAsync(request.SessionId, request.Percent, ip);
        return Ok(ApiResponse.Ok(new { valid = result.Valid }, result.Valid ? "验证通过" : result.ErrorMessage ?? "验证失败"));
    }

    /// <summary>
    /// 获取验证码图片（GET /api/captcha/image）
    /// </summary>
    [HttpGet("image")]
    public async Task<IActionResult> GetImage()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _captchaService.GenerateAsync(ip);
        return Ok(ApiResponse.Ok(new CaptchaImageResponse
        {
            SessionId = result.SessionId,
            ImageBase64 = result.ImageBase64,
        }));
    }

    /// <summary>
    /// 校验验证码（POST /api/captcha/verify）
    /// </summary>
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] CaptchaVerifyRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _captchaService.VerifyAsync(request.SessionId, request.Answer, ip);
        return Ok(ApiResponse.Ok(new { valid = result.Valid }, result.Valid ? "验证通过" : result.ErrorMessage ?? "验证失败"));
    }
}