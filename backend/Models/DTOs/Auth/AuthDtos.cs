namespace Mblog.API.Models.DTOs.Auth;

/// <summary>
/// 登录请求 DTO
/// </summary>
public class LoginRequest
{
    /// <summary>用户名</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>密码（明文）</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>验证码会话 ID（可选）</summary>
    public string? CaptchaSessionId { get; set; }

    /// <summary>验证码答案（可选）</summary>
    public string? CaptchaAnswer { get; set; }
}

/// <summary>
/// 登录响应 DTO
/// </summary>
public class LoginResponse
{
    /// <summary>JWT 访问令牌</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>刷新令牌</summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>访问令牌过期时间（UTC）</summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>用户信息</summary>
    public UserInfo User { get; set; } = null!;
}

/// <summary>
/// 用户简要信息（登录后返回）
/// </summary>
public class UserInfo
{
    /// <summary>用户 ID</summary>
    public Guid Id { get; set; }

    /// <summary>用户名</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>邮箱</summary>
    public string? Email { get; set; }

    /// <summary>角色</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>头像 URL</summary>
    public string? AvatarUrl { get; set; }
}

/// <summary>
/// 刷新令牌请求 DTO
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>刷新令牌字符串</summary>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// 修改密码请求 DTO
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>当前密码</summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>新密码</summary>
    public string NewPassword { get; set; } = string.Empty;
}