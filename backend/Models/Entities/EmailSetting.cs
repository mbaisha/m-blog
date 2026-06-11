namespace Mblog.API.Models.Entities;

/// <summary>
/// 邮件发送设置表，存储 SMTP 发件配置（单行记录）
/// </summary>
public class EmailSetting : BaseEntity
{
    /// <summary>SMTP 服务器地址</summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>SMTP 端口</summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>SMTP 用户名</summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>SMTP 密码</summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>发件人邮箱</summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>发件人名称</summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>是否启用 SSL</summary>
    public bool UseSsl { get; set; } = true;
}