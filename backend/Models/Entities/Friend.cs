namespace Mblog.API.Models.Entities;

/// <summary>
/// 友情链接表
/// </summary>
public class Friend : BaseEntity, ISoftDeletable
{
    /// <summary>站点名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>站点 URL</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>站点描述</summary>
    public string? Description { get; set; }

    /// <summary>Logo 媒体 ID</summary>
    public Guid? LogoImageId { get; set; }

    /// <summary>排序序号，越小越靠前</summary>
    public int SortOrder { get; set; }

    /// <summary>是否对外可见</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>软删除时间</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>Logo 导航属性</summary>
    public Media? LogoImage { get; set; }
}