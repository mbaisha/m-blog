using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs.Project;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 项目展示管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/projects")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// 分页查询项目列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] ProjectQueryParams query)
    {
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1) query.PageSize = 20;
        if (query.PageSize > 100) query.PageSize = 100;

        var result = await _projectService.GetPagedAsync(query);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 根据 ID 获取项目详情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null)
            return NotFound(ApiResponse.Fail("项目不存在", 404));

        return Ok(ApiResponse.Ok(project));
    }

    /// <summary>
    /// 创建项目
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        try
        {
            var project = await _projectService.CreateAsync(request);
            return Ok(ApiResponse.Ok(project, "项目创建成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 更新项目
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request)
    {
        try
        {
            var project = await _projectService.UpdateAsync(id, request);
            if (project == null)
                return NotFound(ApiResponse.Fail("项目不存在", 404));

            return Ok(ApiResponse.Ok(project, "项目更新成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 删除项目
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _projectService.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail("项目不存在", 404));

        return Ok(ApiResponse.Ok("项目已删除"));
    }
}

/// <summary>
/// 项目展示公开控制器（前台）
/// </summary>
[ApiController]
[Route("api/projects")]
public class PublicProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public PublicProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// 获取公开项目列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var projects = await _projectService.GetPublicListAsync();
        return Ok(ApiResponse.Ok(projects));
    }

    /// <summary>
    /// 根据 Slug 获取项目详情
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var project = await _projectService.GetPublicBySlugAsync(slug);
        if (project == null)
            return NotFound(ApiResponse.Fail("项目不存在", 404));

        return Ok(ApiResponse.Ok(project));
    }
}