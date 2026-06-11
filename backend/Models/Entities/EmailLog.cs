namespace Mblog.API.Models.Entities;

/// <summary>
/// 邮件发送日志表，记录所有发送的邮件
/// </summary>
public class EmailLog : BaseEntity
{
    /// <summary>订阅者 ID</summary>
    public Guid? SubscriberId { get; set; }

    /// <summary>模板标识键</summary>
    public string? TemplateKey { get; set; }

    /// <summary>收件人邮箱</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>邮件主题</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>发送时间</summary>
    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>是否成功</summary>
    public bool IsSuccess { get; set; }

    /// <summary>错误信息</summary>
    public string? ErrorMessage { get; set; }
}