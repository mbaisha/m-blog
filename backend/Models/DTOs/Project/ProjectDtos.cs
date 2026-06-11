namespace Mblog.API.Models.DTOs.Project;

/// <summary>
/// 项目列表项
/// </summary>
public class ProjectListItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? CoverImageUrl { get; set; }
    public List<string> TechStack { get; set; } = new();
    public string? ProjectUrl { get; set; }
    public string? GithubUrl { get; set; }
    public string? DemoUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// 项目详情
/// </summary>
public class ProjectDetailResponse : ProjectListItem
{
    /// <summary>正文</summary>
    public string? Content { get; set; }

    /// <summary>封面图媒体 ID</summary>
    public Guid? CoverImageId { get; set; }

    /// <summary>更新时间</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// 创建项目请求
/// </summary>
public class CreateProjectRequest
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public Guid? CoverImageId { get; set; }
    public List<string> TechStack { get; set; } = new();
    public string? ProjectUrl { get; set; }
    public string? GithubUrl { get; set; }
    public string? DemoUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}

/// <summary>
/// 更新项目请求
/// </summary>
public class UpdateProjectRequest
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public Guid? CoverImageId { get; set; }
    public List<string> TechStack { get; set; } = new();
    public string? ProjectUrl { get; set; }
    public string? GithubUrl { get; set; }
    public string? DemoUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}

/// <summary>
/// 项目查询参数
/// </summary>
public class ProjectQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Keyword { get; set; }
    public bool? IsVisible { get; set; }
}

/// <summary>
/// 前台公开项目列表项
/// </summary>
public class PublicProjectListItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? CoverImageUrl { get; set; }
    public List<string> TechStack { get; set; } = new();
    public string? ProjectUrl { get; set; }
    public string? GithubUrl { get; set; }
    public string? DemoUrl { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// 前台公开项目详情
/// </summary>
public class PublicProjectDetailResponse : PublicProjectListItem
{
    public string? Content { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}