using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Models.DTOs.Comment;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

/// <summary>
/// 评论服务接口
/// </summary>
public interface ICommentService
{
    /// <summary>前台分页获取文章已审核评论</summary>
    Task<PagedResponse<CommentItemResponse>> GetApprovedAsync(Guid articleId, int page = 1, int pageSize = 20);

    /// <summary>前台提交评论</summary>
    Task<CreateCommentResponse> CreateAsync(CreateCommentRequest request, string ipAddress, string? userAgent, string? xForwardedFor = null, string? xRealIp = null);

    /// <summary>后台分页查询评论</summary>
    Task<PagedResponse<AdminCommentItemResponse>> GetAdminListAsync(AdminCommentQueryParams query);

    /// <summary>审核通过评论</summary>
    Task<bool> ApproveAsync(Guid id, Guid reviewerId);

    /// <summary>拒绝评论</summary>
    Task<bool> RejectAsync(Guid id, Guid reviewerId, string? reason = null);

    /// <summary>标记为垃圾评论</summary>
    Task<bool> MarkSpamAsync(Guid id, Guid reviewerId);

    /// <summary>删除评论（软删除）</summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>批量审核通过</summary>
    Task<int> BatchApproveAsync(List<Guid> ids, Guid reviewerId);

    /// <summary>批量删除</summary>
    Task<int> BatchDeleteAsync(List<Guid> ids);

    /// <summary>管理员回复评论</summary>
    Task<bool> ReplyAsync(Guid id, string content, Guid adminUserId);
}

/// <summary>
/// 评论服务实现，包含验证码校验、IP 频率限制、内容校验、敏感词过滤等
/// </summary>
public class CommentService : ICommentService
{
    private readonly AppDbContext _db;
    private readonly ICaptchaService _captchaService;
    private readonly ILogger<CommentService> _logger;
    private readonly HttpClient _httpClient;

    // 频率限制配置（内存中维护，生产环境建议使用 Redis 或分布式缓存）
    private static readonly Dictionary<string, List<DateTimeOffset>> _ipSubmissionTimes = new();
    private static readonly Dictionary<string, int> _ipFailCount = new();
    private static readonly object _lock = new();

    // IP 地理定位缓存
    private static readonly Dictionary<string, string?> GeoCache = new();
    private static readonly TimeSpan GeoCacheTtl = TimeSpan.FromHours(1);
    private static DateTimeOffset _lastGeoClean = DateTimeOffset.MinValue;

    // 敏感词列表（示例，生产环境可从配置文件或数据库加载）
    private static readonly string[] _sensitiveWords = { "赌博", "色情", "代开发票", "办证", "枪支", "毒品" };

