namespace Mblog.API.Models.Entities;

/// <summary>
/// 评论表，支持楼中楼嵌套回复
/// </summary>
public class Comment : BaseEntity, ISoftDeletable
{
    /// <summary>所属文章 ID</summary>
    public Guid ArticleId { get; set; }

    /// <summary>父评论 ID，为空表示顶级评论</summary>
    public Guid? ParentId { get; set; }

    /// <summary>评论者昵称</summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>评论者邮箱</summary>
    public string? Email { get; set; }

    /// <summary>评论者网站</summary>
    public string? Website { get; set; }

    /// <summary>评论正文内容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>评论者头像 URL</summary>
    public string? Avatar { get; set; }

    /// <summary>评论者 IP 哈希值（隐私保护）</summary>
    public string IpHash { get; set; } = string.Empty;

    /// <summary>评论者真实 IP 地址（仅管理员可见）</summary>
    public string? IpAddress { get; set; }

    /// <summary>评论者地理位置（精确到城市）</summary>
    public string? Location { get; set; }

    /// <summary>审核状态：pending / approved / rejected / hidden / spam</summary>
    public string Status { get; set; } = "pending";

    /// <summary>是否为垃圾评论</summary>
    public bool IsSpam { get; set; }

    /// <summary>审核人用户 ID</summary>
    public Guid? ReviewedBy { get; set; }

    /// <summary>审核时间</summary>
    public DateTimeOffset? ReviewedAt { get; set; }

    /// <summary>软删除时间</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>所属文章导航属性</summary>
    public Article Article { get; set; } = null!;

    /// <summary>父评论导航属性</summary>
    public Comment? Parent { get; set; }

    /// <summary>审核人导航属性</summary>
    public User? Reviewer { get; set; }

    /// <summary>子评论集合</summary>
    public ICollection<Comment> Children { get; set; } = new List<Comment>();
}