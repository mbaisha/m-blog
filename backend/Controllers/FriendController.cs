using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs.Friend;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 友情链接管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/friends")]
[Authorize]
public class FriendController : ControllerBase
{
    private readonly IFriendService _friendService;

    public FriendController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    /// <summary>
    /// 获取所有友情链接
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var friends = await _friendService.GetAllAsync();
        return Ok(ApiResponse.Ok(friends));
    }

    /// <summary>
    /// 根据 ID 获取友情链接
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var friend = await _friendService.GetByIdAsync(id);
        if (friend == null)
            return NotFound(ApiResponse.Fail("友情链接不存在", 404));

        return Ok(ApiResponse.Ok(friend));
    }

    /// <summary>
    /// 创建友情链接
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFriendRequest request)
    {
        var friend = await _friendService.CreateAsync(request);
        return Ok(ApiResponse.Ok(friend, "友情链接创建成功"));
    }

    /// <summary>
    /// 更新友情链接
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFriendRequest request)
    {
        var friend = await _friendService.UpdateAsync(id, request);
        if (friend == null)
            return NotFound(ApiResponse.Fail("友情链接不存在", 404));

        return Ok(ApiResponse.Ok(friend, "友情链接更新成功"));
    }

    /// <summary>
    /// 删除友情链接
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _friendService.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail("友情链接不存在", 404));

        return Ok(ApiResponse.Ok("友情链接已删除"));
    }
}

/// <summary>
/// 友情链接公开控制器（前台）
/// </summary>
[ApiController]
[Route("api/friends")]
public class PublicFriendController : ControllerBase
{
    private readonly IFriendService _friendService;

    public PublicFriendController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    /// <summary>
    /// 获取公开友情链接列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var friends = await _friendService.GetPublicListAsync();
        return Ok(ApiResponse.Ok(friends));
    }
}