using Mblog.API.Common;

namespace Mblog.API.Models.DTOs;

/// <summary>提交留言请求</summary>
public class CreateMessageRequest
{
    public string Nickname { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string CaptchaSessionId { get; set; } = string.Empty;
    public string CaptchaAnswer { get; set; } = string.Empty;
}

/// <summary>留言响应（树形，含子留言）</summary>
public class MessageResponse
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentNickname { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? IpCity { get; set; }
    public string Status { get; set; } = "pending";
    public string? AdminReply { get; set; }
    public DateTimeOffset? AdminRepliedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<MessageResponse> Children { get; set; } = new();
}

/// <summary>留言扁平响应（后台表格用，不含 Children 树）</summary>
public class MessageFlatResponse
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentNickname { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? IpCity { get; set; }
    public string Status { get; set; } = "pending";
    public string? AdminReply { get; set; }
    public DateTimeOffset? AdminRepliedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>后台留言审核/回复请求</summary>
public class AdminUpdateMessageRequest
{
    public string? Status { get; set; }
    public string? AdminReply { get; set; }
}

/// <summary>后台留言列表查询</summary>
public class AdminMessageQueryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
    public string? Keyword { get; set; }
}
