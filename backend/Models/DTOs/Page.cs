// ===== Page DTOs =====

public class PageListItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Status { get; set; } = "draft";
    public bool IsVisible { get; set; }
    public bool EnableComments { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class PageDetail : PageListItem
{
    public string? Content { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoKeywords { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}

public class CreatePageRequest
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public string Status { get; set; } = "draft";
    public bool IsVisible { get; set; } = true;
    public bool EnableComments { get; set; }
    public int SortOrder { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoKeywords { get; set; }
}

public class UpdatePageRequest : CreatePageRequest { }

public class PublicPageItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoKeywords { get; set; }
    public bool EnableComments { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

// ===== NavigationItem DTOs =====

public class NavigationItemResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string? Icon { get; set; }
    public bool OpenInNewTab { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public string Location { get; set; } = "header";
    public List<NavigationItemResponse> Children { get; set; } = new();
}

public class CreateNavigationItemRequest
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string? Icon { get; set; }
    public bool OpenInNewTab { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    public string Location { get; set; } = "header";
}

public class UpdateNavigationItemRequest : CreateNavigationItemRequest { }

public class PublicNavigationItem
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool OpenInNewTab { get; set; }
    public List<PublicNavigationItem> Children { get; set; } = new();
}

// ===== FooterConfig DTOs =====

public class FooterConfigResponse
{
    public Guid Id { get; set; }
    public string? Copyright { get; set; }
    public string? IcpNumber { get; set; }
    public string? IcpUrl { get; set; }
    public string? PoliceNumber { get; set; }
    public string? PoliceUrl { get; set; }
    public string SocialLinks { get; set; } = "[]";
    public string Blocks { get; set; } = "[]";
    public string Style { get; set; } = "detailed";
    public bool IsVisible { get; set; } = true;
    public string? ContactEmail { get; set; }
    public string? ContactWeChat { get; set; }
    public string? ContactPhone { get; set; }
}

public class UpdateFooterConfigRequest
{
    public string? Copyright { get; set; }
    public string? IcpNumber { get; set; }
    public string? IcpUrl { get; set; }
    public string? PoliceNumber { get; set; }
    public string? PoliceUrl { get; set; }
    public string? SocialLinks { get; set; }
    public string? Blocks { get; set; }
    public string? Style { get; set; }
    public bool? IsVisible { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactWeChat { get; set; }
    public string? ContactPhone { get; set; }
}

// ===== ThemeSetting DTOs =====

public class ThemeSettingResponse
{
    public Guid Id { get; set; }
    public string ThemeName { get; set; } = "default";
    public string PrimaryColor { get; set; } = "#6366f1";
    public string AccentColor { get; set; } = "#a855f7";
    public string BackgroundColor { get; set; } = "#f8fafc";
    public string TextColor { get; set; } = "#111827";
    public string LinkColor { get; set; } = "#6366f1";
    public string? NavbarBackground { get; set; }
    public string? NavbarTextColor { get; set; }
    public string? SurfaceColor { get; set; }
    public string? TextSecondaryColor { get; set; }
    public string? BorderColor { get; set; }
    public string? FontFamily { get; set; }
    public string? SuccessColor { get; set; }
    public string? DangerColor { get; set; }
    public string? WarningColor { get; set; }
    public string? FooterBackground { get; set; }
    public string? FooterTextColor { get; set; }
    public string? HeroBackground { get; set; }
    public string? CodeBackground { get; set; }
    public string BorderRadius { get; set; } = "medium";
    public string CustomCss { get; set; } = "{}";
    public bool IsActive { get; set; }
}

public class UpdateThemeSettingRequest
{
    public string? ThemeName { get; set; }
    public string? PrimaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public string? LinkColor { get; set; }
    public string? NavbarBackground { get; set; }
    public string? NavbarTextColor { get; set; }
    public string? SurfaceColor { get; set; }
    public string? TextSecondaryColor { get; set; }
    public string? BorderColor { get; set; }
    public string? FontFamily { get; set; }
    public string? SuccessColor { get; set; }
    public string? DangerColor { get; set; }
    public string? WarningColor { get; set; }
    public string? FooterBackground { get; set; }
    public string? FooterTextColor { get; set; }
    public string? HeroBackground { get; set; }
    public string? CodeBackground { get; set; }
    public string? BorderRadius { get; set; }
    public string? CustomCss { get; set; }
}

// ===== ModuleLayout DTOs =====

public class ModuleLayoutResponse
{
    public Guid Id { get; set; }
    public string PageKey { get; set; } = string.Empty;
    public string ModuleKey { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; }
    public string Config { get; set; } = "{}";
}

public class SaveModuleLayoutRequest
{
    public string PageKey { get; set; } = string.Empty;
    public string ModuleKey { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string Config { get; set; } = "{}";
}

public class UpdateModuleLayoutRequest
{
    public string? Title { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string Config { get; set; } = "{}";
}

public class BatchSaveLayoutRequest
{
    public string PageKey { get; set; } = string.Empty;
    public List<SaveModuleLayoutRequest> Modules { get; set; } = new();
}