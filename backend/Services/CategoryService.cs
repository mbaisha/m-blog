using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mblog.API.Data;
using Mblog.API.Models.DTOs.Category;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

/// <summary>
/// 分类服务接口
/// </summary>
public interface ICategoryService
{
    /// <summary>获取所有分类（树形结构，按排序序号升序）</summary>
    Task<List<CategoryListItem>> GetAllAsync();

    /// <summary>获取分类平面列表（含层级信息）</summary>
    Task<List<CategoryListItem>> GetFlatListAsync();

    /// <summary>根据 ID 获取分类详情</summary>
    Task<CategoryResponse?> GetByIdAsync(Guid id);

    /// <summary>创建分类</summary>
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);

    /// <summary>更新分类</summary>
    Task<CategoryResponse?> UpdateAsync(Guid id, UpdateCategoryRequest request);

    /// <summary>软删除分类</summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>更新分类排序</summary>
    Task<bool> UpdateSortAsync(Guid id, int sortOrder);
}

/// <summary>
/// 分类服务实现
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public CategoryService(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<List<CategoryListItem>> GetFlatListAsync()
    {
        return await _db.Categories
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new CategoryListItem
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                SortOrder = x.SortOrder,
                ParentId = x.ParentId,
                Level = x.Level,
                ArticleCount = _db.Articles.Count(a => a.DeletedAt == null && a.Status == "published" && (a.CategoryId == x.Id || a.ArticleCategories.Any(ac => ac.CategoryId == x.Id))),
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<CategoryListItem>> GetAllAsync()
    {
        var cacheKey = "categories_all_tree";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var all = await _db.Categories
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CreatedAt)
                .Select(x => new CategoryListItem
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    ParentId = x.ParentId,
                    Level = x.Level,
                    ArticleCount = _db.Articles.Count(a => a.DeletedAt == null && a.Status == "published" && (a.CategoryId == x.Id || a.ArticleCategories.Any(ac => ac.CategoryId == x.Id))),
                    CreatedAt = x.CreatedAt,
                    Children = new List<CategoryListItem>()
                })
                .ToListAsync();

            // 构建树形结构（最多三级）
            var lookup = all.ToDictionary(x => x.Id);
            var roots = new List<CategoryListItem>();

            foreach (var item in all)
            {
                if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parent))
                {
                    parent.Children.Add(item);
                }
                else
                {
                    roots.Add(item);
                }
            }

            return roots;
        })!;
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid id)
    {
        return await _db.Categories
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CategoryResponse
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                CoverImageId = x.CoverImageId,
                CoverImageUrl = x.CoverImage != null ? x.CoverImage.Url : null,
                SortOrder = x.SortOrder,
                ParentId = x.ParentId,
                Level = x.Level,
                ArticleCount = x.Articles.Count(a => a.DeletedAt == null),
                SeoTitle = x.SeoTitle,
                SeoDescription = x.SeoDescription,
                SeoKeywords = x.SeoKeywords,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        // 检查 Slug 是否唯一
        if (await _db.Categories.AnyAsync(x => x.Slug == request.Slug))
        {
            throw new ArgumentException($"Slug '{request.Slug}' 已存在");
        }

        // 计算层级
        var level = 0;
        if (request.ParentId.HasValue)
        {
            var parent = await _db.Categories.FindAsync(request.ParentId.Value);
            if (parent == null)
                throw new ArgumentException("父级分类不存在");
            if (parent.Level >= 1)
                throw new ArgumentException("分类最多支持二级层级");
            level = parent.Level + 1;
        }

        var category = new Models.Entities.Category
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            CoverImageId = request.CoverImageId,
            SortOrder = request.SortOrder,
            ParentId = request.ParentId,
            Level = level,
            SeoTitle = request.SeoTitle,
            SeoDescription = request.SeoDescription,
            SeoKeywords = request.SeoKeywords
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        _cache.Remove("categories_all_tree");

        return await GetByIdAsync(category.Id)
            ?? throw new InvalidOperationException("创建分类后获取详情失败");
    }

    public async Task<CategoryResponse?> UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return null;

        // 检查 Slug 是否唯一（排除自身）
        if (await _db.Categories.AnyAsync(x => x.Slug == request.Slug && x.Id != id))
        {
            throw new ArgumentException($"Slug '{request.Slug}' 已存在");
        }

        // 不允许将自己设为父级
        if (request.ParentId == id)
            throw new ArgumentException("不能将自己设为父级分类");

        // 计算层级
        var level = 0;
        if (request.ParentId.HasValue)
        {
            var parent = await _db.Categories.FindAsync(request.ParentId.Value);
            if (parent == null)
                throw new ArgumentException("父级分类不存在");
            if (parent.Level >= 2)
                throw new ArgumentException("分类最多支持二级层级");
            level = parent.Level + 1;
        }

        category.Name = request.Name;
        category.Slug = request.Slug;
        category.Description = request.Description;
        category.CoverImageId = request.CoverImageId;
        category.SortOrder = request.SortOrder;
        category.ParentId = request.ParentId;
        category.Level = level;
        category.SeoTitle = request.SeoTitle;
        category.SeoDescription = request.SeoDescription;
        category.SeoKeywords = request.SeoKeywords;

        await _db.SaveChangesAsync();
        _cache.Remove("categories_all_tree");

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _db.Categories
            .Include(x => x.Children)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (category == null) return false;

        // 检查是否有子分类
        if (category.Children.Any(c => c.DeletedAt == null))
        {
            throw new InvalidOperationException("该分类下还有子分类，请先删除子分类");
        }

        // 检查是否有文章引用（主分类）
        var hasArticles = await _db.Articles.AnyAsync(x => x.CategoryId == id && x.DeletedAt == null);
        if (hasArticles)
        {
            throw new InvalidOperationException("该分类下还有文章，无法删除");
        }

        category.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        _cache.Remove("categories_all_tree");
        return true;
    }

    public async Task<bool> UpdateSortAsync(Guid id, int sortOrder)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return false;

        category.SortOrder = sortOrder;
        await _db.SaveChangesAsync();
        _cache.Remove("categories_all_tree");
        return true;
    }
}