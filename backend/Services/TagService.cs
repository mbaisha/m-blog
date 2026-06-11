using Microsoft.EntityFrameworkCore;
using Mblog.API.Data;
using Mblog.API.Models.DTOs.Tag;

namespace Mblog.API.Services;

/// <summary>
/// 标签服务接口
/// </summary>
public interface ITagService
{
    /// <summary>获取所有标签（按名称排序）</summary>
    Task<List<TagListItem>> GetAllAsync();

    /// <summary>根据 ID 获取标签详情</summary>
    Task<TagResponse?> GetByIdAsync(Guid id);

    /// <summary>创建标签</summary>
    Task<TagResponse> CreateAsync(CreateTagRequest request);

    /// <summary>更新标签</summary>
    Task<TagResponse?> UpdateAsync(Guid id, UpdateTagRequest request);

    /// <summary>删除标签（软删除）</summary>
    Task<bool> DeleteAsync(Guid id);
}

/// <summary>
/// 标签服务实现
/// </summary>
public class TagService : ITagService
{
    private readonly AppDbContext _db;

    public TagService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TagListItem>> GetAllAsync()
    {
        return await _db.Tags
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new TagListItem
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Color = x.Color,
                BgColor = x.BgColor,
                ArticleCount = x.ArticleTags.Count,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<TagResponse?> GetByIdAsync(Guid id)
    {
        return await _db.Tags
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new TagResponse
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Color = x.Color,
                BgColor = x.BgColor,
                ArticleCount = x.ArticleTags.Count,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<TagResponse> CreateAsync(CreateTagRequest request)
    {
        if (await _db.Tags.AnyAsync(x => x.Slug == request.Slug))
        {
            throw new ArgumentException($"Slug '{request.Slug}' 已存在");
        }

        var tag = new Models.Entities.Tag
        {
            Name = request.Name,
            Slug = request.Slug,
            Color = request.Color,
            BgColor = request.BgColor
        };

        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(tag.Id)
            ?? throw new InvalidOperationException("创建标签后获取详情失败");
    }

    public async Task<TagResponse?> UpdateAsync(Guid id, UpdateTagRequest request)
    {
        var tag = await _db.Tags.FindAsync(id);
        if (tag == null) return null;

        if (await _db.Tags.AnyAsync(x => x.Slug == request.Slug && x.Id != id))
        {
            throw new ArgumentException($"Slug '{request.Slug}' 已存在");
        }

        tag.Name = request.Name;
        tag.Slug = request.Slug;
        tag.Color = request.Color;
        tag.BgColor = request.BgColor;
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tag = await _db.Tags.FindAsync(id);
        if (tag == null) return false;

        var hasArticles = await _db.ArticleTags.AnyAsync(x => x.TagId == id);
        if (hasArticles)
        {
            throw new InvalidOperationException("该标签已被文章使用，无法删除");
        }

        tag.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}