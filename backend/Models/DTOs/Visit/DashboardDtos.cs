// ===== 仪表盘统计 =====

public class DashboardStatsResponse
{
    /// <summary>文章总数</summary>
    public long ArticleCount { get; set; }

    /// <summary>草稿数量</summary>
    public long DraftCount { get; set; }

    /// <summary>待审核评论数</summary>
    public long PendingCommentCount { get; set; }

    /// <summary>今日访问量</summary>
    public long TodayViews { get; set; }

    /// <summary>总访问量</summary>
    public long TotalViews { get; set; }

    /// <summary>媒体文件数量</summary>
    public long MediaCount { get; set; }

    /// <summary>分类数量</summary>
    public long CategoryCount { get; set; }

    /// <summary>标签数量</summary>
    public long TagCount { get; set; }
}

// ===== 设备分布项 =====
public class DeviceDistributionItem
{
    public string DeviceType { get; set; } = string.Empty;
    public long Count { get; set; }
}

// ===== 评论趋势项 =====
public class CommentTrendItem
{
    public string Date { get; set; } = string.Empty;
    public long Count { get; set; }
}