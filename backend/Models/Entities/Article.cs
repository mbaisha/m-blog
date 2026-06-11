namespace Mblog.API.Models.Entities;

/// <summary>
/// 文章表，博客的核心内容实体
/// </summary>
public class Article : BaseEntity, ISoftDeletable
{
    /// <summary>文章标题</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL 标识，唯一</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>文章摘要</summary>
    public string? Summary { get; set; }

    /// <summary>Markdown 正文内容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>封面图媒体 ID</summary>
    public Guid? CoverImageId { get; set; }

    /// <summary>作者用户 ID</summary>
    public Guid AuthorId { get; set; }

    /// <summary>所属分类 ID</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>发布状态：draft / published / archived</summary>
    public string Status { get; set; } = "draft";

    /// <summary>是否置顶</summary>
    public bool IsTop { get; set; }

    /// <summary>是否推荐</summary>
    public bool IsRecommend { get; set; }

    /// <summary>阅读数（防刷）</summary>
    public long ViewCount { get; set; }

    /// <summary>点赞数</summary>
    public long LikeCount { get; set; }

    /// <summary>评论数</summary>
    public long CommentCount { get; set; }

    /// <summary>SEO 标题</summary>
    public string? SeoTitle { get; set; }

    /// <summary>SEO 描述</summary>
    public string? SeoDescription { get; set; }

    /// <summary>SEO 关键词</summary>
    public string? SeoKeywords { get; set; }

    /// <summary>首次发布时间</summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>软删除时间</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>封面图导航属性</summary>
    public Media? CoverImage { get; set; }

    /// <summary>作者导航属性</summary>
    public User Author { get; set; } = null!;

    /// <summary>分类导航属性</summary>
    public Category? Category { get; set; }

    /// <summary>标签关联集合</summary>
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();

    /// <summary>分类关联集合（支持多分类）</summary>
    public ICollection<ArticleCategory> ArticleCategories { get; set; } = new List<ArticleCategory>();

    /// <summary>评论集合</summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}