// ===== 访问上报 =====

public class TrackVisitRequest
{
    /// <summary>页面路径，如 /articles/my-post</summary>
    public string? PagePath { get; set; }

    /// <summary>文章 ID（如果访问的是文章页）</summary>
    public Guid? ArticleId { get; set; }

    /// <summary>文章 Slug（如果访问的是文章页）</summary>
    public string? ArticleSlug { get; set; }

    /// <summary>来源页面</summary>
    public string? Referer { get; set; }
}

// ===== 访客记录查询 =====

public class VisitQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? PagePath { get; set; }
    public string? Keyword { get; set; }
}

public class VisitListItem
{
    public Guid Id { get; set; }
    public string? PagePath { get; set; }
    public string? ArticleTitle { get; set; }
    public string? IpAddress { get; set; }
    public string? IpHash { get; set; }
    public string? Location { get; set; }
    public string? UserAgent { get; set; }
    public string? Referer { get; set; }
    public DateTimeOffset VisitedAt { get; set; }
}

// ===== 统计数据 =====

public class VisitStatsResponse
{
    public long TodayViews { get; set; }
    public long TodayVisitors { get; set; }
    public long YesterdayViews { get; set; }
    public long TotalViews { get; set; }
    public long TotalVisitors { get; set; }

    /// <summary>热门页面 Top N</summary>
    public List<PopularPageItem> PopularPages { get; set; } = new();

    /// <summary>近 30 天趋势</summary>
    public List<DailyTrendItem> DailyTrend { get; set; } = new();

    /// <summary>设备分布</summary>
    public List<DeviceDistributionItem> DeviceDistribution { get; set; } = new();

    /// <summary>近 30 天评论趋势</summary>
    public List<CommentTrendItem> CommentTrend { get; set; } = new();
}

public class PopularPageItem
{
    public string PagePath { get; set; } = string.Empty;
    public long Count { get; set; }
}

public class DailyTrendItem
{
    public string Date { get; set; } = string.Empty;
    public long Views { get; set; }
    public long Visitors { get; set; }
}

// ===== 清理 =====

public class CleanupRequest
{
    /// <summary>保留天数，默认 30</summary>
    public int RetentionDays { get; set; } = 30;
}

public class CleanupResult
{
    public int DeletedCount { get; set; }
    public DateTimeOffset BeforeDate { get; set; }
}