namespace Mblog.API.Models.DTOs.Category;

/// <summary>
/// 创建分类请求 DTO
/// </summary>
public class CreateCategoryRequest
{
    /// <summary>分类名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL 标识，唯一</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>分类描述</summary>
    public string? Description { get; set; }

    /// <summary>封面图媒体 ID</summary>
    public Guid? CoverImageId { get; set; }

    /// <summary>排序序号，越小越靠前</summary>
    public int SortOrder { get; set; }

    /// <summary>父级分类 ID，null 表示顶级分类</summary>
    public Guid? ParentId { get; set; }

    /// <summary>SEO 标题</summary>
    public string? SeoTitle { get; set; }

    /// <summary>SEO 描述</summary>
    public string? SeoDescription { get; set; }

    /// <summary>SEO 关键词</summary>
    public string? SeoKeywords { get; set; }
}

/// <summary>
/// 更新分类请求 DTO
/// </summary>
public class UpdateCategoryRequest
{
    /// <summary>分类名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL 标识，唯一</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>分类描述</summary>
    public string? Description { get; set; }

    /// <summary>封面图媒体 ID</summary>
    public Guid? CoverImageId { get; set; }

    /// <summary>排序序号，越小越靠前</summary>
    public int SortOrder { get; set; }

    /// <summary>父级分类 ID，null 表示顶级分类</summary>
    public Guid? ParentId { get; set; }

    /// <summary>SEO 标题</summary>
    public string? SeoTitle { get; set; }

    /// <summary>SEO 描述</summary>
    public string? SeoDescription { get; set; }

    /// <summary>SEO 关键词</summary>
    public string? SeoKeywords { get; set; }
}

/// <summary>
/// 分类排序请求 DTO
/// </summary>
public class UpdateCategorySortRequest
{
    /// <summary>新的排序序号</summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// 分类响应 DTO
/// </summary>
public class CategoryResponse
{
    /// <summary>分类 ID</summary>
    public Guid Id { get; set; }

    /// <summary>分类名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL 标识</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>分类描述</summary>
    public string? Description { get; set; }

    /// <summary>封面图媒体 ID</summary>
    public Guid? CoverImageId { get; set; }

    /// <summary>封面图 URL</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>排序序号</summary>
    public int SortOrder { get; set; }

    /// <summary>父级分类 ID</summary>
    public Guid? ParentId { get; set; }

    /// <summary>分类层级（0=顶级）</summary>
    public int Level { get; set; }

    /// <summary>子分类列表</summary>
    public List<CategoryListItem> Children { get; set; } = new();

    /// <summary>该分类下的文章数</summary>
    public int ArticleCount { get; set; }

    /// <summary>SEO 标题</summary>
    public string? SeoTitle { get; set; }

    /// <summary>SEO 描述</summary>
    public string? SeoDescription { get; set; }

    /// <summary>SEO 关键词</summary>
    public string? SeoKeywords { get; set; }

    /// <summary>创建时间</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// 分类列表项 DTO（精简）
/// </summary>
public class CategoryListItem
{
    /// <summary>分类 ID</summary>
    public Guid Id { get; set; }

    /// <summary>分类名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL 标识</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>分类描述</summary>
    public string? Description { get; set; }

    /// <summary>排序序号</summary>
    public int SortOrder { get; set; }

    /// <summary>父级分类 ID</summary>
    public Guid? ParentId { get; set; }

    /// <summary>分类层级（0=顶级）</summary>
    public int Level { get; set; }

    /// <summary>文章数</summary>
    public int ArticleCount { get; set; }

    /// <summary>子分类列表</summary>
    public List<CategoryListItem> Children { get; set; } = new();

    /// <summary>创建时间</summary>
    public DateTimeOffset CreatedAt { get; set; }
}