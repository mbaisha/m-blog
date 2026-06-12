using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 数据导入控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/import")]
[Authorize]
public class ImportController : ControllerBase
{
    private readonly ImportService _importService;
    private readonly ILogger<ImportController> _logger;

    public ImportController(ImportService importService, ILogger<ImportController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    /// <summary>
    /// 上传并导入备份文件（支持 .json 和 .json.zip）
    /// </summary>
    /// <remarks>
    /// 导入流程：解析文件 → 清空数据库 → 批量插入 → 返回报告
    /// </remarks>
    [HttpPost("upload")]
    [RequestSizeLimit(200 * 1024 * 1024)] // 200MB 上限
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("请选择要导入的文件"));

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".json" && ext != ".zip")
            return BadRequest(ApiResponse.Fail("仅支持 .json 或 .zip 文件"));

        _logger.LogInformation("开始导入文件: {FileName}, 大小: {Size} bytes", file.FileName, file.Length);

        await using var stream = file.OpenReadStream();
        var report = await _importService.ImportAsync(stream, file.FileName);

        if (report.Success)
            return Ok(ApiResponse.Ok(report, $"数据导入成功，共 {report.SuccessCount} 条记录"));
        else
            return Ok(ApiResponse.Ok(report, $"数据导入部分完成，成功 {report.SuccessCount} 条，错误 {report.Errors.Count} 个"));
    }
}
