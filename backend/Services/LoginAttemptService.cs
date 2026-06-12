using System.Collections.Concurrent;

namespace Mblog.API.Services;

/// <summary>
/// 登录失败尝试记录
/// </summary>
public class LoginAttemptInfo
{
    public int FailedCount { get; set; }
    public DateTimeOffset FirstAttemptAt { get; set; }
    public DateTimeOffset LastAttemptAt { get; set; }
    public bool LockedUntil { get; set; }
    public DateTimeOffset LockExpiresAt { get; set; }
}

/// <summary>
/// 登录尝试服务接口
/// </summary>
public interface ILoginAttemptService
{
    /// <summary>记录一次登录失败，返回是否需要验证码</summary>
    bool RecordFailure(string ipAddress);

    /// <summary>记录一次登录成功，清除失败计数</summary>
    void RecordSuccess(string ipAddress);

    /// <summary>检查 IP 是否被暂时锁定</summary>
    bool IsLocked(string ipAddress);

    /// <summary>获取指定 IP 的失败次数</summary>
    int GetFailedCount(string ipAddress);
}

/// <summary>
/// 登录尝试服务（内存实现）
/// 记录每个 IP 的登录失败次数，首次失败后要求验证码
/// </summary>
public class LoginAttemptService : ILoginAttemptService
{
    private static readonly ConcurrentDictionary<string, LoginAttemptInfo> _attempts = new();
    private const int MaxFailedAttempts = 5;         // 最多失败次数，超过则暂时锁定
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);  // 锁定时间
    private static readonly TimeSpan ResetWindow = TimeSpan.FromMinutes(30);   // 失败计数重置窗口

    /// <summary>
    /// 记录一次登录失败
    /// </summary>
    /// <returns>true 表示需要验证码</returns>
    public bool RecordFailure(string ipAddress)
    {
        var now = DateTimeOffset.UtcNow;

        var info = _attempts.GetOrAdd(ipAddress, _ => new LoginAttemptInfo
        {
            FirstAttemptAt = now
        });

        lock (info)
        {
            // 超过重置窗口，重置计数
            if (now - info.FirstAttemptAt > ResetWindow)
            {
                info.FailedCount = 0;
                info.FirstAttemptAt = now;
                info.LockedUntil = false;
            }

            info.FailedCount++;
            info.LastAttemptAt = now;

            // 超过最大失败次数，锁定 IP
            if (info.FailedCount >= MaxFailedAttempts)
            {
                info.LockedUntil = true;
                info.LockExpiresAt = now.Add(LockDuration);
            }
        }

        // 第一次失败后就需要验证码
        return info.FailedCount >= 1;
    }

    /// <summary>
    /// 记录登录成功，清除失败计数
    /// </summary>
    public void RecordSuccess(string ipAddress)
    {
        _attempts.TryRemove(ipAddress, out _);
    }

    /// <summary>
    /// 检查 IP 是否被锁定
    /// </summary>
    public bool IsLocked(string ipAddress)
    {
        if (!_attempts.TryGetValue(ipAddress, out var info))
            return false;

        lock (info)
        {
            if (info.LockedUntil && DateTimeOffset.UtcNow < info.LockExpiresAt)
                return true;

            // 锁定期已过，清除锁定状态
            if (info.LockedUntil)
            {
                info.LockedUntil = false;
                info.FailedCount = 0;
            }

            return false;
        }
    }

    /// <summary>
    /// 获取失败次数
    /// </summary>
    public int GetFailedCount(string ipAddress)
    {
        return _attempts.TryGetValue(ipAddress, out var info) ? info.FailedCount : 0;
    }
}
