using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs.Media;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 上传与媒体管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/upload")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public UploadController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    /// <summary>
    /// 上传图片（支持 jpg/png/gif/webp/svg，最大 10MB）
    /// </summary>
    [HttpPost("image")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("请选择要上传的文件"));

        var userId = GetCurrentUserId();

        try
        {
            var media = await _mediaService.UploadImageAsync(file, userId);
            return Ok(ApiResponse.Ok(media, "图片上传成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 上传封面图（前端已裁剪为 16:9，此处仅保存原图）
    /// </summary>
    [HttpPost("cover")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadCover(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("请选择要上传的文件"));

        var userId = GetCurrentUserId();

        try
        {
            // 验证文件扩展名
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExts.Contains(ext))
                throw new ArgumentException("封面图仅支持 jpg/png/webp 格式");

            // 直接保存原图（前端已完成 16:9 裁剪）
            var media = await _mediaService.UploadImageAsync(file, userId);
            return Ok(ApiResponse.Ok(media, "封面图上传成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 上传视频（支持 mp4/webm/mov/avi/mkv，最大 500MB）
    /// </summary>
    [HttpPost("video")]
    [RequestSizeLimit(500 * 1024 * 1024)]
    public async Task<IActionResult> UploadVideo(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("请选择要上传的视频"));

        var userId = GetCurrentUserId();

        try
        {
            var media = await _mediaService.UploadVideoAsync(file, userId);
            return Ok(ApiResponse.Ok(media, "视频上传成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 上传附件（支持 pdf/doc/zip 等，最大 100MB）
    /// </summary>
    [HttpPost("file")]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("请选择要上传的文件"));

        var userId = GetCurrentUserId();

        try
        {
            var media = await _mediaService.UploadFileAsync(file, userId);
            return Ok(ApiResponse.Ok(media, "文件上传成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 从 JWT Claims 中获取当前用户 ID
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("无法获取当前用户信息");

        return userId;
    }
}

/// <summary>
/// 媒体文件管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    /// <summary>
    /// 分页查询媒体列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] MediaQueryParams query)
    {
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1) query.PageSize = 30;
        if (query.PageSize > 100) query.PageSize = 100;

        var result = await _mediaService.GetPagedAsync(query);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 删除媒体文件
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediaService.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail("媒体文件不存在", 404));

        return Ok(ApiResponse.Ok("媒体文件已删除"));
    }
}