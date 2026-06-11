namespace Mblog.API.Models.Entities;

/// <summary>
/// 审计日志表，记录管理员操作行为用于安全审计
/// </summary>
public class AuditLog
{
    /// <summary>主键</summary>
    public Guid Id { get; set; }

    /// <summary>操作用户 ID</summary>
    public Guid? UserId { get; set; }

    /// <summary>操作动作描述，如 article.create / article.delete</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>操作资源类型，如 article / category / comment</summary>
    public string? ResourceType { get; set; }

    /// <summary>操作资源 ID</summary>
    public Guid? ResourceId { get; set; }

    /// <summary>操作者客户端 IP</summary>
    public string? Ip { get; set; }

    /// <summary>操作者 User-Agent</summary>
    public string? UserAgent { get; set; }

    /// <summary>操作详情 JSON（记录操作前后的数据变化等）</summary>
    public string Details { get; set; } = "{}";

    /// <summary>操作时间</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>操作用户导航属性</summary>
    public User? User { get; set; }
}