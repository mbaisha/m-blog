namespace Mblog.API.Models.Entities;

/// <summary>
/// 导航菜单项，支持多级、可见性和排序
/// </summary>
public class NavigationItem : BaseEntity
{
    /// <summary>菜单名称</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>链接 URL，支持相对路径和绝对路径</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>父级菜单 ID，null 表示一级菜单</summary>
    public Guid? ParentId { get; set; }

    /// <summary>ICON 图标类名（可选）</summary>
    public string? Icon { get; set; }

    /// <summary>是否在新窗口打开</summary>
    public bool OpenInNewTab { get; set; }

    /// <summary>排序序号，越小越靠前</summary>
    public int SortOrder { get; set; }

    /// <summary>是否对外可见</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>导航位置：header / footer / both</summary>
    public string Location { get; set; } = "header";
}