    public CommentService(AppDbContext db, ICaptchaService captchaService, ILogger<CommentService> logger, HttpClient httpClient)
    {
        _db = db;
        _captchaService = captchaService;
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// 前台分页获取文章已审核评论
    /// </summary>
    public async Task<PagedResponse<CommentItemResponse>> GetApprovedAsync(Guid articleId, int page = 1, int pageSize = 20)
    {
        var query = _db.Comments
            .AsNoTracking()
            .Include(x => x.Parent)
            .Where(x => x.ArticleId == articleId && x.Status == "approved" && x.DeletedAt == null);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CommentItemResponse
            {
                Id = x.Id,
                ArticleId = x.ArticleId,
                ParentId = x.ParentId,
                ParentNickname = x.Parent != null ? x.Parent.Nickname : null,
                ParentContent = x.Parent != null ? x.Parent.Content : null,
                Nickname = x.Nickname,
                Email = x.Email,
                Website = x.Website,
                Content = x.Content,
                Avatar = x.Avatar,
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync();

        // 截断父评论内容（表达式树不支持范围运算符）
        foreach (var item in items)
        {
            if (item.ParentContent != null && item.ParentContent.Length > 50)
                item.ParentContent = item.ParentContent[..50] + "...";
        }

        return new PagedResponse<CommentItemResponse>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = items,
        };
    }

    /// <summary>
    /// 前台提交评论（含验证码校验、频率限制、内容校验）
    /// </summary>
    public async Task<CreateCommentResponse> CreateAsync(CreateCommentRequest request, string ipAddress, string? userAgent, string? xForwardedFor = null, string? xRealIp = null)
    {
        // 提取真实 IP（支持反向代理）
        var realIp = ExtractRealIp(ipAddress, xForwardedFor, xRealIp);
        var ipHash = HashIp(realIp);

        // ===== 1. 验证码校验 =====
        var captchaResult = await _captchaService.VerifyAsync(request.CaptchaSessionId, request.CaptchaAnswer, ipAddress);
        if (!captchaResult.Valid)
        {
            return new CreateCommentResponse { Status = "error", Message = captchaResult.ErrorMessage ?? "验证码错误" };
        }

        // ===== 2. IP 频率限制 =====
        var rateLimitResult = CheckRateLimit(ipHash, request.ArticleId);
        if (!rateLimitResult.Allowed)
        {
            return new CreateCommentResponse { Status = "error", Message = rateLimitResult.Message! };
        }

        // ===== 3. 基础内容校验 =====
        var validationResult = ValidateContent(request);
        if (!validationResult.IsValid)
        {
            return new CreateCommentResponse { Status = "error", Message = validationResult.ErrorMessage! };
        }

        // ===== 4. 检查文章是否存在且已发布 =====
        var article = await _db.Articles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ArticleId && x.Status == "published" && x.DeletedAt == null);
        if (article == null)
        {
            return new CreateCommentResponse { Status = "error", Message = "文章不存在或未发布" };
        }

        // ===== 5. 检查父评论是否存在（如果是回复） =====
        if (request.ParentId.HasValue)
        {
            var parentExists = await _db.Comments
                .AnyAsync(x => x.Id == request.ParentId.Value && x.ArticleId == request.ArticleId && x.DeletedAt == null);
            if (!parentExists)
            {
                return new CreateCommentResponse { Status = "error", Message = "父评论不存在" };
            }
        }

        // ===== 6. 重复内容检测 =====
        var duplicateResult = await CheckDuplicateAsync(ipHash, request.ArticleId, request.Content);
        if (duplicateResult == DuplicateAction.Reject)
        {
            return new CreateCommentResponse { Status = "error", Message = "请勿重复提交相同评论" };
        }

        // ===== 7. 判断是否需要审核 =====
        var (status, isSpam) = DetermineCommentStatus(request.Content, ipHash);

        // ===== 8. 创建评论 =====
        // 查询地理定位
        var location = await GetGeoLocationAsync(realIp);

        var comment = new Comment
        {
            ArticleId = request.ArticleId,
            ParentId = request.ParentId,
            Nickname = request.Nickname.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Website = string.IsNullOrWhiteSpace(request.Website) ? null : SanitizeUrl(request.Website.Trim()),
            Content = request.Content.Trim(),
            IpHash = ipHash,
            IpAddress = realIp,
            Location = location,
            Status = status,
            IsSpam = isSpam,
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        // 更新文章评论数（仅已审核通过的评论才计数）
        if (status == "approved")
        {
            article = await _db.Articles.FindAsync(request.ArticleId);
            if (article != null)
            {
                article.CommentCount++;
                await _db.SaveChangesAsync();
            }
        }

        _logger.LogInformation("评论已提交: ArticleId={ArticleId}, Status={Status}, IpHash={IpHash}", request.ArticleId, status, ipHash);

        var message = status == "approved" ? "评论提交成功" : "评论已提交，等待审核";
        return new CreateCommentResponse { Status = status, Message = message };
    }

    /// <summary>
    /// 后台分页查询评论
    /// </summary>
    public async Task<PagedResponse<AdminCommentItemResponse>> GetAdminListAsync(AdminCommentQueryParams query)
    {
        var q = _db.Comments
            .AsNoTracking()
            .Include(x => x.Article)
            .Where(x => x.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(x => x.Status == query.Status);
        if (query.ArticleId.HasValue)
            q = q.Where(x => x.ArticleId == query.ArticleId.Value);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.ToLower();
            q = q.Where(x => x.Nickname.ToLower().Contains(keyword)
                          || x.Content.ToLower().Contains(keyword));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new AdminCommentItemResponse
            {
                Id = x.Id,
                ArticleId = x.ArticleId,
                ArticleTitle = x.Article.Title,
                ParentId = x.ParentId,
                Nickname = x.Nickname,
                Email = x.Email,
                Website = x.Website,
                Content = x.Content,
                Avatar = x.Avatar,
                IpHash = x.IpHash.Substring(0, Math.Min(x.IpHash.Length, 16)) + "...",
                IpAddress = x.IpAddress,
                Location = x.Location,
                Status = x.Status,
                IsSpam = x.IsSpam,
                ReviewedBy = x.ReviewedBy,
                ReviewedAt = x.ReviewedAt,
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync();

        return new PagedResponse<AdminCommentItemResponse>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total,
            Items = items,
        };
    }

    /// <summary>审核通过评论</summary>
    public async Task<bool> ApproveAsync(Guid id, Guid reviewerId)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment == null || comment.DeletedAt != null)
            return false;

        comment.Status = "approved";
        comment.IsSpam = false;
        comment.ReviewedBy = reviewerId;
        comment.ReviewedAt = DateTimeOffset.UtcNow;

        // 增加文章评论数
        var article = await _db.Articles.FindAsync(comment.ArticleId);
        if (article != null)
        {
            article.CommentCount++;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("评论审核通过: CommentId={CommentId}, ReviewerId={ReviewerId}", id, reviewerId);
        return true;
    }

    /// <summary>拒绝评论</summary>
    public async Task<bool> RejectAsync(Guid id, Guid reviewerId, string? reason = null)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment == null || comment.DeletedAt != null)
            return false;

