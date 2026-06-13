namespace Mblog.API.Models.DTOs.Article;

/// <summary>
/// 文章查询参数
/// </summary>
public class ArticleQueryParams
{
    /// <summary>当前页码，从 1 开始</summary>
    public int Page { get; set; } = 1;

    /// <summary>每页条数</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>搜索关键词（标题/摘要模糊匹配）</summary>
    public string? Keyword { get; set; }

    /// <summary>发布状态筛选：draft / published / archived</summary>
    public string? Status { get; set; }

    /// <summary>分类 ID 筛选</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>标签 ID 筛选</summary>
    public Guid? TagId { get; set; }

    /// <summary>是否仅置顶</summary>
    public bool? IsTop { get; set; }

    /// <summary>是否仅推荐</summary>
    public bool? IsRecommend { get; set; }

    /// <summary>是否显示已删除（回收站）</summary>
    public bool? ShowDeleted { get; set; }

    /// <summary>排序字段：createdAt / publishedAt / viewCount / title</summary>
    public string SortBy { get; set; } = "createdAt";

    /// <summary>排序方向：asc / desc</summary>
    public string SortOrder { get; set; } = "desc";
}

/// <summary>
/// 创建文章请求 DTO
/// </summary>
public class CreateArticleRequest
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

    /// <summary>所属分类 ID 列表（多分类）</summary>
    public List<Guid> CategoryIds { get; set; } = new();

    /// <summary>关联标签 ID 列表</summary>
    public List<Guid> TagIds { get; set; } = new();

    /// <summary>发布状态：draft / published</summary>
    public string Status { get; set; } = "draft";

    /// <summary>是否置顶</summary>
    public bool IsTop { get; set; }

    /// <summary>是否推荐</summary>
    public bool IsRecommend { get; set; }

    /// <summary>SEO 标题</summary>
    public string? SeoTitle { get; set; }

    /// <summary>SEO 描述</summary>
    public string? SeoDescription { get; set; }

    /// <summary>SEO 关键词</summary>
    public string? SeoKeywords { get; set; }

    /// <summary>指定发布时间（UTC），为空则使用当前时间。用于数据迁移</summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>指定创建时间（UTC），为空则使用数据库默认值。用于数据迁移</summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>指定更新时间（UTC），为空则使用数据库默认值。用于数据迁移</summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// 更新文章请求 DTO
/// </summary>
public class UpdateArticleRequest
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

    /// <summary>所属分类 ID 列表（多分类）</summary>
    public List<Guid> CategoryIds { get; set; } = new();

    /// <summary>关联标签 ID 列表</summary>
    public List<Guid> TagIds { get; set; } = new();

    /// <summary>发布状态：draft / published / archived</summary>
    public string Status { get; set; } = "draft";

    /// <summary>是否置顶</summary>
    public bool IsTop { get; set; }

    /// <summary>是否推荐</summary>
    public bool IsRecommend { get; set; }

    /// <summary>SEO 标题</summary>
    public string? SeoTitle { get; set; }

    /// <summary>SEO 描述</summary>
    public string? SeoDescription { get; set; }

    /// <summary>SEO 关键词</summary>
    public string? SeoKeywords { get; set; }
}

/// <summary>
/// 更新文章状态请求 DTO
/// </summary>
public class UpdateArticleStatusRequest
{
    /// <summary>目标状态：draft / published / archived</summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// 文章列表项 DTO
/// </summary>
public class ArticleListItem
{
    /// <summary>文章 ID</summary>
    public Guid Id { get; set; }

    /// <summary>文章标题</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL 标识</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>文章摘要</summary>
    public string? Summary { get; set; }

    /// <summary>发布状态</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>是否置顶</summary>
    public bool IsTop { get; set; }

    /// <summary>是否推荐</summary>
    public bool IsRecommend { get; set; }

    /// <summary>阅读数</summary>
    public long ViewCount { get; set; }

    /// <summary>点赞数</summary>
    public long LikeCount { get; set; }

    /// <summary>评论数</summary>
    public long CommentCount { get; set; }

    /// <summary>分类名称（主分类）</summary>
    public string? CategoryName { get; set; }

    /// <summary>全部分类名称列表</summary>
    public List<string> CategoryNames { get; set; } = new();

    /// <summary>作者名称</summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>关联标签</summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>首次发布时间</summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// 文章详情 DTO
/// </summary>
public class ArticleDetailResponse
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

    /// <summary>封面图媒体 ID</summary>
    public Guid? CoverImageId { get; set; }

    /// <summary>封面图 URL</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>作者信息</summary>
    public ArticleAuthorInfo Author { get; set; } = null!;

    /// <summary>分类信息（主分类）</summary>
    public ArticleCategoryInfo? Category { get; set; }

    /// <summary>全部分类信息</summary>
    public List<ArticleCategoryInfo> Categories { get; set; } = new();

    /// <summary>关联标签</summary>
    public List<ArticleTagInfo> Tags { get; set; } = new();

    /// <summary>发布状态</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>是否置顶</summary>
    public bool IsTop { get; set; }

    /// <summary>是否推荐</summary>
    public bool IsRecommend { get; set; }

    /// <summary>阅读数</summary>
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

    /// <summary>创建时间</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// 文章作者信息
/// </summary>
public class ArticleAuthorInfo
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

/// <summary>
/// 文章分类信息
/// </summary>
public class ArticleCategoryInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

/// <summary>
/// 文章标签信息
/// </summary>
public class ArticleTagInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}

/// <summary>
/// 前台公开文章查询参数
/// </summary>
public class PublicArticleQueryParams
{
    /// <summary>当前页码，从 1 开始</summary>
    public int Page { get; set; } = 1;

    /// <summary>每页条数</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>搜索关键词</summary>
    public string? Keyword { get; set; }

    /// <summary>分类 ID</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>分类 Slug</summary>
    public string? CategorySlug { get; set; }

    /// <summary>标签 ID</summary>
    public Guid? TagId { get; set; }

    /// <summary>标签 Slug</summary>
    public string? TagSlug { get; set; }

    /// <summary>排序方式：latest / views / comments</summary>
    public string SortBy { get; set; } = "latest";
}