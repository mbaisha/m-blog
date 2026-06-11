namespace Mblog.API.Models.DTOs.Tag;

/// <summary>
/// 创建标签请求 DTO
/// </summary>
public class CreateTagRequest
{
    /// <summary>标签名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL 标识，唯一</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>标签文字颜色（十六进制色值），为空则自动选取</summary>
    public string? Color { get; set; }

    /// <summary>标签背景颜色（十六进制色值），为空则自动生成</summary>
    public string? BgColor { get; set; }
}

/// <summary>
/// 更新标签请求 DTO
/// </summary>
public class UpdateTagRequest
{
    /// <summary>标签名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL 标识，唯一</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>标签文字颜色（十六进制色值），为空则自动选取</summary>
    public string? Color { get; set; }

    /// <summary>标签背景颜色（十六进制色值），为空则自动生成</summary>
    public string? BgColor { get; set; }
}

/// <summary>
/// 标签响应 DTO
/// </summary>
public class TagResponse
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

    /// <summary>关联文章数</summary>
    public int ArticleCount { get; set; }

    /// <summary>创建时间</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// 标签列表项 DTO（精简）
/// </summary>
public class TagListItem
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

    /// <summary>关联文章数</summary>
    public int ArticleCount { get; set; }

    /// <summary>创建时间</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
