using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 前台公开归档控制器
/// </summary>
[ApiController]
[Route("api/archive")]
public class PublicArchiveController : ControllerBase
{
    private readonly IArticleService _articleService;

    public PublicArchiveController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    /// <summary>
    /// 获取归档数据（按年月分组，倒序排列）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetArchive()
    {
        var archive = await _articleService.GetArchiveAsync();
        return Ok(ApiResponse.Ok(archive));
    }
}