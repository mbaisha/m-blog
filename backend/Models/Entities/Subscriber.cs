namespace Mblog.API.Models.Entities;

/// <summary>
/// 订阅者表，存储邮件订阅用户信息
/// </summary>
public class Subscriber : BaseEntity
{
    /// <summary>订阅邮箱，唯一</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>订阅时间</summary>
    public DateTimeOffset SubscribedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>是否活跃（取消订阅后置为 false）</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>取消订阅时间</summary>
    public DateTimeOffset? UnsubscribedAt { get; set; }

    /// <summary>确认订阅令牌</summary>
    public string? ConfirmationToken { get; set; }

    /// <summary>确认订阅时间（为空表示未确认）</summary>
    public DateTimeOffset? ConfirmedAt { get; set; }

    /// <summary>令牌过期时间</summary>
    public DateTimeOffset? TokenExpiresAt { get; set; }
}