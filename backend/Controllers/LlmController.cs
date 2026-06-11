using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;
using Mblog.API.Models.DTOs;
using Mblog.API.Services;

namespace Mblog.API.Controllers;

/// <summary>
/// 大模型管理控制器（仅管理员和超级管理员可访问）
/// </summary>
[ApiController]
[Route("api/admin/llm")]
[Authorize(Roles = "super_admin,admin")]
public class LlmController : ControllerBase
{
    private readonly ILlmService _llmService;

    public LlmController(ILlmService llmService) => _llmService = llmService;

    /// <summary>获取 LLM 配置</summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetConfig()
    {
        var config = await _llmService.GetConfigAsync();
        if (config == null) return Ok(ApiResponse.Ok(new { }));
        return Ok(ApiResponse.Ok(new
        {
            config.BaseUrl,
            config.ApiKey,
            config.Model,
            config.Mode,
            config.AvailableModels,
            config.SystemPrompt,
            config.MaxTokens,
            config.Temperature,
            config.MemoryEnabled,
            config.MaxMemoryRounds
        }));
    }

    /// <summary>保存 LLM 配置</summary>
    [HttpPut("config")]
    public async Task<IActionResult> SaveConfig([FromBody] SaveLlmConfigRequest request)
    {
        var config = await _llmService.SaveConfigAsync(request);
        return Ok(ApiResponse.Ok("LLM 配置已保存"));
    }

    /// <summary>LLM 对话（非流式）</summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] LlmChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(ApiResponse.Fail("消息不能为空"));

        if (request.Stream)
        {
            // 流式使用 Server-Sent Events
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            await foreach (var chunk in _llmService.ChatStreamAsync(request, HttpContext.RequestAborted))
            {
                await Response.WriteAsync($"data: {chunk}\n\n", HttpContext.RequestAborted);
                await Response.Body.FlushAsync(HttpContext.RequestAborted);
            }
            await Response.WriteAsync("data: [DONE]\n\n", HttpContext.RequestAborted);
            return new EmptyResult();
        }

        var result = await _llmService.ChatAsync(request);
        return Ok(ApiResponse.Ok(new { content = result, sessionId = request.SessionId }));
    }
}