namespace Mblog.API.Models.Entities;

/// <summary>
/// 媒体资源表，管理所有上传的图片、视频和文件
/// </summary>
public class Media : BaseEntity, ISoftDeletable
{
    /// <summary>媒体类型：image / video / file</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>对外访问 URL</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>缩略图 URL</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>安全存储文件名</summary>
    public string Filename { get; set; } = string.Empty;

    /// <summary>用户上传时的原始文件名</summary>
    public string? OriginalFilename { get; set; }

    /// <summary>本地存储路径或对象存储 Key</summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>文件大小（字节）</summary>
    public long SizeBytes { get; set; }

    /// <summary>MIME 类型，如 image/jpeg</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>图片宽度（像素），仅图片类型有效</summary>
    public int? Width { get; set; }

    /// <summary>图片高度（像素），仅图片类型有效</summary>
    public int? Height { get; set; }

    /// <summary>视频时长（秒），仅视频类型有效</summary>
    public decimal? DurationSeconds { get; set; }

    /// <summary>上传者用户 ID</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>软删除时间</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>上传者导航属性</summary>
    public User? Creator { get; set; }
}