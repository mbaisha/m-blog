using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs.Auth;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 认证控制器，处理登录、刷新令牌、退出、获取当前用户
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILoginAttemptService? _loginAttemptService;
    private readonly ISiteSettingService? _siteSettingService;

    public AuthController(
        IAuthService authService,
        ILoginAttemptService? loginAttemptService = null,
        ISiteSettingService? siteSettingService = null)
    {
        _authService = authService;
        _loginAttemptService = loginAttemptService;
        _siteSettingService = siteSettingService;
    }

    /// <summary>
    /// 用户登录（支持验证码）
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse.Fail("用户名和密码不能为空"));
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // 检查是否被锁定
        if (_loginAttemptService?.IsLocked(ip) == true)
        {
            return StatusCode(429, ApiResponse.Fail("登录尝试次数过多，请 15 分钟后再试"));
        }

        // 如果之前有失败记录且开启了验证码功能，要求验证码
        var captchaEnabled = true;
        if (_siteSettingService != null)
        {
            var siteSetting = await _siteSettingService.GetAsync();
            if (siteSetting != null)
                captchaEnabled = siteSetting.LoginCaptchaEnabled;
        }
        var needCaptcha = captchaEnabled && _loginAttemptService?.GetFailedCount(ip) >= 1;
        if (needCaptcha && string.IsNullOrWhiteSpace(request.CaptchaSessionId))
        {
            return Unauthorized(ApiResponse.Fail("请输入验证码"));
        }

        try
        {
            var result = await _authService.LoginAsync(request, request.CaptchaSessionId, request.CaptchaAnswer, ip);
            _loginAttemptService?.RecordSuccess(ip);
            return Ok(ApiResponse.Ok(result, "登录成功"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _loginAttemptService?.RecordFailure(ip);
            return Unauthorized(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 刷新访问令牌
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(ApiResponse.Fail("刷新令牌不能为空"));
        }

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(ApiResponse.Ok(result, "令牌刷新成功"));
    }

    /// <summary>
    /// 退出登录
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            await _authService.LogoutAsync(request.RefreshToken);
        }
        return Ok(ApiResponse.Ok("退出成功"));
    }

    /// <summary>
    /// 获取当前管理员信息
    /// GET /api/auth/me
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var user = await _authService.GetCurrentUserAsync(userId);
        return Ok(ApiResponse.Ok(user));
    }

    /// <summary>
    /// 修改当前管理员密码
    /// POST /api/auth/change-password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return BadRequest(ApiResponse.Fail("当前密码不能为空"));

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return BadRequest(ApiResponse.Fail("新密码长度不能少于 6 位"));

        try
        {
            var userId = GetCurrentUserId();
            await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            return Ok(ApiResponse.Ok("密码修改成功"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 从 JWT Claims 中获取当前用户 ID
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("无法获取当前用户信息");

        return userId;
    }
}