using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 导航菜单管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/navigation")]
[Authorize]
public class NavigationController : ControllerBase
{
    private readonly INavigationService _navigationService;

    public NavigationController(INavigationService navigationService) => _navigationService = navigationService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _navigationService.GetAllAsync();
        return Ok(ApiResponse.Ok(items));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _navigationService.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse.Fail("菜单项不存在", 404));
        return Ok(ApiResponse.Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNavigationItemRequest request)
    {
        try
        {
            var item = await _navigationService.CreateAsync(request);
            return Ok(ApiResponse.Ok(item, "菜单项创建成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNavigationItemRequest request)
    {
        try
        {
            var item = await _navigationService.UpdateAsync(id, request);
            if (item == null) return NotFound(ApiResponse.Fail("菜单项不存在", 404));
            return Ok(ApiResponse.Ok(item, "菜单项更新成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _navigationService.DeleteAsync(id);
        if (!result) return NotFound(ApiResponse.Fail("菜单项不存在", 404));
        return Ok(ApiResponse.Ok("菜单项已删除"));
    }

    /// <summary>
    /// 批量更新排序（拖拽排序后保存）
    /// </summary>
    [HttpPut("sort")]
    public async Task<IActionResult> BatchUpdateSort([FromBody] List<NavigationSortItem> items)
    {
        await _navigationService.BatchUpdateSortAsync(items);
        return Ok(ApiResponse.Ok("排序已更新"));
    }
}

/// <summary>
/// 导航菜单公开控制器（前台）
/// </summary>
[ApiController]
[Route("api/navigation")]
public class PublicNavigationController : ControllerBase
{
    private readonly INavigationService _navigationService;

    public PublicNavigationController(INavigationService navigationService) => _navigationService = navigationService;

    [HttpGet("{location}")]
    public async Task<IActionResult> GetByLocation(string location)
    {
        var items = await _navigationService.GetPublicByLocationAsync(location);
        return Ok(ApiResponse.Ok(items));
    }
}