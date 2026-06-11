namespace Mblog.API.Models.DTOs.Friend;

/// <summary>
/// 友情链接列表项
/// </summary>
public class FriendListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoImageUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// 创建友情链接请求
/// </summary>
public class CreateFriendRequest
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? LogoImageId { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}

/// <summary>
/// 更新友情链接请求
/// </summary>
public class UpdateFriendRequest
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? LogoImageId { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}

/// <summary>
/// 前台公开友情链接项
/// </summary>
public class PublicFriendItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoImageUrl { get; set; }
    public int SortOrder { get; set; }
}