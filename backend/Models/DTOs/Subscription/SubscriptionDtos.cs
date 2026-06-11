using System.ComponentModel.DataAnnotations;

namespace Mblog.API.Models.DTOs.Subscription;

/// <summary>
/// 订阅请求
/// </summary>
public class SubscribeRequest
{
    /// <summary>订阅邮箱</summary>
    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// 订阅响应
/// </summary>
public class SubscribeResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset SubscribedAt { get; set; }
    /// <summary>是否需要确认（双次确认）</summary>
    public bool NeedsConfirmation { get; set; }
    /// <summary>确认消息</summary>
    public string? Message { get; set; }
}

/// <summary>
/// 订阅者列表项（后台管理用）
/// </summary>
public class SubscriberListItem
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset SubscribedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? UnsubscribedAt { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
}

/// <summary>
/// 确认订阅请求
/// </summary>
public class ConfirmSubscriptionRequest
{
    [Required(ErrorMessage = "令牌不能为空")]
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// 取消订阅请求（完整邮箱）
/// </summary>
public class UnsubscribeRequest
{
    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// 退订验证码请求
/// </summary>
public class UnsubscribeCaptchaRequest
{
    [Required(ErrorMessage = "邮箱不能为空")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// 退订验证码响应
/// </summary>
public class UnsubscribeCaptchaResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string MaskedEmail { get; set; } = string.Empty;
}

/// <summary>
/// 退订确认请求
/// </summary>
public class UnsubscribeConfirmRequest
{
    [Required(ErrorMessage = "会话ID不能为空")]
    public string SessionId { get; set; } = string.Empty;

    [Required(ErrorMessage = "验证码不能为空")]
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// 退订会话（临时存储）
/// </summary>
public class UnsubscribeSession
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
}