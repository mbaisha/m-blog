namespace Mblog.API.Models.DTOs.Media;

/// <summary>
/// 上传响应 DTO
/// </summary>
public class UploadResponse
{
    /// <summary>媒体 ID</summary>
    public Guid Id { get; set; }

    /// <summary>对外访问 URL</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>文件名称</summary>
    public string Filename { get; set; } = string.Empty;

    /// <summary>原始文件名</summary>
    public string? OriginalFilename { get; set; }

    /// <summary>文件大小（字节）</summary>
    public long SizeBytes { get; set; }

    /// <summary>MIME 类型</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>图片宽度</summary>
    public int? Width { get; set; }

    /// <summary>图片高度</summary>
    public int? Height { get; set; }

    /// <summary>媒体类型：image / video / file</summary>
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// 媒体列表项
/// </summary>
public class MediaListItem
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string? OriginalFilename { get; set; }
    public long SizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// 媒体查询参数
/// </summary>
public class MediaQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 30;
    public string? Type { get; set; }
    public string? Keyword { get; set; }
}