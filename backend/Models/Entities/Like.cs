namespace Mblog.API.Models.Entities;

/// <summary>
/// 点赞记录表，记录文章点赞（支持登录用户和匿名访客）
/// </summary>
public class Like
{
    /// <summary>主键</summary>
    public Guid Id { get; set; }

    /// <summary>被点赞文章 ID</summary>
    public Guid ArticleId { get; set; }

    /// <summary>点赞用户 ID（登录用户）</summary>
    public Guid? UserId { get; set; }

    /// <summary>点赞者 IP 哈希（匿名访客）</summary>
    public string? IpHash { get; set; }

    /// <summary>创建时间</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>创建时的客户端 IP</summary>
    public string? CreatedIp { get; set; }

    /// <summary>被点赞文章导航属性</summary>
    public Article Article { get; set; } = null!;

    /// <summary>点赞用户导航属性</summary>
    public User? User { get; set; }
}