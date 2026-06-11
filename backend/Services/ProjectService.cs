using Microsoft.EntityFrameworkCore;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Models.DTOs.Project;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

/// <summary>
/// 项目展示服务接口
/// </summary>
public interface IProjectService
{
    /// <summary>分页查询项目列表（管理端）</summary>
    Task<PagedResponse<ProjectListItem>> GetPagedAsync(ProjectQueryParams query);

    /// <summary>根据 ID 获取项目详情（管理端）</summary>
    Task<ProjectDetailResponse?> GetByIdAsync(Guid id);

    /// <summary>根据 Slug 获取项目详情（公开）</summary>
    Task<PublicProjectDetailResponse?> GetPublicBySlugAsync(string slug);

    /// <summary>创建项目</summary>
    Task<ProjectDetailResponse> CreateAsync(CreateProjectRequest request);

    /// <summary>更新项目</summary>
    Task<ProjectDetailResponse?> UpdateAsync(Guid id, UpdateProjectRequest request);

    /// <summary>删除项目（软删除）</summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>获取公开项目列表</summary>
    Task<List<PublicProjectListItem>> GetPublicListAsync();
}

/// <summary>
/// 项目展示服务实现
/// </summary>
public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;

    public ProjectService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResponse<ProjectListItem>> GetPagedAsync(ProjectQueryParams query)
    {
        var q = _db.Projects
            .AsNoTracking()
            .Include(x => x.CoverImage)
            .Where(x => x.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.ToLower();
            q = q.Where(x => x.Title.ToLower().Contains(keyword)
                          || (x.Summary != null && x.Summary.ToLower().Contains(keyword)));
        }
        if (query.IsVisible.HasValue)
            q = q.Where(x => x.IsVisible == query.IsVisible.Value);

        var total = await q.CountAsync();
        var items = await q
            .OrderBy(x => x.SortOrder)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new ProjectListItem
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Summary = x.Summary,
                CoverImageUrl = x.CoverImage != null ? x.CoverImage.Url : null,
                TechStack = x.TechStack != null ? new List<string>() : new List<string>(),
                ProjectUrl = x.ProjectUrl,
                GithubUrl = x.GithubUrl,
                DemoUrl = x.DemoUrl,
                SortOrder = x.SortOrder,
                IsVisible = x.IsVisible,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        // 手动解析 TechStack JSON
        foreach (var item in items)
        {
            var entity = await _db.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == item.Id);
            if (entity?.TechStack != null)
            {
                try
                {
                    item.TechStack = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.TechStack) ?? new();
                }
                catch { }
            }
        }

        return new PagedResponse<ProjectListItem>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total,
            Items = items
        };
    }

    public async Task<ProjectDetailResponse?> GetByIdAsync(Guid id)
    {
        var entity = await _db.Projects
            .AsNoTracking()
            .Include(x => x.CoverImage)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

        if (entity == null) return null;

        var techStack = new List<string>();
        if (entity.TechStack != null)
        {
            try
            {
                techStack = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.TechStack) ?? new();
            }
            catch { }
        }

        return new ProjectDetailResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Slug = entity.Slug,
            Summary = entity.Summary,
            Content = entity.Content,
            CoverImageId = entity.CoverImageId,
            CoverImageUrl = entity.CoverImage?.Url,
            TechStack = techStack,
            ProjectUrl = entity.ProjectUrl,
            GithubUrl = entity.GithubUrl,
            DemoUrl = entity.DemoUrl,
            SortOrder = entity.SortOrder,
            IsVisible = entity.IsVisible,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public async Task<PublicProjectDetailResponse?> GetPublicBySlugAsync(string slug)
    {
        var entity = await _db.Projects
            .AsNoTracking()
            .Include(x => x.CoverImage)
            .FirstOrDefaultAsync(x => x.Slug == slug && x.IsVisible && x.DeletedAt == null);

        if (entity == null) return null;

        var techStack = new List<string>();
        if (entity.TechStack != null)
        {
            try
            {
                techStack = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.TechStack) ?? new();
            }
            catch { }
        }

        return new PublicProjectDetailResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Slug = entity.Slug,
            Summary = entity.Summary,
            Content = entity.Content,
            CoverImageUrl = entity.CoverImage?.Url,
            TechStack = techStack,
            ProjectUrl = entity.ProjectUrl,
            GithubUrl = entity.GithubUrl,
            DemoUrl = entity.DemoUrl,
            SortOrder = entity.SortOrder,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public async Task<ProjectDetailResponse> CreateAsync(CreateProjectRequest request)
    {
        // 检查 Slug 唯一性
        if (await _db.Projects.AnyAsync(x => x.Slug == request.Slug && x.DeletedAt == null))
            throw new ArgumentException($"Slug \"{request.Slug}\" 已存在");

        var entity = new Models.Entities.Project
        {
            Title = request.Title,
            Slug = request.Slug,
            Summary = request.Summary,
            Content = request.Content,
            CoverImageId = request.CoverImageId,
            TechStack = System.Text.Json.JsonSerializer.Serialize(request.TechStack),
            ProjectUrl = request.ProjectUrl,
            GithubUrl = request.GithubUrl,
            DemoUrl = request.DemoUrl,
            SortOrder = request.SortOrder,
            IsVisible = request.IsVisible
        };

        _db.Projects.Add(entity);
        await _db.SaveChangesAsync();

        return new ProjectDetailResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Slug = entity.Slug,
            Summary = entity.Summary,
            Content = entity.Content,
            TechStack = request.TechStack,
            ProjectUrl = entity.ProjectUrl,
            GithubUrl = entity.GithubUrl,
            DemoUrl = entity.DemoUrl,
            SortOrder = entity.SortOrder,
            IsVisible = entity.IsVisible,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public async Task<ProjectDetailResponse?> UpdateAsync(Guid id, UpdateProjectRequest request)
    {
        var entity = await _db.Projects.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
        if (entity == null) return null;

        // 检查 Slug 唯一性（排除自身）
        if (await _db.Projects.AnyAsync(x => x.Slug == request.Slug && x.Id != id && x.DeletedAt == null))
            throw new ArgumentException($"Slug \"{request.Slug}\" 已存在");

        entity.Title = request.Title;
        entity.Slug = request.Slug;
        entity.Summary = request.Summary;
        entity.Content = request.Content;
        entity.CoverImageId = request.CoverImageId;
        entity.TechStack = System.Text.Json.JsonSerializer.Serialize(request.TechStack);
        entity.ProjectUrl = request.ProjectUrl;
        entity.GithubUrl = request.GithubUrl;
        entity.DemoUrl = request.DemoUrl;
        entity.SortOrder = request.SortOrder;
        entity.IsVisible = request.IsVisible;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _db.Projects.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
        if (entity == null) return false;

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<PublicProjectListItem>> GetPublicListAsync()
    {
        var items = await _db.Projects
            .AsNoTracking()
            .Include(x => x.CoverImage)
            .Where(x => x.IsVisible && x.DeletedAt == null)
            .OrderBy(x => x.SortOrder)
            .Select(x => new PublicProjectListItem
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Summary = x.Summary,
                CoverImageUrl = x.CoverImage != null ? x.CoverImage.Url : null,
                ProjectUrl = x.ProjectUrl,
                GithubUrl = x.GithubUrl,
                DemoUrl = x.DemoUrl,
                SortOrder = x.SortOrder,
                TechStack = new List<string>(),
            })
            .ToListAsync();

        // 手动解析 TechStack JSON
        foreach (var item in items)
        {
            var entity = await _db.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == item.Id);
            if (entity?.TechStack != null)
            {
                try
                {
                    item.TechStack = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.TechStack) ?? new();
                }
                catch { }
            }
        }

        return items;
    }
}