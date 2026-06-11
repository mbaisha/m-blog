using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Mblog.API.Common;

/// <summary>
/// 全局异常处理中间件，统一捕获并格式化异常响应
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// 执行中间件逻辑，捕获未处理异常并返回统一格式
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "未授权访问");
            await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, "未授权访问", ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "参数错误");
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, "参数错误", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "资源不存在");
            await HandleExceptionAsync(context, HttpStatusCode.NotFound, "资源不存在", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "服务器内部错误");
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "服务器内部错误", "请稍后重试或联系管理员");
        }
    }

    /// <summary>
    /// 将异常格式化为统一响应
    /// </summary>
    private static async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message, string detail)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            code = (int)statusCode,
            message,
            detail
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}