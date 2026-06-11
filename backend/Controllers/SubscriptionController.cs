using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs.Subscription;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 订阅控制器，前台提交订阅 + 后台管理
/// </summary>
[ApiController]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IWeeklyDigestService _weeklyDigestService;

    public SubscriptionController(ISubscriptionService subscriptionService, IWeeklyDigestService weeklyDigestService)
    {
        _subscriptionService = subscriptionService;
        _weeklyDigestService = weeklyDigestService;
    }

    /// <summary>
    /// 提交订阅（公开）
    /// POST /api/subscribe
    /// </summary>
    [AllowAnonymous]
    [HttpPost("/api/subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        try
        {
            var result = await _subscriptionService.SubscribeAsync(request);
            return Ok(new { success = true, data = result, message = result.Message ?? "订阅成功" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 确认订阅（公开，通过令牌验证）
    /// GET /api/subscribe/confirm?token=xxx
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/api/subscribe/confirm")]
    public async Task<IActionResult> ConfirmSubscription([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { success = false, message = "缺少确认令牌" });

        var (success, message) = await _subscriptionService.ConfirmSubscriptionAsync(token);
        if (!success)
            return BadRequest(new { success = false, message });

        return Ok(new { success = true, message });
    }

    /// <summary>
    /// 请求退订验证码（公开）
    /// POST /api/unsubscribe/captcha
    /// </summary>
    [AllowAnonymous]
    [HttpPost("/api/unsubscribe/captcha")]
    public async Task<IActionResult> RequestUnsubscribeCaptcha([FromBody] UnsubscribeCaptchaRequest request)
    {
        var result = await _subscriptionService.RequestUnsubscribeCaptchaAsync(request.Email);
        if (result == null)
            return NotFound(new { success = false, message = "未找到匹配的订阅者" });

        return Ok(new { success = true, data = result, message = "验证码已发送至您的邮箱" });
    }

    /// <summary>
    /// 确认退订（公开，验证码验证）
    /// POST /api/unsubscribe/confirm
    /// </summary>
    [AllowAnonymous]
    [HttpPost("/api/unsubscribe/confirm")]
    public async Task<IActionResult> ConfirmUnsubscribe([FromBody] UnsubscribeConfirmRequest request)
    {
        var (success, message) = await _subscriptionService.ConfirmUnsubscribeAsync(request.SessionId, request.Code);
        if (!success)
            return BadRequest(new { success = false, message });

        return Ok(new { success = true, message });
    }

    /// <summary>
    /// 取消订阅（公开，直接使用完整邮箱）
    /// POST /api/unsubscribe
    /// </summary>
    [AllowAnonymous]
    [HttpPost("/api/unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request)
    {
        var result = await _subscriptionService.UnsubscribeByEmailAsync(request.Email);
        if (!result)
            return NotFound(new { success = false, message = "未找到该订阅" });
        return Ok(new { success = true, message = "已取消订阅" });
    }

    /// <summary>
    /// 获取订阅者列表（后台）
    /// GET /api/admin/subscribers
    /// </summary>
    [Authorize(Roles = "super_admin,admin")]
    [HttpGet("/api/admin/subscribers")]
    public async Task<IActionResult> GetSubscribers()
    {
        var subscribers = await _subscriptionService.GetSubscribersAsync();
        return Ok(new { success = true, data = subscribers });
    }

    /// <summary>
    /// 删除订阅记录（后台）
    /// DELETE /api/admin/subscribers/{id}
    /// </summary>
    [Authorize(Roles = "super_admin,admin")]
    [HttpDelete("/api/admin/subscribers/{id:guid}")]
    public async Task<IActionResult> DeleteSubscriber(Guid id)
    {
        var result = await _subscriptionService.DeleteSubscriberAsync(id);
        if (!result)
            return NotFound(new { success = false, message = "未找到该订阅记录" });
        return Ok(new { success = true, message = "已删除" });
    }

    /// <summary>
    /// 获取活跃订阅数
    /// GET /api/admin/subscribers/count
    /// </summary>
    [Authorize(Roles = "super_admin,admin")]
    [HttpGet("/api/admin/subscribers/count")]
    public async Task<IActionResult> GetCount()
    {
        var count = await _subscriptionService.GetActiveCountAsync();
        return Ok(new { success = true, data = count });
    }

    /// <summary>
    /// 补发本周周报（手动触发，全部订阅者）
    /// POST /api/admin/subscribers/resend-weekly
    /// </summary>
    [Authorize(Roles = "super_admin,admin")]
    [HttpPost("/api/admin/subscribers/resend-weekly")]
    public async Task<IActionResult> ResendWeeklyDigest()
    {
        var result = await _weeklyDigestService.ExecuteDigestAsync();
        if (!result.HasContent)
            return Ok(new { success = true, message = result.Message ?? "本周暂无内容更新" });

        return Ok(new { success = true, message = result.Message, data = result });
    }

    /// <summary>
    /// 向指定订阅者补发本周周报
    /// POST /api/admin/subscribers/{id}/resend-weekly
    /// </summary>
    [Authorize(Roles = "super_admin,admin")]
    [HttpPost("/api/admin/subscribers/{id}/resend-weekly")]
    public async Task<IActionResult> ResendWeeklyDigestForSubscriber(Guid id)
    {
        var subscriber = await _subscriptionService.GetByIdAsync(id);
        if (subscriber == null)
            return NotFound(ApiResponse.Fail("订阅者不存在", 404));

        var result = await _weeklyDigestService.ExecuteDigestForSubscriberAsync(subscriber.Email);

        if (!result.HasContent)
            return Ok(new { success = true, message = result.Message ?? "本周暂无内容更新" });

        return Ok(new { success = true, message = result.Message, data = result });
    }
}