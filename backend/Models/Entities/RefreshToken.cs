namespace Mblog.API.Models.Entities;

/// <summary>
/// Refresh Token 表，用于 JWT 无感刷新和登录态管理
/// </summary>
public class RefreshToken
{
    /// <summary>主键</summary>
    public Guid Id { get; set; }

    /// <summary>所属用户 ID</summary>
    public Guid UserId { get; set; }

    /// <summary>Token 哈希值（仅存哈希，不存原文）</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>过期时间</summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>撤销时间，为空表示未撤销</summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>创建时的客户端 IP</summary>
    public string? CreatedIp { get; set; }

    /// <summary>创建时的 User-Agent</summary>
    public string? CreatedUserAgent { get; set; }

    /// <summary>创建时间</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>所属用户导航属性</summary>
    public User User { get; set; } = null!;
}