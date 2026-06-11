namespace Mblog.API.Models.Entities;

/// <summary>
/// 文章-标签多对多关联表
/// </summary>
public class ArticleTag
{
    /// <summary>主键</summary>
    public Guid Id { get; set; }

    /// <summary>文章 ID</summary>
    public Guid ArticleId { get; set; }

    /// <summary>标签 ID</summary>
    public Guid TagId { get; set; }

    /// <summary>关联创建时间</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>关联文章导航属性</summary>
    public Article Article { get; set; } = null!;

    /// <summary>关联标签导航属性</summary>
    public Tag Tag { get; set; } = null!;
}