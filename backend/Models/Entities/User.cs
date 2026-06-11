namespace Mblog.API.Models.Entities;

/// <summary>
/// 后台管理员/用户表
/// </summary>
public class User : BaseEntity, ISoftDeletable
{
    /// <summary>登录用户名，唯一</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>邮箱，唯一</summary>
    public string? Email { get; set; }

    /// <summary>密码哈希值（BCrypt）</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>角色：super_admin / admin / content_admin / comment_moderator</summary>
    public string Role { get; set; } = "admin";

    /// <summary>头像媒体 ID</summary>
    public Guid? AvatarMediaId { get; set; }

    /// <summary>账号状态：active / disabled / deleted</summary>
    public string Status { get; set; } = "active";

    /// <summary>最后登录时间</summary>
    public DateTimeOffset? LastLoginAt { get; set; }

    /// <summary>软删除时间</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>头像导航属性</summary>
    public Media? Avatar { get; set; }

    /// <summary>刷新令牌集合</summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>文章集合</summary>
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}