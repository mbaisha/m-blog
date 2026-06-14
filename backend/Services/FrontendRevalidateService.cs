using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Mblog.API.Services;

/// <summary>
/// 后端在内容变更后通知 Next.js 失效对应 tag/path 的缓存。
/// 通过环境变量 FRONTEND_REVALIDATE_URL（如 http://frontend:3000/api/revalidate）
/// 和 FRONTEND_REVALIDATE_SECRET 鉴权。
/// </summary>
public class FrontendRevalidateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FrontendRevalidateService> _logger;
    private readonly IConfiguration _configuration;

    public FrontendRevalidateService(
        IHttpClientFactory httpClientFactory,
        ILogger<FrontendRevalidateService> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// 触发 Next.js 端 revalidate。
    /// 任何一个参数为空时直接跳过（不抛错），保证业务主流程不被缓存失效影响。
    /// </summary>
    public async Task RevalidateAsync(IEnumerable<string>? tags = null, IEnumerable<string>? paths = null)
    {
        var tagList = tags?.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList() ?? new List<string>();
        var pathList = paths?.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList() ?? new List<string>();
        if (tagList.Count == 0 && pathList.Count == 0) return;

        var url = _configuration["Frontend:RevalidateUrl"];
        var secret = _configuration["Frontend:RevalidateSecret"];
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(secret))
        {
            // 未配置说明未启用该特性，安静退出即可
            return;
        }

        try
        {
            var payload = new
            {
                secret,
                tags = tagList,
                paths = pathList,
            };
            using var http = _httpClientFactory.CreateClient(nameof(FrontendRevalidateService));
            http.Timeout = TimeSpan.FromSeconds(5);
            using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var resp = await http.PostAsync(url, content);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                _logger.LogWarning("[FrontendRevalidate] Failed {Status} {Body}", (int)resp.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            // 缓存失效失败不应该影响主业务
            _logger.LogWarning(ex, "[FrontendRevalidate] Exception while calling {Url}", url);
        }
    }

    /// <summary>单文章失效：tag = article-{slug} + articles-list，path = /articles/{slug}</summary>
    public Task RevalidateArticleAsync(string? slug) =>
        RevalidateAsync(
            tags: new[] { $"article-{slug}", "articles-list" },
            paths: slug == null ? null : new[] { $"/articles/{slug}" });
}
