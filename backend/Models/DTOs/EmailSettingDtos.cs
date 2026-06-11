namespace Mblog.API.Models.DTOs;

/// <summary>
/// 邮件设置响应
/// </summary>
public class EmailSettingResponse
{
    public Guid Id { get; set; }
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    /// <summary>密码 masked 返回</summary>
    public string SmtpPasswordMasked { get; set; } = "********";
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;
}

/// <summary>
/// 更新邮件设置请求
/// </summary>
public class UpdateEmailSettingRequest
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string? SmtpPassword { get; set; }
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;
}

/// <summary>
/// 测试邮件发送请求
/// </summary>
public class TestEmailRequest
{
    public string TestEmail { get; set; } = string.Empty;
}