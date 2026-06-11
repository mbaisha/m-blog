namespace Mblog.API.Models.DTOs.Profile;

/// <summary>
/// 个人页面模块响应
/// </summary>
public class ProfileSectionResponse
{
    public Guid Id { get; set; }
    public string SectionType { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Content { get; set; }
    public int SortOrder { get; set; }
    public string Metadata { get; set; } = "{}";
    public bool IsEnabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// 创建/更新个人页面模块请求
/// </summary>
public class SaveProfileSectionRequest
{
    public string SectionType { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Content { get; set; }
    public int SortOrder { get; set; }
    public string Metadata { get; set; } = "{}";
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 批量保存个人页面模块请求
/// </summary>
public class BatchSaveProfileSectionsRequest
{
    public List<SaveProfileSectionRequest> Sections { get; set; } = new();
}