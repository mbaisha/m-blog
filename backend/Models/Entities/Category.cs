namespace Mblog.API.Models.Entities;

/// <summary>
/// 文章分类表，支持三级分类（通过 ParentId 递归）
/// </summary>
public class Category : BaseEntity, ISoftDeletable
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

    /// <summary>父级分类 ID，支持子分类</summary>
    public Guid? ParentId { get; set; }

    /// <summary>分类层级（0=顶级，1=二级，2=三级）</summary>
    public int Level { get; set; }

    /// <summary>SEO 标题</summary>
    public string? SeoTitle { get; set; }

    /// <summary>SEO 描述</summary>
    public string? SeoDescription { get; set; }

    /// <summary>SEO 关键词</summary>
    public string? SeoKeywords { get; set; }

    /// <summary>软删除时间</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>封面图导航属性</summary>
    public Media? CoverImage { get; set; }

    /// <summary>父级分类导航属性</summary>
    public Category? Parent { get; set; }

    /// <summary>子分类集合</summary>
    public ICollection<Category> Children { get; set; } = new List<Category>();

    /// <summary>该分类下的文章集合（主分类）</summary>
    public ICollection<Article> Articles { get; set; } = new List<Article>();

    /// <summary>文章多分类关联集合</summary>
    public ICollection<ArticleCategory> ArticleCategories { get; set; } = new List<ArticleCategory>();
}