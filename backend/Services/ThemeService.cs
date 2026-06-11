using Microsoft.EntityFrameworkCore;
using Mblog.API.Data;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

public interface IThemeService
{
    Task<ThemeSettingResponse?> GetActiveAsync();
    Task<List<ThemeSettingResponse>> GetAllAsync();
    Task<ThemeSettingResponse> SaveAsync(Guid id, UpdateThemeSettingRequest request);
    Task<ThemeSettingResponse> CreateAsync(string themeName);
    Task<bool> DeleteAsync(Guid id);
    Task<ThemeSettingResponse> DuplicateAsync(Guid id, string newName);
    Task<ThemeSettingResponse> ResetAsync();
    Task<bool> ActivateAsync(Guid id);
}

public class ThemeService : IThemeService
{
    private readonly AppDbContext _db;

    public ThemeService(AppDbContext db) => _db = db;

    public async Task<ThemeSettingResponse?> GetActiveAsync()
    {
        var theme = await _db.ThemeSettings.FirstOrDefaultAsync(t => t.IsActive);
        if (theme == null) return null;
        return MapToResponse(theme);
    }

    public async Task<List<ThemeSettingResponse>> GetAllAsync()
    {
        return await _db.ThemeSettings
            .OrderByDescending(t => t.IsActive)
            .ThenBy(t => t.ThemeName)
            .Select(t => new ThemeSettingResponse
            {
                Id = t.Id,
                ThemeName = t.ThemeName,
                PrimaryColor = t.PrimaryColor,
                AccentColor = t.AccentColor,
                BackgroundColor = t.BackgroundColor,
                TextColor = t.TextColor,
                LinkColor = t.LinkColor,
                NavbarBackground = t.NavbarBackground,
                NavbarTextColor = t.NavbarTextColor,
                SurfaceColor = t.SurfaceColor,
                TextSecondaryColor = t.TextSecondaryColor,
                BorderColor = t.BorderColor,
                BorderRadius = t.BorderRadius,
                CustomCss = t.CustomCss,
                IsActive = t.IsActive
            })
            .ToListAsync();
    }

    public async Task<ThemeSettingResponse> SaveAsync(Guid id, UpdateThemeSettingRequest request)
    {
        var theme = await _db.ThemeSettings.FindAsync(id)
            ?? throw new KeyNotFoundException($"主题 {id} 不存在");

        if (request.ThemeName != null) theme.ThemeName = request.ThemeName;
        if (request.PrimaryColor != null) theme.PrimaryColor = request.PrimaryColor;
        if (request.AccentColor != null) theme.AccentColor = request.AccentColor;
        if (request.BackgroundColor != null) theme.BackgroundColor = request.BackgroundColor;
        if (request.TextColor != null) theme.TextColor = request.TextColor;
        if (request.LinkColor != null) theme.LinkColor = request.LinkColor;
        if (request.NavbarBackground != null) theme.NavbarBackground = request.NavbarBackground;
        if (request.NavbarTextColor != null) theme.NavbarTextColor = request.NavbarTextColor;
        if (request.SurfaceColor != null) theme.SurfaceColor = request.SurfaceColor;
        if (request.TextSecondaryColor != null) theme.TextSecondaryColor = request.TextSecondaryColor;
        if (request.BorderColor != null) theme.BorderColor = request.BorderColor;
        if (request.FontFamily != null) theme.FontFamily = request.FontFamily;
        if (request.SuccessColor != null) theme.SuccessColor = request.SuccessColor;
        if (request.DangerColor != null) theme.DangerColor = request.DangerColor;
        if (request.WarningColor != null) theme.WarningColor = request.WarningColor;
        if (request.FooterBackground != null) theme.FooterBackground = request.FooterBackground;
        if (request.FooterTextColor != null) theme.FooterTextColor = request.FooterTextColor;
        if (request.HeroBackground != null) theme.HeroBackground = request.HeroBackground;
        if (request.CodeBackground != null) theme.CodeBackground = request.CodeBackground;
        if (request.BorderRadius != null) theme.BorderRadius = request.BorderRadius;
        if (request.CustomCss != null) theme.CustomCss = request.CustomCss;

        await _db.SaveChangesAsync();
        return MapToResponse(theme);
    }

