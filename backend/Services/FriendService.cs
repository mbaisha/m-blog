using Microsoft.EntityFrameworkCore;
using Mblog.API.Data;
using Mblog.API.Models.DTOs.Friend;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

/// <summary>
/// 友情链接服务接口
/// </summary>
public interface IFriendService
{
    /// <summary>获取所有友情链接（管理端）</summary>
    Task<List<FriendListItem>> GetAllAsync();

    /// <summary>根据 ID 获取友情链接</summary>
    Task<FriendListItem?> GetByIdAsync(Guid id);

    /// <summary>创建友情链接</summary>
    Task<FriendListItem> CreateAsync(CreateFriendRequest request);

    /// <summary>更新友情链接</summary>
    Task<FriendListItem?> UpdateAsync(Guid id, UpdateFriendRequest request);

    /// <summary>删除友情链接（软删除）</summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>获取公开可见的友情链接列表</summary>
    Task<List<PublicFriendItem>> GetPublicListAsync();
}

/// <summary>
/// 友情链接服务实现
/// </summary>
public class FriendService : IFriendService
{
    private readonly AppDbContext _db;

    public FriendService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<FriendListItem>> GetAllAsync()
    {
        return await _db.Friends
            .AsNoTracking()
            .Include(x => x.LogoImage)
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.SortOrder)
            .Select(x => new FriendListItem
            {
                Id = x.Id,
                Name = x.Name,
                Url = x.Url,
                Description = x.Description,
                LogoImageUrl = x.LogoImage != null ? x.LogoImage.Url : null,
                SortOrder = x.SortOrder,
                IsVisible = x.IsVisible,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<FriendListItem?> GetByIdAsync(Guid id)
    {
        return await _db.Friends
            .AsNoTracking()
            .Include(x => x.LogoImage)
            .Where(x => x.Id == id && x.DeletedAt == null)
            .Select(x => new FriendListItem
            {
                Id = x.Id,
                Name = x.Name,
                Url = x.Url,
                Description = x.Description,
                LogoImageUrl = x.LogoImage != null ? x.LogoImage.Url : null,
                SortOrder = x.SortOrder,
                IsVisible = x.IsVisible,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<FriendListItem> CreateAsync(CreateFriendRequest request)
    {
        var entity = new Friend
        {
            Name = request.Name,
            Url = request.Url,
            Description = request.Description,
            LogoImageId = request.LogoImageId,
            SortOrder = request.SortOrder,
            IsVisible = request.IsVisible
        };

        _db.Friends.Add(entity);
        await _db.SaveChangesAsync();

        return new FriendListItem
        {
            Id = entity.Id,
            Name = entity.Name,
            Url = entity.Url,
            Description = entity.Description,
            LogoImageUrl = null,
            SortOrder = entity.SortOrder,
            IsVisible = entity.IsVisible,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<FriendListItem?> UpdateAsync(Guid id, UpdateFriendRequest request)
    {
        var entity = await _db.Friends.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
        if (entity == null) return null;

        entity.Name = request.Name;
        entity.Url = request.Url;
        entity.Description = request.Description;
        entity.LogoImageId = request.LogoImageId;
        entity.SortOrder = request.SortOrder;
        entity.IsVisible = request.IsVisible;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _db.Friends.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
        if (entity == null) return false;

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<PublicFriendItem>> GetPublicListAsync()
    {
        return await _db.Friends
            .AsNoTracking()
            .Include(x => x.LogoImage)
            .Where(x => x.IsVisible && x.DeletedAt == null)
            .OrderBy(x => x.SortOrder)
            .Select(x => new PublicFriendItem
            {
                Id = x.Id,
                Name = x.Name,
                Url = x.Url,
                Description = x.Description,
                LogoImageUrl = x.LogoImage != null ? x.LogoImage.Url : null,
                SortOrder = x.SortOrder
            })
            .ToListAsync();
    }
}