using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.Entities;
using SkiaSharp;

namespace Mblog.API.Services;

/// <summary>
/// 文章 AI 辅助服务接口
/// </summary>
public interface IArticleAiService
{
    /// <summary>AI 提取文章信息（标题、摘要、SEO）</summary>
    Task<ArticleExtractResult> ExtractInfoAsync(string content, string? currentTitle = null);

    /// <summary>AI 生成文章封面图</summary>
    Task<ArticleCoverResult> GenerateCoverAsync(string title, string content, Guid userId);

    /// <summary>AI 润色文章内容，可选自动配图</summary>
    Task<ArticlePolishResult> PolishAsync(string content, bool generateImages, string? aspectRatio, Guid userId);

    /// <summary>AI 一键写文：灵感 -> 完整文章 + 标题/简介/SEO/封面</summary>
    Task<ArticleWriteResult> WriteAsync(string inspiration, bool generateCover, bool generateArticleImages, string? articleImageAspectRatio, Guid userId);

    /// <summary>AI 生成文章内配图（插入编辑器光标位置）</summary>
    Task<ArticleCoverResult> GenerateArticleImageAsync(string prompt, string aspectRatio, Guid userId);

    /// <summary>根据比例获取对应图片尺寸</summary>
    static string GetSizeForAspectRatio(string aspectRatio) => aspectRatio switch
    {
        "4:3" => "2368x1760",
        "3:4" => "1760x2368",
        "16:9" => "2752x1536",
        "9:16" => "1536x2752",
        "1:1" => "2048x2048",
        _ => "2752x1536"
    };
}

/// <summary>
/// AI 提取结果
/// </summary>
public class ArticleExtractResult
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string SeoTitle { get; set; } = string.Empty;
    public string SeoDescription { get; set; } = string.Empty;
    public string SeoKeywords { get; set; } = string.Empty;
}

/// <summary>
/// AI 封面生成结果
/// </summary>
public class ArticleCoverResult
{
    public Guid MediaId { get; set; }
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// AI 润色结果
/// </summary>
public class ArticlePolishResult
{
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// AI 一键写文结果
/// </summary>
public class ArticleWriteResult
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string SeoTitle { get; set; } = string.Empty;
    public string SeoDescription { get; set; } = string.Empty;
    public string SeoKeywords { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string? CoverMediaId { get; set; }
}

/// <summary>
/// AI 一键写文 JSON 解析中间对象
/// </summary>
internal class ArticleWriteRaw
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string SeoTitle { get; set; } = string.Empty;
    public string SeoDescription { get; set; } = string.Empty;
    public string SeoKeywords { get; set; } = string.Empty;
    public string CoverPrompt { get; set; } = string.Empty;
}

/// <summary>
/// 文章 AI 辅助服务实现
/// </summary>
public class ArticleAiService : IArticleAiService
{
    private readonly ILlmService _llmService;
    private readonly IImageGenService _imageGenService;
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _storageRoot;

    public ArticleAiService(
        ILlmService llmService,
        IImageGenService imageGenService,
        AppDbContext db,
        IWebHostEnvironment env,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _llmService = llmService;
        _imageGenService = imageGenService;
        _db = db;
        _env = env;
        _httpClientFactory = httpClientFactory;
        _storageRoot = configuration.GetValue<string>("Storage:RootPath") ?? "./uploads";
    }

    public async Task<ArticleExtractResult> ExtractInfoAsync(string content, string? currentTitle = null)
    {
        var titleHint = string.IsNullOrEmpty(currentTitle) ? "" : $"\n当前文章标题：{currentTitle}";
        var prompt = $@"请分析以下文章内容，提取核心信息，以纯 JSON 格式返回（不要包含 ```json 等 markdown 标记）：

{{
  ""title"": ""精炼的文章标题（15字以内）"",
  ""summary"": ""简洁的内容摘要，150字以内"",
  ""seoTitle"": ""SEO 优化的标题，可适当加入关键词"",
  ""seoDescription"": ""SEO 描述，120-160字，包含核心关键词"",
  ""seoKeywords"": ""3-5个核心关键词，逗号分隔""
}}
{titleHint}
文章内容：
{content}";

        var request = new LlmChatRequest
        {
            Message = prompt,
            SessionId = null // 不使用记忆
        };

        var result = await _llmService.ChatAsync(request);
        return ParseExtractResult(result);
    }

