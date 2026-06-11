using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs.Tag;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 标签管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/tags")]
[Authorize]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    /// <summary>
    /// 获取所有标签列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tags = await _tagService.GetAllAsync();
        return Ok(ApiResponse.Ok(tags));
    }

    /// <summary>
    /// 根据 ID 获取标签详情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tag = await _tagService.GetByIdAsync(id);
        if (tag == null)
            return NotFound(ApiResponse.Fail("标签不存在", 404));

        return Ok(ApiResponse.Ok(tag));
    }

    /// <summary>
    /// 创建标签
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
    {
        try
        {
            var tag = await _tagService.CreateAsync(request);
            return Ok(ApiResponse.Ok(tag, "标签创建成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 更新标签
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTagRequest request)
    {
        try
        {
            var tag = await _tagService.UpdateAsync(id, request);
            if (tag == null)
                return NotFound(ApiResponse.Fail("标签不存在", 404));

            return Ok(ApiResponse.Ok(tag, "标签更新成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 删除标签（软删除）
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _tagService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse.Fail("标签不存在", 404));

            return Ok(ApiResponse.Ok("标签删除成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}