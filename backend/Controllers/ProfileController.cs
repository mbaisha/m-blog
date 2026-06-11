using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs.Profile;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 个人页面配置控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// 获取所有个人页面模块
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sections = await _profileService.GetAllSectionsAsync();
        return Ok(ApiResponse.Ok(sections));
    }

    /// <summary>
    /// 批量保存个人页面模块
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> BatchSave([FromBody] List<SaveProfileSectionRequest> requests)
    {
        var sections = await _profileService.BatchSaveAsync(requests);
        return Ok(ApiResponse.Ok(sections, "个人页面配置已更新"));
    }
}

/// <summary>
/// 个人页面公开控制器（前台）
/// </summary>
[ApiController]
[Route("api/profile")]
public class PublicProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public PublicProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// 获取公开的个人页面模块
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var sections = await _profileService.GetEnabledSectionsAsync();
        return Ok(ApiResponse.Ok(sections));
    }
}