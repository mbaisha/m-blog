namespace Mblog.API.Models.Entities;

/// <summary>
/// 图形验证码会话表，用于评论防灌水
/// </summary>
public class CaptchaSession
{
    /// <summary>主键</summary>
    public Guid Id { get; set; }

    /// <summary>会话标识（客户端传回的 Key）</summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>验证码答案的哈希值</summary>
    public string AnswerHash { get; set; } = string.Empty;

    /// <summary>请求方 IP 哈希</summary>
    public string IpHash { get; set; } = string.Empty;

    /// <summary>过期时间</summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>使用时间，为空表示未使用</summary>
    public DateTimeOffset? UsedAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTimeOffset CreatedAt { get; set; }
}