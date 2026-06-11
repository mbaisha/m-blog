namespace Mblog.API.Models.DTOs;

/// <summary>
/// 邮件模板响应
/// </summary>
public class EmailTemplateResponse
{
    public Guid Id { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// 更新邮件模板请求
/// </summary>
public class UpdateEmailTemplateRequest
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? Description { get; set; }
}