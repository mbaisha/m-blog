namespace Mblog.API.Models.Entities;

/// <summary>
/// 访问记录表，记录每次页面访问用于统计分析
/// </summary>
public class Visit
{
    /// <summary>主键</summary>
    public Guid Id { get; set; }

    /// <summary>被访问文章 ID，为空表示非文章页面</summary>
    public Guid? ArticleId { get; set; }

    /// <summary>访问页面路径</summary>
    public string? PagePath { get; set; }

    /// <summary>访客真实 IP 地址</summary>
    public string? IpAddress { get; set; }

    /// <summary>访客 IP 哈希（隐私保护）</summary>
    public string? IpHash { get; set; }

    /// <summary>访客地理位置（精确到城市）</summary>
    public string? Location { get; set; }

    /// <summary>访客 User-Agent</summary>
    public string? UserAgent { get; set; }

    /// <summary>来源页面 URL</summary>
    public string? Referer { get; set; }

    /// <summary>访问时间</summary>
    public DateTimeOffset VisitedAt { get; set; }

    /// <summary>被访问文章导航属性</summary>
    public Article? Article { get; set; }
}