    public async Task<ThemeSettingResponse> CreateAsync(string themeName)
    {
        var theme = new ThemeSetting
        {
            ThemeName = string.IsNullOrWhiteSpace(themeName) ? "新主题" : themeName,
            IsActive = false
        };
        _db.ThemeSettings.Add(theme);
        await _db.SaveChangesAsync();
        return MapToResponse(theme);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var theme = await _db.ThemeSettings.FindAsync(id);
        if (theme == null) return false;

        // 不允许删除最后一个主题
        var count = await _db.ThemeSettings.CountAsync();
        if (count <= 1) return false;

        _db.ThemeSettings.Remove(theme);
        await _db.SaveChangesAsync();

        // 如果删除的是激活主题，激活另一个
        if (theme.IsActive)
        {
            var next = await _db.ThemeSettings.FirstOrDefaultAsync();
            if (next != null)
            {
                next.IsActive = true;
                await _db.SaveChangesAsync();
            }
        }
        return true;
    }

    public async Task<ThemeSettingResponse> DuplicateAsync(Guid id, string newName)
    {
        var source = await _db.ThemeSettings.FindAsync(id)
            ?? throw new KeyNotFoundException($"主题 {id} 不存在");

        var clone = new ThemeSetting
        {
            ThemeName = string.IsNullOrWhiteSpace(newName) ? $"{source.ThemeName} (副本)" : newName,
            PrimaryColor = source.PrimaryColor,
            AccentColor = source.AccentColor,
            BackgroundColor = source.BackgroundColor,
            TextColor = source.TextColor,
            LinkColor = source.LinkColor,
            NavbarBackground = source.NavbarBackground,
            NavbarTextColor = source.NavbarTextColor,
            SurfaceColor = source.SurfaceColor,
            TextSecondaryColor = source.TextSecondaryColor,
            BorderColor = source.BorderColor,
            FontFamily = source.FontFamily,
            SuccessColor = source.SuccessColor,
            DangerColor = source.DangerColor,
            WarningColor = source.WarningColor,
            FooterBackground = source.FooterBackground,
            FooterTextColor = source.FooterTextColor,
            HeroBackground = source.HeroBackground,
            CodeBackground = source.CodeBackground,
            BorderRadius = source.BorderRadius,
            CustomCss = source.CustomCss,
            IsActive = false
        };
        _db.ThemeSettings.Add(clone);
        await _db.SaveChangesAsync();
        return MapToResponse(clone);
    }

    public async Task<ThemeSettingResponse> ResetAsync()
    {
        var theme = await _db.ThemeSettings.FirstOrDefaultAsync(t => t.IsActive);
        if (theme == null)
        {
            theme = new ThemeSetting();
            _db.ThemeSettings.Add(theme);
        }

        theme.ThemeName = "default";
        theme.PrimaryColor = "#6366f1";
        theme.AccentColor = "#a855f7";
        theme.BackgroundColor = "#f8fafc";
        theme.TextColor = "#111827";
        theme.LinkColor = "#6366f1";
        theme.NavbarBackground = null;
        theme.NavbarTextColor = null;
        theme.SurfaceColor = null;
        theme.TextSecondaryColor = null;
        theme.BorderColor = null;
        theme.FontFamily = null;
        theme.SuccessColor = null;
        theme.DangerColor = null;
        theme.WarningColor = null;
        theme.FooterBackground = null;
        theme.FooterTextColor = null;
        theme.HeroBackground = null;
        theme.CodeBackground = null;
        theme.BorderRadius = "medium";
        theme.CustomCss = "{}";

        await _db.SaveChangesAsync();
        return MapToResponse(theme);
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        var theme = await _db.ThemeSettings.FindAsync(id);
        if (theme == null) return false;

        await _db.ThemeSettings
            .Where(t => t.IsActive)
            .ForEachAsync(t => t.IsActive = false);

        theme.IsActive = true;
        await _db.SaveChangesAsync();
        return true;
    }

    private static ThemeSettingResponse MapToResponse(ThemeSetting theme)
    {
        return new ThemeSettingResponse
        {
            Id = theme.Id,
            ThemeName = theme.ThemeName,
            PrimaryColor = theme.PrimaryColor,
            AccentColor = theme.AccentColor,
            BackgroundColor = theme.BackgroundColor,
            TextColor = theme.TextColor,
            LinkColor = theme.LinkColor,
            NavbarBackground = theme.NavbarBackground,
            NavbarTextColor = theme.NavbarTextColor,
            SurfaceColor = theme.SurfaceColor,
            TextSecondaryColor = theme.TextSecondaryColor,
            BorderColor = theme.BorderColor,
            FontFamily = theme.FontFamily,
            SuccessColor = theme.SuccessColor,
            DangerColor = theme.DangerColor,
            WarningColor = theme.WarningColor,
            FooterBackground = theme.FooterBackground,
            FooterTextColor = theme.FooterTextColor,
            HeroBackground = theme.HeroBackground,
            CodeBackground = theme.CodeBackground,
            BorderRadius = theme.BorderRadius,
            CustomCss = theme.CustomCss,
            IsActive = theme.IsActive
        };
    }
}