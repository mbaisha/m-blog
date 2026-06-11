using Microsoft.EntityFrameworkCore;
using Mblog.API.Data;
using Mblog.API.Models.DTOs.Profile;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

/// <summary>
/// 个人页面服务接口
/// </summary>
public interface IProfileService
{
    /// <summary>获取所有启用的个人页面模块（公开）</summary>
    Task<List<ProfileSectionResponse>> GetEnabledSectionsAsync();

    /// <summary>获取所有个人页面模块（管理端）</summary>
    Task<List<ProfileSectionResponse>> GetAllSectionsAsync();

    /// <summary>批量保存个人页面模块</summary>
    Task<List<ProfileSectionResponse>> BatchSaveAsync(List<SaveProfileSectionRequest> requests);
}

/// <summary>
/// 个人页面服务实现
/// </summary>
public class ProfileService : IProfileService
{
    private readonly AppDbContext _db;

    public ProfileService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProfileSectionResponse>> GetEnabledSectionsAsync()
    {
        return await _db.ProfileSections
            .AsNoTracking()
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.SortOrder)
            .Select(x => new ProfileSectionResponse
            {
                Id = x.Id,
                SectionType = x.SectionType,
                Title = x.Title,
                Content = x.Content,
                SortOrder = x.SortOrder,
                Metadata = x.Metadata,
                IsEnabled = x.IsEnabled,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<List<ProfileSectionResponse>> GetAllSectionsAsync()
    {
        return await _db.ProfileSections
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .Select(x => new ProfileSectionResponse
            {
                Id = x.Id,
                SectionType = x.SectionType,
                Title = x.Title,
                Content = x.Content,
                SortOrder = x.SortOrder,
                Metadata = x.Metadata,
                IsEnabled = x.IsEnabled,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<List<ProfileSectionResponse>> BatchSaveAsync(List<SaveProfileSectionRequest> requests)
    {
        // 清除现有数据，重新插入（保持简单，适合个人页面数据量小的场景）
        var existing = await _db.ProfileSections.ToListAsync();
        _db.ProfileSections.RemoveRange(existing);

        var sections = requests.Select((r, i) => new ProfileSection
        {
            SectionType = r.SectionType,
            Title = r.Title,
            Content = r.Content,
            SortOrder = r.SortOrder,
            Metadata = r.Metadata,
            IsEnabled = r.IsEnabled
        }).ToList();

        _db.ProfileSections.AddRange(sections);
        await _db.SaveChangesAsync();

        return sections.Select(x => new ProfileSectionResponse
        {
            Id = x.Id,
            SectionType = x.SectionType,
            Title = x.Title,
            Content = x.Content,
            SortOrder = x.SortOrder,
            Metadata = x.Metadata,
            IsEnabled = x.IsEnabled,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        }).ToList();
    }
}