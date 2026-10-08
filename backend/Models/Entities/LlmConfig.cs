namespace Mblog.API.Models.Entities;

/// <summary>
/// 大模型配置表，存储 LLM API 连接和参数配置（单行记录）
/// </summary>
public class LlmConfig : BaseEntity
{
    /// <summary>API 基础地址（如 https://api.openai.com/v1）</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>API Key / Token</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>当前使用的模型名称</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>模式：chat（Chat Completions API）或 responses（Responses API）</summary>
    public string Mode { get; set; } = "chat";

    /// <summary>可用模型列表（JSON 数组字符串）</summary>
    public string AvailableModels { get; set; } = "[]";

    /// <summary>系统提示词</summary>
    public string? SystemPrompt { get; set; }

    /// <summary>最大 Token 数</summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>温度参数 0~2</summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>是否启用记忆对话</summary>
    public bool MemoryEnabled { get; set; } = false;

    /// <summary>记忆保留轮数（MaxMemoryRounds * 2 = 消息条数）</summary>
    public int MaxMemoryRounds { get; set; } = 10;

    /// <summary>请求大模型 API 的超时时间（秒），&lt;=0 表示不限制</summary>
    public int TimeoutSeconds { get; set; } = 300;
}