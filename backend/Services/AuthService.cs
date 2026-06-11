using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mblog.API.Data;
using Mblog.API.Models.DTOs.Auth;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

/// <summary>
/// JWT 配置选项
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>JWT 签发者</summary>
    public string Issuer { get; set; } = "mblog";

    /// <summary>JWT 接收者</summary>
    public string Audience { get; set; } = "mblog";

    /// <summary>签名密钥（至少 32 字符）</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>访问令牌过期时间（分钟）</summary>
    public int AccessTokenExpirationMinutes { get; set; } = 120;

    /// <summary>刷新令牌过期时间（天）</summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

/// <summary>
/// 认证服务接口
/// </summary>
public interface IAuthService
{
    /// <summary>执行登录验证</summary>
    Task<LoginResponse> LoginAsync(LoginRequest request);

    /// <summary>刷新访问令牌</summary>
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>退出登录（删除刷新令牌）</summary>
    Task LogoutAsync(string refreshToken);

    /// <summary>获取当前用户信息</summary>
    Task<UserInfo> GetCurrentUserAsync(Guid userId);

    /// <summary>修改当前用户密码</summary>
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
}

/// <summary>
/// 认证服务，处理 JWT 生成、刷新、退出
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtSettings _jwt;

    public AuthService(AppDbContext db, IOptions<JwtSettings> jwt)
    {
        _db = db;
        _jwt = jwt.Value;
    }

    /// <summary>
    /// 验证用户名密码，返回 JWT 令牌对
    /// </summary>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // 查找用户（排除软删除）
        var user = await _db.Users
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Username == request.Username)
            ?? throw new UnauthorizedAccessException("用户名或密码不正确");

        // 验证密码
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("用户名或密码不正确");
        }

        // 检查账号状态
        if (user.Status != "active")
        {
            throw new UnauthorizedAccessException("账号已被禁用");
        }

        // 生成令牌对
        return await GenerateTokenPairAsync(user);
    }

    /// <summary>
    /// 使用刷新令牌获取新的令牌对
    /// </summary>
    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        // 计算哈希并查找
        var tokenHash = ComputeSha256Hash(refreshToken);
        var storedToken = await _db.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.Avatar)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash)
            ?? throw new UnauthorizedAccessException("刷新令牌无效");

        // 检查是否已使用或过期
        if (storedToken.RevokedAt != null)
        {
            // 如果令牌已被使用，可能存在令牌重放攻击，使该用户的所有刷新令牌失效
            await RevokeAllUserTokensAsync(storedToken.UserId);
            throw new UnauthorizedAccessException("刷新令牌已被使用，所有令牌已失效");
        }

        if (storedToken.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedAccessException("刷新令牌已过期");
        }

        // 标记当前令牌为已使用
        storedToken.RevokedAt = DateTimeOffset.UtcNow;
        _db.RefreshTokens.Update(storedToken);
        await _db.SaveChangesAsync();

        return await GenerateTokenPairAsync(storedToken.User);
    }

    /// <summary>
    /// 退出登录，撤销刷新令牌
    /// </summary>
    public async Task LogoutAsync(string refreshToken)
    {
        var tokenHash = ComputeSha256Hash(refreshToken);
        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (storedToken != null)
        {
            storedToken.RevokedAt = DateTimeOffset.UtcNow;
            _db.RefreshTokens.Update(storedToken);
            await _db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    public async Task<UserInfo> GetCurrentUserAsync(Guid userId)
    {
        var user = await _db.Users
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedAccessException("用户不存在");

        return new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            AvatarUrl = user.Avatar?.Url
        };
    }

    /// <summary>
    /// 修改当前用户密码
    /// </summary>
    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedAccessException("用户不存在");

        // 验证当前密码
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("当前密码不正确");
        }

        // 新密码不能为空
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            throw new ArgumentException("新密码长度不能少于 6 位");
        }

        // 更新密码
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// 为用户生成 JWT 访问令牌和刷新令牌
    /// </summary>
    private async Task<LoginResponse> GenerateTokenPairAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // 存储刷新令牌哈希
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = ComputeSha256Hash(refreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays)
        };
        _db.RefreshTokens.Add(refreshTokenEntity);
        await _db.SaveChangesAsync();

        // 清理过期令牌
        await CleanupExpiredTokensAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes),
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                AvatarUrl = user.Avatar?.Url
            }
        };
    }

    /// <summary>
    /// 生成 JWT 访问令牌
    /// </summary>
    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>生成随机刷新令牌字符串</summary>
    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>计算 SHA256 哈希</summary>
    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>使指定用户的所有刷新令牌失效</summary>
    private async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        var now = DateTimeOffset.UtcNow;
        foreach (var token in tokens)
        {
            token.RevokedAt = now;
        }
        _db.RefreshTokens.UpdateRange(tokens);
        await _db.SaveChangesAsync();
    }

    /// <summary>清理过期的已使用刷新令牌</summary>
    private async Task CleanupExpiredTokensAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var expiredTokens = await _db.RefreshTokens
            .ToListAsync();

        expiredTokens = expiredTokens.Where(rt => rt.ExpiresAt < now).ToList();

        if (expiredTokens.Count > 0)
        {
            _db.RefreshTokens.RemoveRange(expiredTokens);
            await _db.SaveChangesAsync();
        }
    }
}