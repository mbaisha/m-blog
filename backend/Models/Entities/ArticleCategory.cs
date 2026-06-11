namespace Mblog.API.Models.Entities;

/// <summary>
/// 文章-分类多对多关联表，支持一篇文章对应多个分类
/// </summary>
public class ArticleCategory
{
    /// <summary>主键</summary>
    public Guid Id { get; set; }

    /// <summary>文章 ID</summary>
    public Guid ArticleId { get; set; }

    /// <summary>分类 ID</summary>
    public Guid CategoryId { get; set; }

    /// <summary>关联创建时间</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>关联文章导航属性</summary>
    public Article Article { get; set; } = null!;

    /// <summary>关联分类导航属性</summary>
    public Category Category { get; set; } = null!;
}