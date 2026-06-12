using System.Collections.Concurrent;

namespace Mblog.API.Middleware;

/// <summary>
/// IP 访问记录
/// </summary>
public class IpAccessRecord
{
    /// <summary>请求时间戳队列（用于统计窗口内的请求数）</summary>
    public Queue<DateTimeOffset> Timestamps { get; set; } = new();

    /// <summary>是否被拉黑</summary>
    public bool IsBanned { get; set; }

    /// <summary>拉黑到期时间</summary>
    public DateTimeOffset BanExpiresAt { get; set; }
}

/// <summary>
/// IP 黑名单配置（运行时从 SiteSetting 刷新）
/// </summary>
public class IpBanOptions
{
    /// <summary>窗口内请求次数阈值，超过则拉黑（默认 200）</summary>
    public int Threshold { get; set; } = 200;

    /// <summary>统计窗口（秒），默认 10 秒</summary>
    public int WindowSeconds { get; set; } = 10;

    /// <summary>拉黑时长（分钟），默认 5 分钟</summary>
    public int BanDurationMinutes { get; set; } = 5;
}

/// <summary>
/// IP 黑名单中间件
/// 内存存储，不查数据库，只拦截 API 请求
/// </summary>
public class IpBanMiddleware
{
    private static readonly ConcurrentDictionary<string, IpAccessRecord> _records = new();
    private static IpBanOptions _options = new();
    private static bool _enabled = true;
    private static readonly object _lock = new();
    private static DateTimeOffset _lastCleanup = DateTimeOffset.UtcNow;

    private readonly RequestDelegate _next;

    public IpBanMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// 运行时更新配置（由站点设置保存后调用）
    /// </summary>
    public static void UpdateOptions(int threshold, int windowSeconds, int banDurationMinutes, bool enabled = true)
    {
        lock (_lock)
        {
            _enabled = enabled;
            _options = new IpBanOptions
            {
                Threshold = threshold > 0 ? threshold : 200,
                WindowSeconds = windowSeconds > 0 ? windowSeconds : 10,
                BanDurationMinutes = banDurationMinutes > 0 ? banDurationMinutes : 5
            };
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 如果未启用，直接放行
        if (!_enabled)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? "";

        // 只拦截 API 路由，跳过静态资源
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // 跳过高频低价值的 API（访问追踪、验证码等），不计入黑名单计数
        if (ShouldSkipCounting(path))
        {
            await _next(context);
            return;
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // 检查是否被拉黑
        if (IsBanned(ip, out var banExpiresAt))
        {
            var retryAfterSeconds = (int)Math.Ceiling((banExpiresAt - DateTimeOffset.UtcNow).TotalSeconds);
            if (retryAfterSeconds < 0) retryAfterSeconds = 0;
            context.Response.StatusCode = 429;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(
                $"{{\"code\":429,\"message\":\"访问太频繁，已被拒绝访问，请 {_options.BanDurationMinutes} 分钟后再试\",\"retryAfterSeconds\":{retryAfterSeconds}}}");
            return;
        }

        // 记录本次访问
        RecordAccess(ip);

        await _next(context);
    }

    /// <summary>
    /// 检查 IP 是否被拉黑
    /// </summary>
    private static bool IsBanned(string ip, out DateTimeOffset banExpiresAt)
    {
        banExpiresAt = DateTimeOffset.MinValue;
        if (!_records.TryGetValue(ip, out var record))
            return false;

        CleanupExpired();

        if (!record.IsBanned)
            return false;

        if (DateTimeOffset.UtcNow >= record.BanExpiresAt)
        {
            // 拉黑到期，移除记录
            _records.TryRemove(ip, out _);
            return false;
        }

        banExpiresAt = record.BanExpiresAt;
        return true;
    }

    /// <summary>
    /// 记录一次访问，如果超过阈值则拉黑
    /// </summary>
    private static void RecordAccess(string ip)
    {
        var now = DateTimeOffset.UtcNow;
        var options = _options; // 快照读取

        var record = _records.GetOrAdd(ip, _ => new IpAccessRecord());

        lock (record)
        {
            // 如果已被拉黑且未到期，不再处理
            if (record.IsBanned && now < record.BanExpiresAt)
                return;

            // 清理窗口外的过期时间戳
            while (record.Timestamps.Count > 0 &&
                   now - record.Timestamps.Peek() > TimeSpan.FromSeconds(options.WindowSeconds))
            {
                record.Timestamps.Dequeue();
            }

            // 加入当前时间戳
            record.Timestamps.Enqueue(now);

            // 检查是否超过阈值
            if (record.Timestamps.Count >= options.Threshold)
            {
                record.IsBanned = true;
                record.BanExpiresAt = now.AddMinutes(options.BanDurationMinutes);
            }
        }
    }

    /// <summary>
    /// 判断是否跳过计数的高频低价值 API
    /// </summary>
    private static bool ShouldSkipCounting(string path)
    {
        // 访问追踪、验证码、健康检查等不计入黑名单
        return path.StartsWith("/api/visits/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/captcha/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/health", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/settings", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 定期清理过期记录（每 60 秒触发一次）
    /// </summary>
    private static void CleanupExpired()
    {
        var now = DateTimeOffset.UtcNow;
        if (now - _lastCleanup < TimeSpan.FromSeconds(60))
            return;

        _lastCleanup = now;

        var expiredIps = _records
            .Where(kv =>
            {
                var r = kv.Value;
                // 已到期且未被拉黑 或 拉黑已到期
                return (!r.IsBanned && r.Timestamps.Count == 0) ||
                       (r.IsBanned && now >= r.BanExpiresAt);
            })
            .Select(kv => kv.Key)
            .ToList();

        foreach (var ip in expiredIps)
        {
            _records.TryRemove(ip, out _);
        }
    }

    /// <summary>获取当前被拉黑的 IP 数量（调试用）</summary>
    public static int GetBannedCount()
    {
        CleanupExpired();
        return _records.Count(kv => kv.Value.IsBanned);
    }
}
