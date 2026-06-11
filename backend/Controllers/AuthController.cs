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

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse.Fail("用户名和密码不能为空"));
        }

        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse.Ok(result, "登录成功"));
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