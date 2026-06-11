namespace Mblog.API.Models.Entities;

/// <summary>
/// 软删除接口，实现此接口的实体支持逻辑删除
/// 默认查询过滤器会自动排除 DeletedAt 不为空的数据
/// </summary>
public interface ISoftDeletable
{
    /// <summary>软删除时间，为空表示未删除</summary>
    DateTimeOffset? DeletedAt { get; set; }
}