    public async Task<ArticleCoverResult> GenerateCoverAsync(string title, string content, Guid userId)
    {
        // Step 1: 使用 LLM 生成文生图提示词
        var imagePrompt = await GenerateImagePrompt(title, content);

        // Step 2: 调用文生图服务生成图片 (16:9 -> 2752x1536)
        var genRequest = new ImageGenRequest
        {
            Prompt = imagePrompt,
            Size = "2752x1536", // 16:9
            N = 1
        };

        List<string> urls;
        try
        {
            urls = await _imageGenService.GenerateAsync(genRequest);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"AI 生成封面失败，生图服务繁忙: {ex.Message}", ex);
        }

        if (urls.Count == 0)
            throw new InvalidOperationException("AI 生成封面失败：未返回图片");

        var imageUrl = urls[0];

        // Step 3: 下载生成的图片并保存到 media 库
        return await DownloadAndSaveImage(imageUrl, userId);
    }

    private async Task<string> GenerateImagePrompt(string title, string content)
    {
        var prompt = $@"请根据以下文章内容，生成一个中文的 AI 文生图提示词，要求：
- 图片适合作为博客文章封面
- 16:9 横向宽屏比例
- 风格现代、简洁、有设计感
- 不要包含文字
- 提示词描述清楚，对于复杂的内容一定要说明清楚
- 只返回提示词本身，不要加任何前缀或说明
- 使用中文回复

文章标题：{title}
文章内容：{content}";

        var request = new LlmChatRequest
        {
            Message = prompt,
            SessionId = null
        };

        return (await _llmService.ChatAsync(request)).Trim();
    }

    private async Task<ArticleCoverResult> DownloadAndSaveImage(string imageUrl, Guid userId)
    {
        var client = _httpClientFactory.CreateClient();
        var imageBytes = await client.GetByteArrayAsync(imageUrl);

        // 推断扩展名
        var ext = ".png";
        var contentType = "image/png";
        // 尝试从 URL 或响应头推断
        if (imageUrl.Contains(".jpg") || imageUrl.Contains(".jpeg")) { ext = ".jpg"; contentType = "image/jpeg"; }
        else if (imageUrl.Contains(".webp")) { ext = ".webp"; contentType = "image/webp"; }

        var safeFilename = $"{Guid.NewGuid()}{ext}";
        var dateDir = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var relativePath = Path.Combine("images", dateDir);
        var storageDir = Path.Combine(_storageRoot, relativePath);
        var fullDir = Path.Combine(_env.ContentRootPath, storageDir);

        if (!Directory.Exists(fullDir))
            Directory.CreateDirectory(fullDir);

        var fullPath = Path.Combine(fullDir, safeFilename);
        await File.WriteAllBytesAsync(fullPath, imageBytes);

        var url = $"/uploads/{relativePath}/{safeFilename}".Replace("\\", "/");

        var media = new Media
        {
            Type = "image",
            Url = url,
            Filename = safeFilename,
            OriginalFilename = safeFilename,
            StoragePath = fullPath,
            SizeBytes = imageBytes.Length,
            MimeType = contentType,
            CreatedBy = userId
        };

        _db.Media.Add(media);
        await _db.SaveChangesAsync();

        return new ArticleCoverResult
        {
            MediaId = media.Id,
            Url = url
        };
    }

