namespace Mblog.API.Common;

/// <summary>
/// API URL 辅助工具
/// </summary>
public static class UrlHelper
{
    /// <summary>
    /// 规范化 Base URL，自动补全 /v1 后缀。
    /// 如果 URL 不以 /v1 结尾，则自动追加 /v1。
    /// 如果 URL 已经是完整的 API 地址（如 xxx.com/v1/chat/completions），不做处理。
    /// </summary>
    public static string NormalizeBaseUrl(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL 不能为空");

        var url = baseUrl.TrimEnd('/');

        // 如果已经以 /v1 结尾，直接返回
        if (url.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
            return url;

        // 自动补全 /v1
        return url + "/v1";
    }

    /// <summary>
    /// 构建完整的 API 地址
    /// </summary>
    /// <param name="baseUrl">基础地址（可为完整地址或需补全 /v1 的地址）</param>
    /// <param name="path">API 路径，如 "/chat/completions"</param>
    public static string BuildApiUrl(string baseUrl, string path)
    {
        var normalized = NormalizeBaseUrl(baseUrl);
        return normalized.TrimEnd('/') + "/" + path.TrimStart('/');
    }
}