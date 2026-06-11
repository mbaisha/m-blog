namespace Mblog.API.Models.Entities;

/// <summary>
/// 文章标签表
/// </summary>
public class Tag : BaseEntity, ISoftDeletable
{
    /// <summary>标签名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL 标识，唯一</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>标签文字颜色（十六进制色值）</summary>
    public string? Color { get; set; }

    /// <summary>标签背景颜色（十六进制色值），为空时自动生成</summary>
    public string? BgColor { get; set; }

    /// <summary>软删除时间</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>标签-文章关联集合</summary>
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}