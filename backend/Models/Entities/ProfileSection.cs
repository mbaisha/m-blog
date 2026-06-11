namespace Mblog.API.Models.Entities;

/// <summary>
/// 个人页面模块表，管理关于我页面的各个展示区块
/// </summary>
public class ProfileSection : BaseEntity
{
    /// <summary>模块类型：hero / intro / skills / experience / education / contact / social / custom</summary>
    public string SectionType { get; set; } = string.Empty;

    /// <summary>模块标题</summary>
    public string? Title { get; set; }

    /// <summary>模块内容（Markdown 或 HTML）</summary>
    public string? Content { get; set; }

    /// <summary>排序序号，越小越靠前</summary>
    public int SortOrder { get; set; }

    /// <summary>扩展配置 JSON</summary>
    public string Metadata { get; set; } = "{}";

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;
}