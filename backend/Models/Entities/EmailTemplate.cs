namespace Mblog.API.Models.Entities;

/// <summary>
/// 邮件模板表，存储各场景邮件模板内容
/// </summary>
public class EmailTemplate : BaseEntity
{
    /// <summary>模板标识键（confirm_subscription / subscription_detail / unsubscribed）</summary>
    public string TemplateKey { get; set; } = string.Empty;

    /// <summary>邮件主题</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>邮件 HTML 内容</summary>
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>模板描述</summary>
    public string? Description { get; set; }
}