        // 如果之前是已审核状态，需要减少评论计数
        bool wasApproved = comment.Status == "approved";
        comment.Status = "rejected";
        comment.ReviewedBy = reviewerId;
        comment.ReviewedAt = DateTimeOffset.UtcNow;

        if (wasApproved)
        {
            var article = await _db.Articles.FindAsync(comment.ArticleId);
            if (article != null && article.CommentCount > 0)
            {
                article.CommentCount--;
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("评论已拒绝: CommentId={CommentId}, ReviewerId={ReviewerId}, Reason={Reason}", id, reviewerId, reason);
        return true;
    }

    /// <summary>标记为垃圾评论</summary>
    public async Task<bool> MarkSpamAsync(Guid id, Guid reviewerId)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment == null || comment.DeletedAt != null)
            return false;

        bool wasApproved = comment.Status == "approved";
        comment.Status = "spam";
        comment.IsSpam = true;
        comment.ReviewedBy = reviewerId;
        comment.ReviewedAt = DateTimeOffset.UtcNow;

        if (wasApproved)
        {
            var article = await _db.Articles.FindAsync(comment.ArticleId);
            if (article != null && article.CommentCount > 0)
            {
                article.CommentCount--;
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogWarning("评论标记为垃圾: CommentId={CommentId}, ReviewerId={ReviewerId}", id, reviewerId);
        return true;
    }

    /// <summary>删除评论（软删除）</summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment == null || comment.DeletedAt != null)
            return false;

        bool wasApproved = comment.Status == "approved";
        comment.DeletedAt = DateTimeOffset.UtcNow;
        comment.Status = "hidden";

        if (wasApproved)
        {
            var article = await _db.Articles.FindAsync(comment.ArticleId);
            if (article != null && article.CommentCount > 0)
            {
                article.CommentCount--;
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("评论已删除: CommentId={CommentId}", id);
        return true;
    }

    /// <summary>批量审核通过</summary>
    public async Task<int> BatchApproveAsync(List<Guid> ids, Guid reviewerId)
    {
        var count = 0;
        foreach (var id in ids)
        {
            if (await ApproveAsync(id, reviewerId))
                count++;
        }
        return count;
    }

    /// <summary>批量删除</summary>
    public async Task<int> BatchDeleteAsync(List<Guid> ids)
    {
        var count = 0;
        foreach (var id in ids)
        {
            if (await DeleteAsync(id))
                count++;
        }
        return count;
    }

    /// <summary>
    /// 管理员回复评论（创建一条子评论）
    /// </summary>
    public async Task<bool> ReplyAsync(Guid id, string content, Guid adminUserId)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
        if (comment == null) return false;

        var reply = new Comment
        {
            Id = Guid.NewGuid(),
            ArticleId = comment.ArticleId,
            ParentId = id,
            Nickname = "管理员",
            Email = null,
            Content = content,
            Status = "approved",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _db.Comments.Add(reply);
        await _db.SaveChangesAsync();
        return true;
    }

    #region 私有方法

    /// <summary>IP 频率限制检查</summary>
    private static RateLimitResult CheckRateLimit(string ipHash, Guid articleId)
    {
        var now = DateTimeOffset.UtcNow;

        lock (_lock)
        {
            // 连续失败检查
            if (_ipFailCount.TryGetValue(ipHash, out var failCount) && failCount >= 5)
            {
                return new RateLimitResult { Allowed = false, Message = "提交过于频繁，请稍后再试" };
            }

            if (!_ipSubmissionTimes.ContainsKey(ipHash))
                _ipSubmissionTimes[ipHash] = new List<DateTimeOffset>();

            var times = _ipSubmissionTimes[ipHash];
            // 清理 10 分钟前的记录
            times.RemoveAll(t => t < now.AddMinutes(-10));

            // 同一 IP 每分钟最多 1 次
            if (times.Any(t => t > now.AddMinutes(-1)))
                return new RateLimitResult { Allowed = false, Message = "提交过于频繁，请 1 分钟后再试" };

            // 同一 IP 每 10 分钟最多 3 次
            if (times.Count >= 3)
                return new RateLimitResult { Allowed = false, Message = "提交过于频繁，请稍后再试" };

            times.Add(now);
        }
        return new RateLimitResult { Allowed = true };
    }

    /// <summary>基础内容校验</summary>
    private static ContentValidationResult ValidateContent(CreateCommentRequest request)
    {
        // 昵称校验
        if (string.IsNullOrWhiteSpace(request.Nickname) || request.Nickname.Trim().Length > 64)
            return new ContentValidationResult { IsValid = false, ErrorMessage = "昵称长度应在 1-64 字符之间" };

        // 邮箱校验（可选）
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (!Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return new ContentValidationResult { IsValid = false, ErrorMessage = "邮箱格式不正确" };
        }

        // 网站校验（可选）
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            if (!Uri.TryCreate(request.Website, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https"))
                return new ContentValidationResult { IsValid = false, ErrorMessage = "网站 URL 格式不正确" };
        }

        // 内容校验
        var content = request.Content?.Trim() ?? "";
        if (content.Length < 5)
            return new ContentValidationResult { IsValid = false, ErrorMessage = "评论内容至少 5 个字符" };
        if (content.Length > 1000)
            return new ContentValidationResult { IsValid = false, ErrorMessage = "评论内容不能超过 1000 字符" };

        // 纯空格/纯表情等
        if (Regex.IsMatch(content, @"^\s+$"))
            return new ContentValidationResult { IsValid = false, ErrorMessage = "评论内容不能为空" };
        if (Regex.IsMatch(content, @"^[\p{So}\p{Cn}\s]+$")) // 纯表情
            return new ContentValidationResult { IsValid = false, ErrorMessage = "评论内容不能仅包含表情符号" };

        // 纯链接
        if (Regex.IsMatch(content, @"^(https?://|www\.)\S+$"))
            return new ContentValidationResult { IsValid = false, ErrorMessage = "评论内容不能仅包含链接" };

        // 大量重复字符
        if (Regex.IsMatch(content, @"(.)\1{20,}"))
            return new ContentValidationResult { IsValid = false, ErrorMessage = "评论内容包含过多重复字符" };

        // 明显脚本内容
        if (Regex.IsMatch(content, @"<script|javascript:|onerror=|onclick=", RegexOptions.IgnoreCase))
            return new ContentValidationResult { IsValid = false, ErrorMessage = "评论内容包含非法脚本" };

        // 敏感词检查
        foreach (var word in _sensitiveWords)
        {
            if (content.Contains(word))
                return new ContentValidationResult { IsValid = false, ErrorMessage = "评论包含敏感词，请修改" };
        }

        return new ContentValidationResult { IsValid = true };
    }

    /// <summary>重复内容检测</summary>
    private async Task<DuplicateAction> CheckDuplicateAsync(string ipHash, Guid articleId, string content)
    {
        var fiveMinAgo = DateTimeOffset.UtcNow.AddMinutes(-5);
        var recentComments = await _db.Comments
            .Where(x => x.IpHash == ipHash && x.ArticleId == articleId && x.CreatedAt >= fiveMinAgo)
            .ToListAsync();

        foreach (var c in recentComments)
        {
            var similarity = ComputeSimilarity(c.Content, content);
            if (similarity > 0.9)
            {
                // 第一次进入审核，第二次拒绝
                if (c.Status == "pending" || c.Status == "approved")
                    return DuplicateAction.Pending;
                else
                    return DuplicateAction.Reject;
            }
        }
        return DuplicateAction.Allow;
    }

    /// <summary>判断评论状态：所有评论默认待审核</summary>
    private static (string status, bool isSpam) DetermineCommentStatus(string content, string ipHash)
    {
        // 包含链接 → 标记疑似垃圾
        if (Regex.IsMatch(content, @"https?://|www\.[a-zA-Z0-9]"))
            return ("pending", true);

        // 包含邮箱 → 标记疑似垃圾
        if (Regex.IsMatch(content, @"[^@\s]+@[^@\s]+\.[^@\s]+"))
            return ("pending", true);

        // 包含手机号 → 标记疑似垃圾
        if (Regex.IsMatch(content, @"1[3-9]\d{9}"))
            return ("pending", true);

        // 默认一律待审核
        return ("pending", false);
    }

    /// <summary>简单的文本相似度计算（基于字符重叠）</summary>
    private static double ComputeSimilarity(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            return 0;

        var set1 = new HashSet<char>(a.ToLower().Where(char.IsLetterOrDigit));
        var set2 = new HashSet<char>(b.ToLower().Where(char.IsLetterOrDigit));

        if (set1.Count == 0 || set2.Count == 0)
            return 0;

        var intersection = set1.Intersect(set2).Count();
        var union = set1.Union(set2).Count();
        return (double)intersection / union;
    }

    /// <summary>清理 URL</summary>
    private static string? SanitizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            url = "https://" + url;

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            (uri.Scheme == "http" || uri.Scheme == "https"))
            return uri.ToString();

        return null;
    }

    /// <summary>哈希 IP</summary>
    private static string HashIp(string ip)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(ip));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    #endregion

