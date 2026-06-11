namespace Mblog.API.Models.Entities;

/// <summary>
/// 页脚配置表，存储版权信息、备案号、社交媒体链接等
/// </summary>
public class FooterConfig : BaseEntity
{
    /// <summary>版权信息文字</summary>
    public string? Copyright { get; set; }

    /// <summary>ICP 备案号</summary>
    public string? IcpNumber { get; set; }

    /// <summary>ICP 备案链接</summary>
    public string? IcpUrl { get; set; }

    /// <summary>公安备案号</summary>
    public string? PoliceNumber { get; set; }

    /// <summary>公安备案链接</summary>
    public string? PoliceUrl { get; set; }

    /// <summary>社交媒体链接 JSON：[{ "platform": "github", "url": "...", "icon": "..." }]</summary>
    public string SocialLinks { get; set; } = "[]";

    /// <summary>页脚区块 JSON：[{ "title": "关于", "links": [{ "label": "...", "url": "..." }] }]</summary>
    public string Blocks { get; set; } = "[]";

    /// <summary>页脚样式：simple / detailed / centered</summary>
    public string Style { get; set; } = "detailed";

    /// <summary>是否显示页脚</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>联系邮箱</summary>
    public string? ContactEmail { get; set; }

    /// <summary>微信号</summary>
    public string? ContactWeChat { get; set; }

    /// <summary>联系电话</summary>
    public string? ContactPhone { get; set; }
}