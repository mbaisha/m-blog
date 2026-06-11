namespace Mblog.API.Models.Entities;

/// <summary>
/// 主题配置表，存储站点主题色及相关样式设置
/// </summary>
public class ThemeSetting : BaseEntity
{
    /// <summary>主题名称</summary>
    public string ThemeName { get; set; } = "default";

    /// <summary>主色（Hex），如 #6366f1</summary>
    public string PrimaryColor { get; set; } = "#6366f1";

    /// <summary>强调色（Hex）</summary>
    public string AccentColor { get; set; } = "#a855f7";

    /// <summary>背景色（Hex）</summary>
    public string BackgroundColor { get; set; } = "#f8fafc";

    /// <summary>文字色（Hex）</summary>
    public string TextColor { get; set; } = "#111827";

    /// <summary>链接色（Hex）</summary>
    public string LinkColor { get; set; } = "#6366f1";

    /// <summary>导航栏背景色（Hex）</summary>
    public string? NavbarBackground { get; set; }

    /// <summary>导航栏文字色（Hex）</summary>
    public string? NavbarTextColor { get; set; }

    /// <summary>卡片底色（Hex），为空时自动根据背景色计算</summary>
    public string? SurfaceColor { get; set; }

    /// <summary>次级文字色（Hex），为空时自动计算</summary>
    public string? TextSecondaryColor { get; set; }

    /// <summary>边框色（Hex），为空时自动计算</summary>
    public string? BorderColor { get; set; }

    /// <summary>字体族：sans-serif / serif / monospace</summary>
    public string? FontFamily { get; set; }

    /// <summary>成功色（Hex）</summary>
    public string? SuccessColor { get; set; }

    /// <summary>危险色（Hex）</summary>
    public string? DangerColor { get; set; }

    /// <summary>警告色（Hex）</summary>
    public string? WarningColor { get; set; }

    /// <summary>页脚背景色（Hex），为空时自动计算</summary>
    public string? FooterBackground { get; set; }

    /// <summary>页脚文字色（Hex），为空时自动计算</summary>
    public string? FooterTextColor { get; set; }

    /// <summary>Hero 区域背景色（Hex），为空时自动计算</summary>
    public string? HeroBackground { get; set; }

    /// <summary>代码块背景色（Hex），为空时自动计算</summary>
    public string? CodeBackground { get; set; }

    /// <summary>圆角大小：none / small / medium / large</summary>
    public string BorderRadius { get; set; } = "medium";

    /// <summary>自定义 CSS 变量 JSON 扩展</summary>
    public string CustomCss { get; set; } = "{}";

    /// <summary>是否启用</summary>
    public bool IsActive { get; set; } = true;
}