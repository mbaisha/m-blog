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
        config.DefaultQuality = request.DefaultQuality;
        config.DefaultN = request.DefaultN;

        await _db.SaveChangesAsync();
        return config;
    }

    public async Task<List<string>> GenerateAsync(ImageGenRequest request, CancellationToken ct = default)
    {
        var config = await GetConfigAsync();
        if (config == null)
            throw new InvalidOperationException("文生图配置未设置，请先在系统设置中配置文生图参数");

        var apiUrl = UrlHelper.BuildApiUrl(config.BaseUrl, "/images/generations");

        var requestBody = new
        {
            model = request.Model ?? config.Model,
            prompt = request.Prompt,
            n = request.N > 0 ? request.N : config.DefaultN,
            size = request.Size ?? config.DefaultSize,
            quality = request.Quality ?? config.DefaultQuality
        };

        var client = _httpClientFactory.CreateClient();
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);

        var response = await client.SendAsync(httpRequest, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"文生图 API 返回错误 ({response.StatusCode}): {responseBody}");

        var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
        var urls = new List<string>();

        if (result.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                if (item.TryGetProperty("url", out var url))
                    urls.Add(url.GetString() ?? "");
            }
        }

        return urls;
    }
}