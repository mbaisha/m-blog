namespace Mblog.API.Models.DTOs;

/// <summary>
/// 保存大模型配置请求
/// </summary>
public class SaveLlmConfigRequest
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Mode { get; set; } = "chat";
    public string AvailableModels { get; set; } = "[]";
    public string? SystemPrompt { get; set; }
    public int MaxTokens { get; set; } = 4096;
    public double Temperature { get; set; } = 0.7;
    public bool MemoryEnabled { get; set; } = false;
    public int MaxMemoryRounds { get; set; } = 10;
    public int TimeoutSeconds { get; set; } = 300;
}

/// <summary>
/// LLM 对话请求
/// </summary>
public class LlmChatRequest
{
    /// <summary>用户消息内容</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>会话 ID（用于记忆模式，为空则创建新会话）</summary>
    public string? SessionId { get; set; }

    /// <summary>是否使用流式输出</summary>
    public bool Stream { get; set; } = false;
}

/// <summary>
/// AI 文章信息提取请求
/// </summary>
public class AiExtractRequest
{
    public string Content { get; set; } = string.Empty;
    public string? CurrentTitle { get; set; }
}

/// <summary>
/// AI 文章封面生成请求
/// </summary>
public class AiGenerateCoverRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// AI 润色请求
/// </summary>
public class AiPolishRequest
{
    public string Content { get; set; } = string.Empty;
    public bool GenerateImages { get; set; } = false;
    public string? AspectRatio { get; set; } = "16:9";
}

/// <summary>
/// AI 一键写文请求
/// </summary>
public class AiWriteRequest
{
    public string Inspiration { get; set; } = string.Empty;
    public bool GenerateCover { get; set; } = true;
    public bool GenerateArticleImages { get; set; } = false;
    public string? ArticleImageAspectRatio { get; set; } = "16:9";
}

/// <summary>
/// AI 文章配图生成请求（插入到编辑器光标位置）
/// </summary>
public class AiGenerateArticleImageRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string AspectRatio { get; set; } = "16:9";
}