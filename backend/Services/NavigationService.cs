using Microsoft.EntityFrameworkCore;
using Mblog.API.Data;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

public interface INavigationService
{
    Task<List<NavigationItemResponse>> GetAllAsync();
    Task<NavigationItemResponse?> GetByIdAsync(Guid id);
    Task<NavigationItemResponse> CreateAsync(CreateNavigationItemRequest request);
    Task<NavigationItemResponse?> UpdateAsync(Guid id, UpdateNavigationItemRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<List<PublicNavigationItem>> GetPublicByLocationAsync(string location);
    Task<bool> BatchUpdateSortAsync(List<NavigationSortItem> items);
}

public class NavigationSortItem
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
    public Guid? ParentId { get; set; }
}

public class NavigationService : INavigationService
{
    private readonly AppDbContext _db;

    public NavigationService(AppDbContext db) => _db = db;

    public async Task<List<NavigationItemResponse>> GetAllAsync()
    {
        var items = await _db.NavigationItems
            .OrderBy(n => n.SortOrder)
            .ToListAsync();

        return BuildTree(items, null);
    }

    public async Task<NavigationItemResponse?> GetByIdAsync(Guid id)
    {
        var item = await _db.NavigationItems.FindAsync(id);
        if (item == null) return null;

        return MapToResponse(item);
    }

    public async Task<NavigationItemResponse> CreateAsync(CreateNavigationItemRequest request)
    {
        if (request.ParentId.HasValue)
        {
            var parent = await _db.NavigationItems.FindAsync(request.ParentId.Value);
            if (parent == null)
                throw new ArgumentException("父级菜单不存在");
        }

        var item = new NavigationItem
        {
            Title = request.Title,
            Url = request.Url,
            ParentId = request.ParentId,
            Icon = request.Icon,
            OpenInNewTab = request.OpenInNewTab,
            SortOrder = request.SortOrder,
            IsVisible = request.IsVisible,
            Location = request.Location
        };

        _db.NavigationItems.Add(item);
        await _db.SaveChangesAsync();

        return MapToResponse(item);
    }

    public async Task<NavigationItemResponse?> UpdateAsync(Guid id, UpdateNavigationItemRequest request)
    {
        var item = await _db.NavigationItems.FindAsync(id);
        if (item == null) return null;

        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == id)
                throw new ArgumentException("不能将自身设为父级");

            var parent = await _db.NavigationItems.FindAsync(request.ParentId.Value);
            if (parent == null)
                throw new ArgumentException("父级菜单不存在");
        }

        item.Title = request.Title;
        item.Url = request.Url;
        item.ParentId = request.ParentId;
        item.Icon = request.Icon;
        item.OpenInNewTab = request.OpenInNewTab;
        item.SortOrder = request.SortOrder;
        item.IsVisible = request.IsVisible;
        item.Location = request.Location;

        await _db.SaveChangesAsync();
        return MapToResponse(item);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var item = await _db.NavigationItems.FindAsync(id);
        if (item == null) return false;

        // 同时删除子菜单
        var children = await _db.NavigationItems.Where(n => n.ParentId == id).ToListAsync();
        _db.NavigationItems.RemoveRange(children);
        _db.NavigationItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<PublicNavigationItem>> GetPublicByLocationAsync(string location)
    {
        var items = await _db.NavigationItems
            .Where(n => n.IsVisible && (n.Location == location || n.Location == "both"))
            .OrderBy(n => n.SortOrder)
            .ToListAsync();

        return BuildPublicTree(items, null);
    }

    private List<NavigationItemResponse> BuildTree(List<NavigationItem> items, Guid? parentId)
    {
        return items
            .Where(n => n.ParentId == parentId)
            .Select(n => new NavigationItemResponse
            {
                Id = n.Id,
                Title = n.Title,
                Url = n.Url,
                ParentId = n.ParentId,
                Icon = n.Icon,
                OpenInNewTab = n.OpenInNewTab,
                SortOrder = n.SortOrder,
                IsVisible = n.IsVisible,
                Location = n.Location,
                Children = BuildTree(items, n.Id)
            })
            .ToList();
    }

    private List<PublicNavigationItem> BuildPublicTree(List<NavigationItem> items, Guid? parentId)
    {
        return items
            .Where(n => n.ParentId == parentId && n.IsVisible)
            .Select(n => new PublicNavigationItem
            {
                Title = n.Title,
                Url = n.Url,
                Icon = n.Icon,
                OpenInNewTab = n.OpenInNewTab,
                Children = BuildPublicTree(items, n.Id)
            })
            .ToList();
    }

    private static NavigationItemResponse MapToResponse(NavigationItem item)
    {
        return new NavigationItemResponse
        {
            Id = item.Id,
            Title = item.Title,
            Url = item.Url,
            ParentId = item.ParentId,
            Icon = item.Icon,
            OpenInNewTab = item.OpenInNewTab,
            SortOrder = item.SortOrder,
            IsVisible = item.IsVisible,
            Location = item.Location,
            Children = new()
        };
    }

    public async Task<bool> BatchUpdateSortAsync(List<NavigationSortItem> items)
    {
        var ids = items.Select(i => i.Id).ToList();
        var existing = await _db.NavigationItems.Where(n => ids.Contains(n.Id)).ToListAsync();

        foreach (var item in items)
        {
            var nav = existing.FirstOrDefault(n => n.Id == item.Id);
            if (nav != null)
            {
                nav.SortOrder = item.SortOrder;
                nav.ParentId = item.ParentId;
            }
        }

        await _db.SaveChangesAsync();
        return true;
    }
}