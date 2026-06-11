namespace Mblog.API.Models.Entities;

/// <summary>
/// 项目展示表，用于个人作品集展示
/// </summary>
public class Project : BaseEntity, ISoftDeletable
{
    /// <summary>项目标题</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL 标识，唯一</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>项目简介</summary>
    public string? Summary { get; set; }

    /// <summary>项目详情（Markdown）</summary>
    public string? Content { get; set; }

    /// <summary>封面图媒体 ID</summary>
    public Guid? CoverImageId { get; set; }

    /// <summary>技术栈 JSON 数组</summary>
    public string TechStack { get; set; } = "[]";

    /// <summary>项目链接</summary>
    public string? ProjectUrl { get; set; }

    /// <summary>GitHub 仓库链接</summary>
    public string? GithubUrl { get; set; }

    /// <summary>在线演示链接</summary>
    public string? DemoUrl { get; set; }

    /// <summary>排序序号，越小越靠前</summary>
    public int SortOrder { get; set; }

    /// <summary>是否对外可见</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>软删除时间</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>封面图导航属性</summary>
    public Media? CoverImage { get; set; }
}