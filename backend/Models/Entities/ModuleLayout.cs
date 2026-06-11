namespace Mblog.API.Models.Entities;

/// <summary>
/// 模块布局配置表，定义首页及各页面的模块顺序、显示状态和数据源
/// </summary>
public class ModuleLayout : BaseEntity
{
    /// <summary>页面标识：home / post / archive / search / about / projects / friends</summary>
    public string PageKey { get; set; } = string.Empty;

    /// <summary>模块标识：hero / featured_posts / recent_posts / categories / tags / projects / about_me / custom</summary>
    public string ModuleKey { get; set; } = string.Empty;

    /// <summary>模块显示标题</summary>
    public string? Title { get; set; }

    /// <summary>排序序号，越小越靠前</summary>
    public int SortOrder { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>模块配置 JSON，如数据源、显示数量、布局样式等</summary>
    public string Config { get; set; } = "{}";
}