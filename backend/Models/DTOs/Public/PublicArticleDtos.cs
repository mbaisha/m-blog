namespace Mblog.API.Models.DTOs.Public;

/// <summary>
/// 前台公开文章列表项 DTO
/// </summary>
public class PublicArticleListItem
{
    /// <summary>文章 ID</summary>
    public Guid Id { get; set; }

    /// <summary>文章标题</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL 标识</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>文章摘要</summary>
    public string? Summary { get; set; }

    /// <summary>封面图 URL</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>分类信息</summary>
    public PublicCategoryInfo? Category { get; set; }

    /// <summary>全部分类信息</summary>
    public List<PublicCategoryInfo> Categories { get; set; } = new();

    /// <summary>关联标签</summary>
    public List<PublicTagInfo> Tags { get; set; } = new();

    /// <summary>作者名称</summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>阅读数</summary>
    public long ViewCount { get; set; }

    /// <summary>评论数</summary>
    public long CommentCount { get; set; }

    /// <summary>是否推荐</summary>
    public bool IsRecommend { get; set; }

    /// <summary>是否置顶</summary>
    public bool IsTop { get; set; }

    /// <summary>发布时间</summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// 前台公开文章详情 DTO
/// </summary>
public class PublicArticleDetailResponse
{
    /// <summary>文章 ID</summary>
    public Guid Id { get; set; }

    /// <summary>文章标题</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL 标识</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>文章摘要</summary>
    public string? Summary { get; set; }

    /// <summary>Markdown 正文</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>封面图 URL</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>作者信息</summary>
    public PublicAuthorInfo Author { get; set; } = null!;

    /// <summary>分类信息</summary>
    public PublicCategoryInfo? Category { get; set; }

    /// <summary>全部分类信息</summary>
    public List<PublicCategoryInfo> Categories { get; set; } = new();

    /// <summary>关联标签</summary>
    public List<PublicTagInfo> Tags { get; set; } = new();

    /// <summary>阅读数</summary>
    public long ViewCount { get; set; }

    /// <summary>评论数</summary>
    public long CommentCount { get; set; }

    /// <summary>SEO 标题</summary>
    public string? SeoTitle { get; set; }

    /// <summary>SEO 描述</summary>
    public string? SeoDescription { get; set; }

    /// <summary>SEO 关键词</summary>
    public string? SeoKeywords { get; set; }

    /// <summary>文章状态：draft / published / archived</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>发布时间</summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// 前台公开分类信息
/// </summary>
public class PublicCategoryInfo
{
    /// <summary>分类 ID</summary>
    public Guid Id { get; set; }

    /// <summary>分类名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL 标识</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>分类描述</summary>
    public string? Description { get; set; }

    /// <summary>封面图 URL</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>文章数</summary>
    public int ArticleCount { get; set; }
}

/// <summary>
/// 前台公开标签信息
/// </summary>
public class PublicTagInfo
{
    /// <summary>标签 ID</summary>
    public Guid Id { get; set; }

    /// <summary>标签名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL 标识</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>标签文字颜色</summary>
    public string? Color { get; set; }

    /// <summary>标签背景颜色</summary>
    public string? BgColor { get; set; }

    /// <summary>文章数</summary>
    public int ArticleCount { get; set; }
}

/// <summary>
/// 前台公开作者信息
/// </summary>
public class PublicAuthorInfo
{
    /// <summary>作者 ID</summary>
    public Guid Id { get; set; }

    /// <summary>作者名称</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>头像 URL</summary>
    public string? AvatarUrl { get; set; }
}

/// <summary>
/// 归档项 DTO（按年月分组）
/// </summary>
public class ArchiveItem
{
    /// <summary>年份</summary>
    public int Year { get; set; }

    /// <summary>月份</summary>
    public int Month { get; set; }

    /// <summary>该月文章数</summary>
    public int Count { get; set; }

    /// <summary>该月文章列表</summary>
    public List<ArchiveArticleItem> Articles { get; set; } = new();
}

/// <summary>
/// 归档文章项
/// </summary>
public class ArchiveArticleItem
{
    /// <summary>文章 ID</summary>
    public Guid Id { get; set; }

    /// <summary>文章标题</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL 标识</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>文章摘要</summary>
    public string? Summary { get; set; }

    /// <summary>发布时间</summary>
    public DateTimeOffset PublishedAt { get; set; }
}