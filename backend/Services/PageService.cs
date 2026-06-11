using Microsoft.EntityFrameworkCore;
using Mblog.API.Data;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

public interface IPageService
{
    Task<List<PageListItem>> GetAllAsync();
    Task<PageDetail?> GetByIdAsync(Guid id);
    Task<PageDetail> CreateAsync(CreatePageRequest request);
    Task<PageDetail?> UpdateAsync(Guid id, UpdatePageRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<PublicPageItem?> GetPublishedBySlugAsync(string slug);
    Task<List<PublicPageItem>> GetPublishedListAsync();

    /// <summary>发布页面</summary>
    Task<PageDetail?> PublishAsync(Guid id);

    /// <summary>下架页面</summary>
    Task<PageDetail?> UnpublishAsync(Guid id);
}

public class PageService : IPageService
{
    private readonly AppDbContext _db;

    public PageService(AppDbContext db) => _db = db;

    public async Task<List<PageListItem>> GetAllAsync()
    {
        return await _db.Pages
            .OrderBy(p => p.SortOrder)
            .ThenByDescending(p => p.CreatedAt)
            .Select(p => new PageListItem
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Summary = p.Summary,
                Status = p.Status,
                IsVisible = p.IsVisible,
                EnableComments = p.EnableComments,
                SortOrder = p.SortOrder,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<PageDetail?> GetByIdAsync(Guid id)
    {
        return await _db.Pages
            .Where(p => p.Id == id)
            .Select(p => new PageDetail
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Content = p.Content,
                Summary = p.Summary,
                Status = p.Status,
                IsVisible = p.IsVisible,
                EnableComments = p.EnableComments,
                SortOrder = p.SortOrder,
                SeoTitle = p.SeoTitle,
                SeoDescription = p.SeoDescription,
                SeoKeywords = p.SeoKeywords,
                PublishedAt = p.PublishedAt,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PageDetail> CreateAsync(CreatePageRequest request)
    {
        if (await _db.Pages.AnyAsync(p => p.Slug == request.Slug))
            throw new ArgumentException($"Slug「{request.Slug}」已被使用");

        var page = new Page
        {
            Title = request.Title,
            Slug = request.Slug,
            Content = request.Content,
            Summary = request.Summary,
            Status = request.Status,
            IsVisible = request.IsVisible,
            EnableComments = request.EnableComments,
            SortOrder = request.SortOrder,
            SeoTitle = request.SeoTitle,
            SeoDescription = request.SeoDescription,
            SeoKeywords = request.SeoKeywords,
            PublishedAt = request.Status == "published" ? DateTimeOffset.UtcNow : null
        };

        _db.Pages.Add(page);
        await _db.SaveChangesAsync();

        return (await GetByIdAsync(page.Id))!;
    }

    public async Task<PageDetail?> UpdateAsync(Guid id, UpdatePageRequest request)
    {
        var page = await _db.Pages.FindAsync(id);
        if (page == null) return null;

        if (await _db.Pages.AnyAsync(p => p.Slug == request.Slug && p.Id != id))
            throw new ArgumentException($"Slug「{request.Slug}」已被使用");

        page.Title = request.Title;
        page.Slug = request.Slug;
        page.Content = request.Content;
        page.Summary = request.Summary;
        page.Status = request.Status;
        page.IsVisible = request.IsVisible;
        page.EnableComments = request.EnableComments;
        page.SortOrder = request.SortOrder;
        page.SeoTitle = request.SeoTitle;
        page.SeoDescription = request.SeoDescription;
        page.SeoKeywords = request.SeoKeywords;

        if (request.Status == "published" && page.PublishedAt == null)
            page.PublishedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var page = await _db.Pages.FindAsync(id);
        if (page == null) return false;
        _db.Pages.Remove(page);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PageDetail?> PublishAsync(Guid id)
    {
        var page = await _db.Pages.FindAsync(id);
        if (page == null) return null;

        page.Status = "published";
        page.IsVisible = true;
        if (page.PublishedAt == null)
            page.PublishedAt = DateTimeOffset.UtcNow;
        page.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<PageDetail?> UnpublishAsync(Guid id)
    {
        var page = await _db.Pages.FindAsync(id);
        if (page == null) return null;

        page.Status = "draft";
        page.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<PublicPageItem?> GetPublishedBySlugAsync(string slug)
    {
        return await _db.Pages
            .Where(p => p.Slug == slug && p.Status == "published" && p.IsVisible)
            .Select(p => new PublicPageItem
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Summary = p.Summary,
                Content = p.Content,
                SeoTitle = p.SeoTitle,
                SeoDescription = p.SeoDescription,
                SeoKeywords = p.SeoKeywords,
                EnableComments = p.EnableComments,
                PublishedAt = p.PublishedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<PublicPageItem>> GetPublishedListAsync()
    {
        return await _db.Pages
            .Where(p => p.Status == "published" && p.IsVisible)
            .OrderBy(p => p.SortOrder)
            .Select(p => new PublicPageItem
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Summary = p.Summary,
                EnableComments = p.EnableComments,
                PublishedAt = p.PublishedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();
    }
}