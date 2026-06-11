using Microsoft.EntityFrameworkCore;
using Mblog.API.Data;

namespace Mblog.API.Services;

public interface IDashboardService
{
    /// <summary>获取仪表盘概览统计</summary>
    Task<DashboardStatsResponse> GetStatsAsync();
}

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardStatsResponse> GetStatsAsync()
    {
        var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);

        var articleCount = await _db.Articles.CountAsync(a => a.DeletedAt == null);
        var draftCount = await _db.Articles.CountAsync(a => a.Status == "draft" && a.DeletedAt == null);
        var pendingCommentCount = await _db.Comments.CountAsync(c => c.Status == "pending" && c.DeletedAt == null);
        var todayViews = await _db.Visits.CountAsync(v => v.VisitedAt >= todayStart);
        var totalViews = await _db.Visits.CountAsync();
        var mediaCount = await _db.Media.CountAsync();
        var categoryCount = await _db.Categories.CountAsync();
        var tagCount = await _db.Tags.CountAsync();

        return new DashboardStatsResponse
        {
            ArticleCount = articleCount,
            DraftCount = draftCount,
            PendingCommentCount = pendingCommentCount,
            TodayViews = todayViews,
            TotalViews = totalViews,
            MediaCount = mediaCount,
            CategoryCount = categoryCount,
            TagCount = tagCount
        };
    }
}