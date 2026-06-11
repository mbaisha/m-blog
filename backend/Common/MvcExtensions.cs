using Microsoft.AspNetCore.Mvc;
using Mblog.API.Common;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// MVC 配置扩展方法，用于统一 ModelState 校验失败响应格式
/// </summary>
public static class MvcExtensions
{
    /// <summary>
    /// 配置 ModelState 校验失败时返回统一格式的 ApiResponse
    /// </summary>
    public static IMvcBuilder ConfigureInvalidModelStateResponse(this IMvcBuilder builder)
    {
        builder.ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .SelectMany(e => e.Value!.Errors.Select(x => x.ErrorMessage))
                    .ToList();

                var response = ApiResponse.Fail("请求参数校验失败", errors, 400);
                return new BadRequestObjectResult(response);
            };
        });

        return builder;
    }
}