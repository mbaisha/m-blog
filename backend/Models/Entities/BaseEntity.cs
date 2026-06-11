namespace Mblog.API.Models.Entities;

/// <summary>
/// 实体基类，包含所有业务表共有的基础字段
/// </summary>
public class BaseEntity
{
    /// <summary>主键，统一使用 UUID</summary>
    public Guid Id { get; set; }

    /// <summary>创建时间，数据库默认 now()</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>更新时间，数据库默认 now()</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}