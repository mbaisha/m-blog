using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Models.Entities;

namespace Mblog.API.Controllers;

/// <summary>
/// 邮件发送日志管理控制器（管理员）
/// </summary>
[ApiController]
[Route("api/admin/email-logs")]
[Authorize(Roles = "super_admin,admin")]
public class EmailLogController : ControllerBase
{
    private readonly AppDbContext _db;

    public EmailLogController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>获取邮件发送日志（分页）</summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? email = null,
        [FromQuery] bool? isSuccess = null,
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null)
    {
        var query = _db.Set<EmailLog>().AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(l => l.Email.Contains(email));

        if (isSuccess.HasValue)
            query = query.Where(l => l.IsSuccess == isSuccess.Value);

        if (DateTimeOffset.TryParse(startDate, out var start))
            query = query.Where(l => l.SentAt >= start);

        if (DateTimeOffset.TryParse(endDate, out var end))
            query = query.Where(l => l.SentAt <= end);

        query = query.OrderByDescending(l => l.SentAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new
            {
                l.Id,
                l.Email,
                l.Subject,
                l.TemplateKey,
                l.SentAt,
                l.IsSuccess,
                l.ErrorMessage
            })
            .ToListAsync();

        var paged = new PagedResponse<object>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items.Cast<object>().ToList()
        };

        return Ok(ApiResponse<object>.Ok(paged));
    }

    /// <summary>删除发送日志</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var log = await _db.Set<EmailLog>().FindAsync(id);
        if (log == null)
            return NotFound(ApiResponse.Fail("日志不存在"));

        _db.Set<EmailLog>().Remove(log);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse.Ok(new { }, "日志已删除"));
    }

    /// <summary>清空所有发送日志</summary>
    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var logs = await _db.Set<EmailLog>().ToListAsync();
        _db.Set<EmailLog>().RemoveRange(logs);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse.Ok(new { }, $"已清空 {logs.Count} 条日志"));
    }
}