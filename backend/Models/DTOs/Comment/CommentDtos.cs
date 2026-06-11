using System.Text.Json.Serialization;

namespace Mblog.API.Models.DTOs.Comment;

/// <summary>
/// 验证码图片响应
/// </summary>
public class CaptchaImageResponse
{
    /// <summary>验证码会话 ID，提交时需传入</summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>Base64 编码的验证码图片（data:image/png;base64,...）</summary>
    public string ImageBase64 { get; set; } = string.Empty;
}

/// <summary>
/// 验证码校验请求
/// </summary>
public class CaptchaVerifyRequest
{
    /// <summary>验证码会话 ID</summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>用户输入的验证码答案</summary>
    public string Answer { get; set; } = string.Empty;
}

/// <summary>
/// 前台提交评论请求
/// </summary>
public class CreateCommentRequest
{
    /// <summary>所属文章 ID</summary>
    public Guid ArticleId { get; set; }

    /// <summary>父评论 ID，楼中楼回复时填写</summary>
    public Guid? ParentId { get; set; }

    /// <summary>评论者昵称</summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>评论者邮箱（可选）</summary>
    public string? Email { get; set; }

    /// <summary>评论者网站（可选）</summary>
    public string? Website { get; set; }

    /// <summary>评论正文</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>验证码会话 ID</summary>
    public string CaptchaSessionId { get; set; } = string.Empty;

    /// <summary>验证码答案</summary>
    public string CaptchaAnswer { get; set; } = string.Empty;
}

/// <summary>
/// 前台评论列表项响应
/// </summary>
public class CommentItemResponse
{
    /// <summary>评论 ID</summary>
    public Guid Id { get; set; }

    /// <summary>所属文章 ID</summary>
    public Guid ArticleId { get; set; }

    /// <summary>父评论 ID</summary>
    public Guid? ParentId { get; set; }

    /// <summary>父评论者昵称</summary>
    public string? ParentNickname { get; set; }

    /// <summary>父评论内容摘要</summary>
    public string? ParentContent { get; set; }

    /// <summary>评论者昵称</summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>评论者邮箱</summary>
    public string? Email { get; set; }

    /// <summary>评论者网站</summary>
    public string? Website { get; set; }

    /// <summary>评论正文</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>评论者头像</summary>
    public string? Avatar { get; set; }

    /// <summary>创建时间</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>管理员回复内容</summary>
    public string? ReplyContent { get; set; }
}

/// <summary>
/// 前台提交评论响应
/// </summary>
public class CreateCommentResponse
{
    /// <summary>评论审核状态</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>提示消息</summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 后台评论列表项
/// </summary>
public class AdminCommentItemResponse
{
    /// <summary>评论 ID</summary>
    public Guid Id { get; set; }

    /// <summary>所属文章 ID</summary>
    public Guid ArticleId { get; set; }

    /// <summary>文章标题</summary>
    public string ArticleTitle { get; set; } = string.Empty;

    /// <summary>父评论 ID</summary>
    public Guid? ParentId { get; set; }

    /// <summary>评论者昵称</summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>评论者邮箱</summary>
    public string? Email { get; set; }

    /// <summary>评论者网站</summary>
    public string? Website { get; set; }

    /// <summary>评论正文</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>评论者头像</summary>
    public string? Avatar { get; set; }

    /// <summary>IP 哈希（摘要显示用）</summary>
    public string IpHash { get; set; } = string.Empty;

    /// <summary>真实 IP 地址</summary>
    public string? IpAddress { get; set; }

    /// <summary>地理位置（精确到城市）</summary>
    public string? Location { get; set; }

    /// <summary>审核状态</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>是否为垃圾评论</summary>
    public bool IsSpam { get; set; }

    /// <summary>审核人 ID</summary>
    public Guid? ReviewedBy { get; set; }

    /// <summary>审核时间</summary>
    public DateTimeOffset? ReviewedAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// 后台评论查询参数
/// </summary>
public class AdminCommentQueryParams
{
    /// <summary>页码（从 1 开始）</summary>
    public int Page { get; set; } = 1;

    /// <summary>每页条数</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>按状态筛选</summary>
    public string? Status { get; set; }

    /// <summary>按文章 ID 筛选</summary>
    public Guid? ArticleId { get; set; }

    /// <summary>搜索关键词</summary>
    public string? Keyword { get; set; }
}

/// <summary>
/// 批量操作请求
/// </summary>
public class BatchCommentRequest
{
    /// <summary>评论 ID 列表</summary>
    public List<Guid> Ids { get; set; } = new();
}