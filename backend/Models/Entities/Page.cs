namespace Mblog.API.Models.Entities;

/// <summary>
/// 自定义页面表，支持后台创建和管理独立页面
/// </summary>
public class Page : BaseEntity
{
    /// <summary>页面标题</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL 标识，唯一</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>页面内容（Markdown）</summary>
    public string? Content { get; set; }

    /// <summary>页面摘要</summary>
    public string? Summary { get; set; }

    /// <summary>发布状态：draft / published</summary>
    public string Status { get; set; } = "draft";

    /// <summary>是否对外可见</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>排序序号，越小越靠前</summary>
    public int SortOrder { get; set; }

    /// <summary>SEO 标题</summary>
    public string? SeoTitle { get; set; }

    /// <summary>SEO 描述</summary>
    public string? SeoDescription { get; set; }

    /// <summary>SEO 关键词</summary>
    public string? SeoKeywords { get; set; }

    /// <summary>首次发布时间</summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>是否启用评论区（前台展示联系方式/留言入口）</summary>
    public bool EnableComments { get; set; }
}