    #region 内部类

    private class RateLimitResult
    {
        public bool Allowed { get; set; }
        public string? Message { get; set; }
    }

    private class ContentValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }

    private enum DuplicateAction
    {
        Allow,
        Pending,
        Reject,
    }

    #endregion

    #region IP 地理位置

    /// <summary>从请求头中提取真实访客 IP（支持反向代理）</summary>
    private static string? ExtractRealIp(string? remoteIp, string? xForwardedFor, string? xRealIp)
    {
        if (!string.IsNullOrWhiteSpace(xForwardedFor))
        {
            var first = xForwardedFor.Split(',')[0].Trim();
            if (System.Net.IPAddress.TryParse(first, out _))
                return first;
        }
        if (!string.IsNullOrWhiteSpace(xRealIp))
        {
            if (System.Net.IPAddress.TryParse(xRealIp, out _))
                return xRealIp;
        }
        return remoteIp;
    }

    /// <summary>查询 IP 地理位置（精确到城市）</summary>
    private async Task<string?> GetGeoLocationAsync(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "::1" || ipAddress == "127.0.0.1")
            return "本地";

        if (DateTimeOffset.UtcNow - _lastGeoClean > GeoCacheTtl)
        {
            GeoCache.Clear();
            _lastGeoClean = DateTimeOffset.UtcNow;
        }

        if (GeoCache.TryGetValue(ipAddress, out var cached))
            return cached;

        try
        {
            var response = await _httpClient.GetFromJsonAsync<IpApiResponse>(
                $"http://ip-api.com/json/{ipAddress}?fields=status,city,regionName,country&lang=zh-CN");
            if (response?.Status == "success")
            {
                var location = $"{response.Country} · {response.RegionName} · {response.City}";
                GeoCache[ipAddress] = location;
                return location;
            }
            GeoCache[ipAddress] = null;
        }
        catch { }
        return null;
    }

    private class IpApiResponse
    {
        public string Status { get; set; } = "";
        public string City { get; set; } = "";
        public string RegionName { get; set; } = "";
        public string Country { get; set; } = "";
    }

    #endregion
}