namespace Mblog.API.Models.Entities;

/// <summary>
/// 留言板消息表，支持前台回复和后台回复
/// </summary>
public class Message : BaseEntity
{
    /// <summary>父消息 ID，为空表示顶级留言</summary>
    public Guid? ParentId { get; set; }

    /// <summary>留言者昵称</summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>留言者邮箱（选填）</summary>
    public string? Email { get; set; }

    /// <summary>留言正文</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>访客 IP 地址</summary>
    public string? IpAddress { get; set; }

    /// <summary>访客 IP 所在城市</summary>
    public string? IpCity { get; set; }

    /// <summary>访客 User-Agent</summary>
    public string? UserAgent { get; set; }

    /// <summary>审核状态：pending / approved / rejected</summary>
    public string Status { get; set; } = "pending";

    /// <summary>后台管理回复内容（非前台回复，是管理员直接在后台回复的）</summary>
    public string? AdminReply { get; set; }

    /// <summary>后台回复时间</summary>
    public DateTimeOffset? AdminRepliedAt { get; set; }

    /// <summary>父消息导航属性</summary>
    public Message? Parent { get; set; }

    /// <summary>子消息集合（前台回复）</summary>
    public ICollection<Message> Children { get; set; } = new List<Message>();
}
