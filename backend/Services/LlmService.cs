using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

/// <summary>
/// LLM 服务接口
/// </summary>
public interface ILlmService
{
    /// <summary>获取 LLM 配置</summary>
    Task<LlmConfig?> GetConfigAsync();

    /// <summary>保存 LLM 配置</summary>
    Task<LlmConfig> SaveConfigAsync(SaveLlmConfigRequest request);

    /// <summary>LLM 对话（非流式）</summary>
    Task<string> ChatAsync(LlmChatRequest request, CancellationToken ct = default);

    /// <summary>LLM 流式对话</summary>
    IAsyncEnumerable<string> ChatStreamAsync(LlmChatRequest request, CancellationToken ct = default);
}

/// <summary>
/// LLM 服务实现，支持 Chat Completions 和 Responses 两种模式，流式/非流式，记忆/非记忆对话
/// </summary>
public class LlmService : ILlmService
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>会话记忆存储（SessionId -> 消息列表）</summary>
    private static readonly ConcurrentDictionary<string, List<ChatMessage>> _sessions = new();

    public LlmService(AppDbContext db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<LlmConfig?> GetConfigAsync()
    {
        return await _db.Set<LlmConfig>().FirstOrDefaultAsync();
    }

    public async Task<LlmConfig> SaveConfigAsync(SaveLlmConfigRequest request)
    {
        var config = await _db.Set<LlmConfig>().FirstOrDefaultAsync();
        if (config == null)
        {
            config = new LlmConfig();
            _db.Set<LlmConfig>().Add(config);
        }

        config.BaseUrl = request.BaseUrl;
        config.ApiKey = request.ApiKey;
        config.Model = request.Model;
        config.Mode = request.Mode;
        config.AvailableModels = request.AvailableModels;
        config.SystemPrompt = request.SystemPrompt;
        config.MaxTokens = request.MaxTokens;
        config.Temperature = request.Temperature;
        config.MemoryEnabled = request.MemoryEnabled;
        config.MaxMemoryRounds = request.MaxMemoryRounds;

        await _db.SaveChangesAsync();
        return config;
    }

    public async Task<string> ChatAsync(LlmChatRequest request, CancellationToken ct = default)
    {
        var config = await GetConfigAsync();
        if (config == null)
            throw new InvalidOperationException("LLM 配置未设置，请先在系统设置中配置大模型参数");

        var messages = BuildMessages(config, request);
        var apiUrl = GetApiUrl(config);
        var requestBody = BuildRequestBody(config, messages, stream: false);

        var client = _httpClientFactory.CreateClient();client.Timeout = TimeSpan.FromMinutes(5); // 设置更长的超时时间以适应慢响应
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);

        var response = await client.SendAsync(httpRequest, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"LLM API 返回错误 ({response.StatusCode}): {responseBody}");

        var result = JsonSerializer.Deserialize<JsonElement>(responseBody);

        // 根据模式解析响应
        string content;
        if (config.Mode == "responses")
        {
            content = result.GetProperty("output").GetProperty("choices")[0].GetProperty("content").GetString() ?? "";
        }
        else
        {
            content = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }

        // 记忆模式：保存对话历史
        if (config.MemoryEnabled)
            SaveMessages(config, request.SessionId ?? Guid.NewGuid().ToString(), request.Message, content);

        return content;
    }

    public async IAsyncEnumerable<string> ChatStreamAsync(LlmChatRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var config = await GetConfigAsync();
        if (config == null)
            throw new InvalidOperationException("LLM 配置未设置");

        var messages = BuildMessages(config, request);
        var apiUrl = GetApiUrl(config);
        var requestBody = BuildRequestBody(config, messages, stream: true);

        var client = _httpClientFactory.CreateClient();
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);

        var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"LLM API 返回错误 ({response.StatusCode}): {errorBody}");
        }

        var fullContent = new StringBuilder();
        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrEmpty(line)) continue;
            if (!line.StartsWith("data: ")) continue;

            var data = line[6..];
            if (data == "[DONE]") break;

            var delta = ParseStreamDelta(data, config.Mode);
            if (!string.IsNullOrEmpty(delta))
            {
                fullContent.Append(delta);
                yield return delta;
            }
        }

        // 记忆模式：保存完整对话
        if (config.MemoryEnabled)
            SaveMessages(config, request.SessionId ?? Guid.NewGuid().ToString(), request.Message, fullContent.ToString());
    }

    /// <summary>获取 API URL</summary>
    private string GetApiUrl(LlmConfig config)
    {
        var path = config.Mode == "responses" ? "/responses" : "/chat/completions";
        return UrlHelper.BuildApiUrl(config.BaseUrl, path);
    }

    /// <summary>构建消息列表（含记忆）</summary>
    private List<ChatMessage> BuildMessages(LlmConfig config, LlmChatRequest request)
    {
        var messages = new List<ChatMessage>();

        // 系统提示词
        if (!string.IsNullOrWhiteSpace(config.SystemPrompt))
            messages.Add(new ChatMessage { Role = "system", Content = config.SystemPrompt });

        // 记忆模式：加载历史消息
        var sessionId = request.SessionId ?? Guid.NewGuid().ToString();
        if (config.MemoryEnabled && _sessions.TryGetValue(sessionId, out var history))
        {
            var maxMessages = config.MaxMemoryRounds * 2;
            var recentHistory = history.Count > maxMessages
                ? history.Skip(history.Count - maxMessages).ToList()
                : history;
            messages.AddRange(recentHistory);
        }

        // 用户当前消息
        messages.Add(new ChatMessage { Role = "user", Content = request.Message });

        return messages;
    }

    /// <summary>构建请求体 JSON</summary>
    private string BuildRequestBody(LlmConfig config, List<ChatMessage> messages, bool stream)
    {
        using var doc = JsonDocument.Parse("{}");
        var dict = new Dictionary<string, object>
        {
            ["model"] = config.Model,
            ["stream"] = stream
        };

        if (config.Mode == "responses")
        {
            dict["input"] = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray();
        }
        else
        {
            dict["messages"] = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray();
            dict["max_tokens"] = config.MaxTokens;
            dict["temperature"] = config.Temperature;
        }

        return JsonSerializer.Serialize(dict);
    }

    /// <summary>解析流式 SSE 数据块中的 delta 文本</summary>
    private static string? ParseStreamDelta(string data, string mode)
    {
        try
        {
            var chunk = JsonSerializer.Deserialize<JsonElement>(data);
            if (mode == "responses")
            {
                if (chunk.TryGetProperty("output", out var output) &&
                    output.TryGetProperty("choices", out var choices) &&
                    choices.GetArrayLength() > 0)
                    return choices[0].TryGetProperty("content", out var c) ? c.GetString() : null;
            }
            else
            {
                if (chunk.TryGetProperty("choices", out var choices2) && choices2.GetArrayLength() > 0)
                    return choices2[0].TryGetProperty("delta", out var d) && d.TryGetProperty("content", out var c2) ? c2.GetString() : null;
            }
        }
        catch { }
        return null;
    }

    /// <summary>保存对话到记忆</summary>
    private void SaveMessages(LlmConfig config, string sessionId, string userMessage, string assistantMessage)
    {
        var messages = _sessions.GetOrAdd(sessionId, _ => new List<ChatMessage>());
        messages.Add(new ChatMessage { Role = "user", Content = userMessage });
        messages.Add(new ChatMessage { Role = "assistant", Content = assistantMessage });
    }
}

/// <summary>
/// 对话消息
/// </summary>
public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}