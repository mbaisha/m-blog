using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

public interface IVisitService
{
    /// <summary>记录访问（含去重逻辑）</summary>
    Task TrackAsync(TrackVisitRequest request, string? ipAddress, string? userAgent, string? xForwardedFor = null, string? xRealIp = null);

    /// <summary>分页查询访客记录（管理端）</summary>
    Task<PagedResponse<VisitListItem>> GetPagedAsync(VisitQueryParams query);

    /// <summary>获取访问统计聚合数据</summary>
    Task<VisitStatsResponse> GetStatsAsync();

    /// <summary>手动清理旧记录</summary>
    Task<CleanupResult> CleanupAsync(int retentionDays);

    /// <summary>获取指定页面的阅读量</summary>
    Task<long> GetViewCountAsync(string pageType, Guid? pageId);
}

public class VisitService : IVisitService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _httpClient;
    private static readonly TimeSpan DedupWindow = TimeSpan.FromMinutes(5);
    private static readonly SemaphoreSlim GeoLock = new(1, 1);
    private static readonly Dictionary<string, string?> GeoCache = new();
    private static readonly TimeSpan GeoCacheTtl = TimeSpan.FromHours(1);
    private static DateTimeOffset _lastGeoClean = DateTimeOffset.MinValue;

    public VisitService(AppDbContext db, HttpClient httpClient)
    {
        _db = db;
        _httpClient = httpClient;
    }

    /// <summary>
    /// 从请求头中提取真实访客 IP（支持 X-Forwarded-For / X-Real-IP）
    /// </summary>
    public static string? ExtractRealIp(string? remoteIp, string? xForwardedFor, string? xRealIp)
    {
        // X-Forwarded-For: client, proxy1, proxy2
        if (!string.IsNullOrWhiteSpace(xForwardedFor))
        {
            var first = xForwardedFor.Split(',')[0].Trim();
            if (IPAddress.TryParse(first, out _))
                return first;
        }

        // X-Real-IP
        if (!string.IsNullOrWhiteSpace(xRealIp))
        {
            if (IPAddress.TryParse(xRealIp, out _))
                return xRealIp;
        }

        return remoteIp;
    }

    /// <summary>
    /// 通过 ip-api.com 查询 IP 地理位置（精确到城市）
    /// </summary>
    private async Task<string?> GetGeoLocationAsync(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "::1" || ipAddress == "127.0.0.1")
            return "本地";

        // 清理过期缓存
        if (DateTimeOffset.UtcNow - _lastGeoClean > GeoCacheTtl)
        {
            GeoCache.Clear();
            _lastGeoClean = DateTimeOffset.UtcNow;
        }

        // 缓存命中
        if (GeoCache.TryGetValue(ipAddress, out var cached))
            return cached;

        try
        {
            var response = await _httpClient.GetFromJsonAsync<IpApiResponse>(
                $"http://ip-api.com/json/{ipAddress}?fields=status,city,regionName,country&lang=zh-CN");

            if (response?.Status == "success")
            {
                var location = $"{response.Country} · {response.RegionName} · {response.City}";
                GeoCache[ipAddress] = location;
                return location;
            }

            GeoCache[ipAddress] = null;
        }
        catch
        {
            // 网络错误不影响访问记录
        }

        return null;
    }

    private class IpApiResponse
    {
        public string Status { get; set; } = "";
        public string City { get; set; } = "";
        public string RegionName { get; set; } = "";
        public string Country { get; set; } = "";
    }

    public async Task TrackAsync(TrackVisitRequest request, string? ipAddress, string? userAgent, string? xForwardedFor = null, string? xRealIp = null)
    {
        if (string.IsNullOrWhiteSpace(request.PagePath) && request.ArticleId == null)
            return;

        // 提取真实 IP
        var realIp = ExtractRealIp(ipAddress, xForwardedFor, xRealIp);

        // 生成 IP 哈希
        var ipHash = HashIp(realIp);
        var now = DateTimeOffset.UtcNow;

        // 去重逻辑：同一 IP 访问同一路径在 5 分钟内不重复记录
        var recent = await _db.Visits
            .Where(v => v.IpHash == ipHash && v.PagePath == request.PagePath
                && v.VisitedAt > now.Add(-DedupWindow))
            .AnyAsync();

        if (recent)
            return;

        // 如果是文章访问，尝试从 Slug 获取 ArticleId
        Guid? articleId = request.ArticleId;
        if (articleId == null && !string.IsNullOrWhiteSpace(request.ArticleSlug))
        {
            var article = await _db.Articles
                .Where(a => a.Slug == request.ArticleSlug)
                .Select(a => new { a.Id })
                .FirstOrDefaultAsync();
            if (article != null)
                articleId = article.Id;
        }

        // 查询地理定位
        var location = await GetGeoLocationAsync(realIp);

        // 记录访问
        var visit = new Visit
        {
            ArticleId = articleId,
            PagePath = request.PagePath,
            IpAddress = realIp,
            IpHash = ipHash,
            Location = location,
            UserAgent = userAgent,
            Referer = request.Referer,
            VisitedAt = now
        };

        _db.Visits.Add(visit);

        // 同时增加文章的 ViewCount
        if (articleId.HasValue)
        {
            var article = await _db.Articles.FindAsync(articleId.Value);
            if (article != null)
            {
                article.ViewCount++;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<PagedResponse<VisitListItem>> GetPagedAsync(VisitQueryParams query)
    {
        var q = _db.Visits
            .AsNoTracking()
            .Include(v => v.Article)
            .AsQueryable();

        if (query.StartDate.HasValue)
            q = q.Where(v => v.VisitedAt >= query.StartDate.Value);
        if (query.EndDate.HasValue)
            q = q.Where(v => v.VisitedAt <= query.EndDate.Value);
        if (!string.IsNullOrWhiteSpace(query.PagePath))
            q = q.Where(v => v.PagePath != null && v.PagePath.Contains(query.PagePath));
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.ToLower();
            q = q.Where(v => (v.PagePath != null && v.PagePath.ToLower().Contains(kw))
                          || (v.IpHash != null && v.IpHash.Contains(kw)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(v => v.VisitedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(v => new VisitListItem
            {
                Id = v.Id,
                PagePath = v.PagePath,
                ArticleTitle = v.Article != null ? v.Article.Title : null,
                IpAddress = v.IpAddress,
                IpHash = v.IpHash,
                Location = v.Location,
                UserAgent = v.UserAgent,
                Referer = v.Referer,
                VisitedAt = v.VisitedAt
            })
            .ToListAsync();

        return new PagedResponse<VisitListItem>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total,
            Items = items
        };
    }

    public async Task<VisitStatsResponse> GetStatsAsync()
    {
        var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var yesterdayStart = todayStart.AddDays(-1);
        var thirtyDaysAgo = todayStart.AddDays(-30);

        var todayViews = await _db.Visits.CountAsync(v => v.VisitedAt >= todayStart);

        // 今日独立访客（按 IP 哈希去重）
        var todayVisitors = await _db.Visits
            .Where(v => v.VisitedAt >= todayStart && v.IpHash != null)
            .Select(v => v.IpHash)
            .Distinct()
            .CountAsync();

        var yesterdayViews = await _db.Visits.CountAsync(v =>
            v.VisitedAt >= yesterdayStart && v.VisitedAt < todayStart);

        var totalViews = await _db.Visits.CountAsync();

        var totalVisitors = await _db.Visits
            .Where(v => v.IpHash != null)
            .Select(v => v.IpHash)
            .Distinct()
            .CountAsync();

        // 热门页面 Top 10
        var popularPages = await _db.Visits
            .Where(v => v.PagePath != null)
            .GroupBy(v => v.PagePath)
            .Select(g => new PopularPageItem
            {
                PagePath = g.Key!,
                Count = g.Count()
            })
            .OrderByDescending(p => p.Count)
            .Take(10)
            .ToListAsync();

        // 近 30 天趋势
        var dailyTrend = await _db.Visits
            .Where(v => v.VisitedAt >= thirtyDaysAgo)
            .GroupBy(v => v.VisitedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Views = g.Count(),
                Visitors = g.Where(v => v.IpHash != null).Select(v => v.IpHash).Distinct().Count()
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        var trend = dailyTrend.Select(d => new DailyTrendItem
        {
            Date = d.Date.ToString("yyyy-MM-dd"),
            Views = d.Views,
            Visitors = d.Visitors
        }).ToList();

        // 设备分布：取最近 1000 条记录分类
        var recentVisits = await _db.Visits
            .Where(v => v.UserAgent != null && v.VisitedAt >= thirtyDaysAgo)
            .Select(v => v.UserAgent!)
            .Take(1000)
            .ToListAsync();

        var deviceDistribution = ClassifyDevices(recentVisits);

        // 近 30 天评论趋势
        var commentTrend = await _db.Comments
            .Where(c => c.CreatedAt >= thirtyDaysAgo)
            .GroupBy(c => c.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        var commentTrendItems = commentTrend.Select(c => new CommentTrendItem
        {
            Date = c.Date.ToString("yyyy-MM-dd"),
            Count = c.Count
        }).ToList();

        return new VisitStatsResponse
        {
            TodayViews = todayViews,
            TodayVisitors = todayVisitors,
            YesterdayViews = yesterdayViews,
            TotalViews = totalViews,
            TotalVisitors = totalVisitors,
            PopularPages = popularPages,
            DailyTrend = trend,
            DeviceDistribution = deviceDistribution,
            CommentTrend = commentTrendItems
        };
    }

    /// <summary>
    /// 根据 UserAgent 简单分类设备类型
    /// </summary>
    private static List<DeviceDistributionItem> ClassifyDevices(List<string> userAgents)
    {
        var desktop = 0L;
        var mobile = 0L;
        var tablet = 0L;
        var other = 0L;

        foreach (var ua in userAgents)
        {
            var lower = ua.ToLower();
            if (lower.Contains("tablet") || lower.Contains("ipad") || lower.Contains("playbook"))
                tablet++;
            else if (lower.Contains("mobile") || lower.Contains("android") || lower.Contains("iphone")
                  || lower.Contains("ipod") || lower.Contains("blackberry"))
                mobile++;
            else if (lower.Contains("windows") || lower.Contains("macintosh") || lower.Contains("linux")
                  || lower.Contains("cros") || lower.Contains("x11"))
                desktop++;
            else
                other++;
        }

        var list = new List<DeviceDistributionItem>();
        if (desktop > 0) list.Add(new DeviceDistributionItem { DeviceType = "桌面端", Count = desktop });
        if (mobile > 0) list.Add(new DeviceDistributionItem { DeviceType = "移动端", Count = mobile });
        if (tablet > 0) list.Add(new DeviceDistributionItem { DeviceType = "平板端", Count = tablet });
        if (other > 0) list.Add(new DeviceDistributionItem { DeviceType = "其他", Count = other });
        return list;
    }

    /// <summary>
    /// 获取指定页面的阅读量
    /// </summary>
    public async Task<long> GetViewCountAsync(string pageType, Guid? pageId)
    {
        if (pageType == "article" && pageId.HasValue)
        {
            return await _db.Articles
                .Where(a => a.Id == pageId.Value)
                .Select(a => (long)a.ViewCount)
                .FirstOrDefaultAsync();
        }

        if (!string.IsNullOrWhiteSpace(pageType) && pageId.HasValue)
        {
            return await _db.Visits
                .Where(v => v.ArticleId == pageId.Value)
                .CountAsync();
        }

        return 0;
    }

    public async Task<CleanupResult> CleanupAsync(int retentionDays)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-retentionDays);
        var toDelete = await _db.Visits
            .Where(v => v.VisitedAt < cutoff)
            .ToListAsync();

        var count = toDelete.Count;
        if (count > 0)
        {
            _db.Visits.RemoveRange(toDelete);
            await _db.SaveChangesAsync();
        }

        return new CleanupResult
        {
            DeletedCount = count,
            BeforeDate = cutoff
        };
    }

    private static string HashIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return "unknown";

        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(ip));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}