    private static ArticleExtractResult ParseExtractResult(string raw)
    {
        // 清理可能存在的 markdown 代码块标记
        var json = raw.Trim();
        if (json.StartsWith("```"))
        {
            var end = json.LastIndexOf("```", StringComparison.Ordinal);
            if (end > 3)
            {
                json = json[3..end].Trim();
                // 去掉可能的 "json" 标记
                if (json.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                    json = json[4..].Trim();
            }
        }

        try
        {
            var doc = JsonDocument.Parse(json);
            return new ArticleExtractResult
            {
                Title = GetString(doc, "title"),
                Summary = GetString(doc, "summary"),
                SeoTitle = GetString(doc, "seoTitle"),
                SeoDescription = GetString(doc, "seoDescription"),
                SeoKeywords = GetString(doc, "seoKeywords")
            };
        }
        catch
        {
            return new ArticleExtractResult();
        }
    }

    private static string GetString(JsonDocument doc, string prop)
    {
        return doc.RootElement.TryGetProperty(prop, out var v) ? v.GetString() ?? "" : "";
    }

    // ==================== AI 润色 ====================

    public async Task<ArticlePolishResult> PolishAsync(string content, bool generateImages, string? aspectRatio, Guid userId)
    {
        var imageInstruction = generateImages
            ? @"在需要配图的段落之间，插入 [IMAGE_GEN: 英文图片描述]，AI 会自动替你决定哪些段落之后适合配图。
配图规则：
- 每隔 2-4 个段落插入一张配图比较合适
- 配图描述用英文，风格为现代插画/摄影风格
- 在一个有意义的段落转折或新主题开始处配图"
            : "";

        var prompt = $@"请润色以下 Markdown 文章内容，要求：
- 优化表达，使内容更流畅自然
- 修正语法和拼写错误
- 保持原有的 Markdown 结构和格式
- 不要改变文章的核心内容和观点
- 如果段落结构不佳，可以适当重组
- 保留文章中所有已有的图片（![](...) 格式），不能删除或替换
{imageInstruction}

请直接返回润色后的完整 Markdown，不要加任何前缀说明。

{content}";

        var request = new LlmChatRequest { Message = prompt, SessionId = null };
        var result = await _llmService.ChatAsync(request);

        if (!generateImages)
            return new ArticlePolishResult { Content = result };

        // 带配图模式：解析 [IMAGE_GEN: ...] 标记并生成图片
        return await ReplaceImageMarkers(result, userId, aspectRatio ?? "16:9");
    }

    private async Task<ArticlePolishResult> ReplaceImageMarkers(string text, Guid userId, string aspectRatio = "16:9")
    {
        var size = IArticleAiService.GetSizeForAspectRatio(aspectRatio);
        var markers = new List<(int index, int length, string prompt)>();
        var searchStart = 0;

        while (true)
        {
            var start = text.IndexOf("[IMAGE_GEN:", searchStart, StringComparison.OrdinalIgnoreCase);
            if (start < 0) break;

            var promptStart = start + 11;
            var end = text.IndexOf(']', promptStart);
            if (end < 0) break;

            var prompt = text[promptStart..end].Trim();
            markers.Add((start, end - start + 1, prompt));
            searchStart = end + 1;
        }

        if (markers.Count == 0)
            return new ArticlePolishResult { Content = text };

        // 从后往前替换，避免索引偏移
        var parts = new List<string>();
        var lastEnd = 0;

        foreach (var (index, length, prompt) in markers)
        {
            parts.Add(text[lastEnd..index]);
            parts.Add($"[IMAGE:{prompt}]");
            lastEnd = index + length;
        }
        parts.Add(text[lastEnd..]);

        var textWithPlaceholders = string.Join("", parts);

        // 逐个生成图片
        for (var i = 0; i < markers.Count; i++)
        {
            var placeholder = $"[IMAGE:{markers[i].prompt}]";
            try
            {
                var genRequest = new ImageGenRequest
                {
                    Prompt = markers[i].prompt,
                    Size = size,
                    N = 1
                };
                var urls = await _imageGenService.GenerateAsync(genRequest);
                if (urls.Count > 0)
                {
                    var media = await DownloadAndSaveImageAsMedia(urls[0], userId);
                    var md = $"\n\n![{markers[i].prompt}]({media.Url})\n\n";
                    textWithPlaceholders = textWithPlaceholders.Replace(placeholder, md);
                }
                else
                {
                    textWithPlaceholders = textWithPlaceholders.Replace(placeholder, "");
                }
            }
            catch
            {
                textWithPlaceholders = textWithPlaceholders.Replace(placeholder, "");
            }
        }

        return new ArticlePolishResult { Content = textWithPlaceholders };
    }

    // ==================== AI 一键写文 ====================

    public async Task<ArticleWriteResult> WriteAsync(string inspiration, bool generateCover, bool generateArticleImages, string? articleImageAspectRatio, Guid userId)
    {
        var aspectRatio = articleImageAspectRatio ?? "16:9";
        var imageInstruction = generateArticleImages
            ? $@"7. 在需要配图的段落之间，插入 [IMAGE_GEN: 英文图片描述]，AI 会自动替你决定哪些段落之后适合配图。
配图规则：
- 每隔 2-4 个段落插入一张配图比较合适
- 配图描述用英文，风格为现代插画/摄影风格
- 在一个有意义的段落转折或新主题开始处配图
- 配图比例为 {aspectRatio}"
            : "";

        var prompt = $@"你是一个专业的博客作者。请根据以下灵感/想法，撰写一篇完整的博客文章。

要求：
1. 生成一个吸引人的文章标题（15字以内）
2. 生成一段简洁的摘要（150字以内）
3. 生成 SEO 信息（标题、描述、关键词）
4. 撰写完整的文章内容（Markdown 格式，至少 500 字）
5. 文章结构清晰，有引言、正文分段、结尾
6. 内容要有深度和可读性
{imageInstruction}

请以 JSON 格式返回（不要包含 markdown 代码块标记）：
{{
  ""title"": ""文章标题"",
  ""summary"": ""文章摘要"",
  ""content"": ""完整的 Markdown 文章内容"",
  ""seoTitle"": ""SEO 标题"",
  ""seoDescription"": ""SEO 描述，120-160字"",
  ""seoKeywords"": ""关键词1,关键词2,关键词3"",
  ""coverPrompt"": ""适合作为文章封面的英文文生图提示词""
}}

灵感/想法：
{inspiration}";

        var request = new LlmChatRequest { Message = prompt, SessionId = null };
        var result = await _llmService.ChatAsync(request);
        var d = ParseArticleWriteJson(result);

        var content = d.Content;

        // 可选生成文章内配图
        if (generateArticleImages)
        {
            var polishResult = await ReplaceImageMarkers(content, userId, aspectRatio);
            content = polishResult.Content;
        }

        var writeResult = new ArticleWriteResult
        {
            Title = d.Title,
            Summary = d.Summary,
            Content = content,
            SeoTitle = d.SeoTitle,
            SeoDescription = d.SeoDescription,
            SeoKeywords = d.SeoKeywords
        };

        // 可选生成封面图
        if (generateCover && !string.IsNullOrWhiteSpace(d.CoverPrompt))
        {
            try
            {
                var coverResult = await GenerateCoverAsync(d.Title, inspiration, userId);
                writeResult.CoverUrl = coverResult.Url;
                writeResult.CoverMediaId = coverResult.MediaId.ToString();
            }
            catch
            {
                // 封面生成失败不影响整体
            }
        }

        return writeResult;
    }

    private static ArticleWriteRaw ParseArticleWriteJson(string raw)
    {
        var json = raw.Trim();
        if (json.StartsWith("```"))
        {
            var end = json.LastIndexOf("```", StringComparison.Ordinal);
            if (end > 3)
            {
                json = json[3..end].Trim();
                if (json.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                    json = json[4..].Trim();
            }
        }

        try
        {
            var doc = JsonDocument.Parse(json);
            return new ArticleWriteRaw
            {
                Title = GetString(doc, "title"),
                Summary = GetString(doc, "summary"),
                Content = GetString(doc, "content"),
                SeoTitle = GetString(doc, "seoTitle"),
                SeoDescription = GetString(doc, "seoDescription"),
                SeoKeywords = GetString(doc, "seoKeywords"),
                CoverPrompt = GetString(doc, "coverPrompt")
            };
        }
        catch
        {
            return new ArticleWriteRaw();
        }
    }

    // ==================== AI 文章内配图（编辑器插入） ====================

    public async Task<ArticleCoverResult> GenerateArticleImageAsync(string prompt, string aspectRatio, Guid userId)
    {
        var size = IArticleAiService.GetSizeForAspectRatio(aspectRatio);

        var genRequest = new ImageGenRequest
        {
            Prompt = prompt,
            Size = size,
            N = 1
        };

        List<string> urls;
        try
        {
            urls = await _imageGenService.GenerateAsync(genRequest);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"AI 生图服务繁忙，请稍后重试: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"AI 生图失败: {ex.Message}", ex);
        }

        if (urls.Count == 0)
            throw new InvalidOperationException("AI 生成配图失败：未返回图片");

        // 下载图片并可能压缩
        byte[] imageBytes;
        try
        {
            var client = _httpClientFactory.CreateClient();
            imageBytes = await client.GetByteArrayAsync(urls[0]);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"图片下载失败: {ex.Message}", ex);
        }

        // 如果图片超过 2MB，进行压缩
        try
        {
            if (imageBytes.Length > 2 * 1024 * 1024)
            {
                imageBytes = CompressImage(imageBytes);
            }
        }
        catch
        {
            // 压缩失败不影响整体流程，使用原图
        }

        var ext = ".png";
        var contentType = "image/png";
        if (urls[0].Contains(".jpg") || urls[0].Contains(".jpeg")) { ext = ".jpg"; contentType = "image/jpeg"; }
        else if (urls[0].Contains(".webp")) { ext = ".webp"; contentType = "image/webp"; }

        var safeFilename = $"{Guid.NewGuid()}{ext}";
        var dateDir = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var relativePath = Path.Combine("images", dateDir);
        var storageDir = Path.Combine(_storageRoot, relativePath);
        var fullDir = Path.Combine(_env.ContentRootPath, storageDir);

        if (!Directory.Exists(fullDir))
            Directory.CreateDirectory(fullDir);

        var fullPath = Path.Combine(fullDir, safeFilename);
        await File.WriteAllBytesAsync(fullPath, imageBytes);

        var url = $"/uploads/{relativePath}/{safeFilename}".Replace("\\", "/");

        var media = new Media
        {
            Type = "image",
            Url = url,
            Filename = safeFilename,
            OriginalFilename = safeFilename,
            StoragePath = fullPath,
            SizeBytes = imageBytes.Length,
            MimeType = contentType,
            CreatedBy = userId
        };

        _db.Media.Add(media);
        await _db.SaveChangesAsync();

        return new ArticleCoverResult
        {
            MediaId = media.Id,
            Url = url
        };
    }

    /// <summary>使用 SkiaSharp 压缩图片（质量 80%，最大宽度 1920px）</summary>
    private static byte[] CompressImage(byte[] imageBytes)
    {
        using var inputStream = new SKMemoryStream(imageBytes);
        using var codec = SKCodec.Create(inputStream);
        if (codec == null) return imageBytes;

        var info = new SKImageInfo(codec.Info.Width, codec.Info.Height);
        using var bitmap = SKBitmap.Decode(codec);
        if (bitmap == null) return imageBytes;

        // 如果宽度超过 1920px，等比缩放
        var maxWidth = 1920;
        if (bitmap.Width > maxWidth)
        {
            var ratio = (float)maxWidth / bitmap.Width;
            var newWidth = maxWidth;
            var newHeight = (int)(bitmap.Height * ratio);
            using var resized = bitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.Medium);
            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
            return data.ToArray();
        }

        using var image2 = SKImage.FromBitmap(bitmap);
        using var data2 = image2.Encode(SKEncodedImageFormat.Jpeg, 80);
        return data2.ToArray();
    }

    private async Task<ArticleCoverResult> DownloadAndSaveImageAsMedia(string imageUrl, Guid userId)
    {
        return await DownloadAndSaveImage(imageUrl, userId);
    }
}