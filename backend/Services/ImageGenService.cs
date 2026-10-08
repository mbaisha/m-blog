using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

/// <summary>
/// 文生图服务接口
/// </summary>
public interface IImageGenService
{
    /// <summary>获取文生图配置</summary>
    Task<ImageGenConfig?> GetConfigAsync();

    /// <summary>保存文生图配置</summary>
    Task<ImageGenConfig> SaveConfigAsync(SaveImageGenConfigRequest request);

    /// <summary>执行文生图</summary>
    Task<List<string>> GenerateAsync(ImageGenRequest request, CancellationToken ct = default);
}

/// <summary>
/// 文生图服务实现
/// </summary>
public class ImageGenService : IImageGenService
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;

    public ImageGenService(AppDbContext db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ImageGenConfig?> GetConfigAsync()
    {
        return await _db.Set<ImageGenConfig>().FirstOrDefaultAsync();
    }

    public async Task<ImageGenConfig> SaveConfigAsync(SaveImageGenConfigRequest request)
    {
        var config = await _db.Set<ImageGenConfig>().FirstOrDefaultAsync();
        if (config == null)
        {
            config = new ImageGenConfig();
            _db.Set<ImageGenConfig>().Add(config);
        }

        config.BaseUrl = request.BaseUrl;
        config.ApiKey = request.ApiKey;
        config.Model = request.Model;
        config.AvailableModels = request.AvailableModels;
        config.DefaultSize = request.DefaultSize;
        config.AvailableSizes = request.AvailableSizes;
        config.DefaultQuality = request.DefaultQuality;
        config.DefaultN = request.DefaultN;
        config.TimeoutSeconds = request.TimeoutSeconds;

        await _db.SaveChangesAsync();
        return config;
    }

    public async Task<List<string>> GenerateAsync(ImageGenRequest request, CancellationToken ct = default)
    {
        var config = await GetConfigAsync();
        if (config == null)
            throw new InvalidOperationException("文生图配置未设置，请先在系统设置中配置文生图参数");

        var apiUrl = UrlHelper.BuildApiUrl(config.BaseUrl, "/images/generations");

        // 按需构建请求体：空值字段不发，避免不支持的参数导致 400
        var body = new Dictionary<string, object?>
        {
            ["model"] = string.IsNullOrWhiteSpace(request.Model) ? config.Model : request.Model,
            ["prompt"] = request.Prompt
        };
        var n = request.N > 0 ? request.N : config.DefaultN;
        if (n > 0) body["n"] = n;
        var size = string.IsNullOrWhiteSpace(request.Size) ? config.DefaultSize : request.Size;
        if (!string.IsNullOrWhiteSpace(size)) body["size"] = size;
        var quality = string.IsNullOrWhiteSpace(request.Quality) ? config.DefaultQuality : request.Quality;
        if (!string.IsNullOrWhiteSpace(quality)) body["quality"] = quality;

        var client = _httpClientFactory.CreateClient();
        if (config.TimeoutSeconds > 0)
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, JsonOpt), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);

        var response = await client.SendAsync(httpRequest, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"文生图 API 返回错误 ({response.StatusCode}): {responseBody}");

        return ParseImageResponse(responseBody);
    }

    /// <summary>
    /// 兼容多种供应商的响应解析：支持 data[].url / data[].b64_json / data[].image_url，
    /// 以及 images/output/results/urls 等替代根字段，base64 自动转为 data URI。
    /// </summary>
    private static List<string> ParseImageResponse(string responseBody)
    {
        var urls = new List<string>();
        if (string.IsNullOrWhiteSpace(responseBody)) return urls;

        JsonElement root;
        try { root = JsonSerializer.Deserialize<JsonElement>(responseBody); }
        catch { return urls; } // 非 JSON 响应

        // 1) OpenAI 兼容格式：data 数组
        if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
                ExtractItem(item, urls);
            if (urls.Count > 0) return urls;
        }

        // 2) 替代根字段：images / output / results / urls
        foreach (var key in new[] { "images", "output", "results", "urls" })
        {
            if (root.TryGetProperty(key, out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in arr.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var s = item.GetString();
                        if (!string.IsNullOrEmpty(s)) urls.Add(s);
                    }
                    else ExtractItem(item, urls);
                }
                if (urls.Count > 0) return urls;
            }
        }

        // 3) 兜底：递归扫描所有 url / b64_json 字符串字段
        CollectUrls(root, urls);
        return urls;
    }

    private static void ExtractItem(JsonElement item, List<string> urls)
    {
        if (item.ValueKind != JsonValueKind.Object) return;

        if (item.TryGetProperty("url", out var u) && u.ValueKind == JsonValueKind.String)
        {
            var s = u.GetString();
            if (!string.IsNullOrEmpty(s)) urls.Add(s);
            return;
        }
        if (item.TryGetProperty("image_url", out var iu) && iu.ValueKind == JsonValueKind.String)
        {
            var s = iu.GetString();
            if (!string.IsNullOrEmpty(s)) urls.Add(s);
            return;
        }
        if (item.TryGetProperty("b64_json", out var b64) && b64.ValueKind == JsonValueKind.String)
        {
            var s = b64.GetString();
            if (!string.IsNullOrEmpty(s)) urls.Add(ToDataUri(s, item));
        }
    }

    private static string ToDataUri(string base64, JsonElement item)
    {
        var mime = "image/png";
        foreach (var key in new[] { "content_type", "mime_type", "content-type" })
        {
            if (item.TryGetProperty(key, out var m) && m.ValueKind == JsonValueKind.String)
            {
                var s = m.GetString();
                if (!string.IsNullOrEmpty(s)) { mime = s; break; }
            }
        }
        return $"data:{mime};base64,{base64}";
    }

    private static void CollectUrls(JsonElement element, List<string> urls)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var p in element.EnumerateObject())
                {
                    if ((p.Name == "url" || p.Name == "image_url") && p.Value.ValueKind == JsonValueKind.String)
                    {
                        var s = p.Value.GetString();
                        if (!string.IsNullOrEmpty(s)) urls.Add(s);
                    }
                    else if (p.Name == "b64_json" && p.Value.ValueKind == JsonValueKind.String)
                    {
                        var s = p.Value.GetString();
                        if (!string.IsNullOrEmpty(s)) urls.Add(ToDataUri(s, element));
                    }
                    else CollectUrls(p.Value, urls);
                }
                break;
            case JsonValueKind.Array:
                foreach (var e in element.EnumerateArray()) CollectUrls(e, urls);
                break;
        }
    }

    private static readonly JsonSerializerOptions JsonOpt = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
}