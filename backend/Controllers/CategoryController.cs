using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs.Category;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 分类管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/categories")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// 获取所有分类列表（按排序序号升序）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(ApiResponse.Ok(categories));
    }

    /// <summary>
    /// 获取分类平面列表（用于下拉选择，不含树形结构）
    /// </summary>
    [HttpGet("flat")]
    public async Task<IActionResult> GetFlatList()
    {
        var categories = await _categoryService.GetFlatListAsync();
        return Ok(ApiResponse.Ok(categories));
    }

    /// <summary>
    /// 根据 ID 获取分类详情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
            return NotFound(ApiResponse.Fail("分类不存在", 404));

        return Ok(ApiResponse.Ok(category));
    }

    /// <summary>
    /// 创建分类
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var category = await _categoryService.CreateAsync(request);
            return Ok(ApiResponse.Ok(category, "分类创建成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 更新分类
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var category = await _categoryService.UpdateAsync(id, request);
            if (category == null)
                return NotFound(ApiResponse.Fail("分类不存在", 404));

            return Ok(ApiResponse.Ok(category, "分类更新成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 删除分类（软删除）
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse.Fail("分类不存在", 404));

            return Ok(ApiResponse.Ok("分类删除成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 更新分类排序
    /// </summary>
    [HttpPut("{id:guid}/sort")]
    public async Task<IActionResult> UpdateSort(Guid id, [FromBody] UpdateCategorySortRequest request)
    {
        var result = await _categoryService.UpdateSortAsync(id, request.SortOrder);
        if (!result)
            return NotFound(ApiResponse.Fail("分类不存在", 404));

        return Ok(ApiResponse.Ok("排序更新成功"));
    }
}