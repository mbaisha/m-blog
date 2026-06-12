using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Mblog.API.Data;
using Mblog.API.Models.Entities;
using SkiaSharp;

namespace Mblog.API.Services;

/// <summary>
/// 验证码服务接口
/// </summary>
public interface ICaptchaService
{
    /// <summary>生成验证码图片，返回会话 ID 和 Base64 图片</summary>
    Task<CaptchaResult> GenerateAsync(string ipAddress);

    /// <summary>生成滑块验证码</summary>
    Task<SliderCaptchaResult> GenerateSliderAsync(string ipAddress);

    /// <summary>校验验证码</summary>
    Task<CaptchaVerifyResult> VerifyAsync(string sessionId, string answer, string ipAddress);

    /// <summary>校验滑块验证码</summary>
    Task<CaptchaVerifyResult> VerifySliderAsync(string sessionId, double percent, string ipAddress);
}

/// <summary>验证码生成结果</summary>
public class CaptchaResult
{
    public string SessionId { get; set; } = string.Empty;
    public string ImageBase64 { get; set; } = string.Empty;
}

/// <summary>验证码校验结果</summary>
public class CaptchaVerifyResult
{
    public bool Valid { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>滑块验证码生成结果</summary>
public class SliderCaptchaResult
{
    public string SessionId { get; set; } = string.Empty;
    public double TargetPercent { get; set; }
}

/// <summary>
/// 验证码服务实现（使用 SkiaSharp 生成图形验证码）
/// </summary>
public class CaptchaService : ICaptchaService
{
    private readonly AppDbContext _db;
    private const int CodeLength = 4;                // 验证码长度
    private const int ImageWidth = 120;               // 图片宽度
    private const int ImageHeight = 40;               // 图片高度
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(5); // 有效期5分钟
    private const int MaxVerifyAttempts = 3;          // 最大校验次数

    public CaptchaService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 生成验证码图片
    /// </summary>
    public async Task<CaptchaResult> GenerateAsync(string ipAddress)
    {
        // 1. 生成随机验证码（纯数字，易于识别）
        var code = GenerateRandomCode(CodeLength);
        var ipHash = HashIp(ipAddress);

        // 2. 生成会话 ID
        var sessionId = Guid.NewGuid().ToString("N");

        // 3. 用 SkiaSharp 绘制验证码图片
        var imageBase64 = DrawCaptchaImage(code);

        // 4. 保存到数据库
        var session = new CaptchaSession
        {
            SessionId = sessionId,
            AnswerHash = HashAnswer(code),
            IpHash = ipHash,
            ExpiresAt = DateTimeOffset.UtcNow.Add(Expiration),
        };
        _db.CaptchaSessions.Add(session);
        await _db.SaveChangesAsync();

        return new CaptchaResult
        {
            SessionId = sessionId,
            ImageBase64 = imageBase64,
        };
    }

    /// <summary>
    /// 校验验证码
    /// </summary>
    public async Task<CaptchaVerifyResult> VerifyAsync(string sessionId, string answer, string ipAddress)
    {
        var ipHash = HashIp(ipAddress);

        var session = await _db.CaptchaSessions
            .FirstOrDefaultAsync(x => x.SessionId == sessionId);

        if (session == null)
            return new CaptchaVerifyResult { Valid = false, ErrorMessage = "验证码会话不存在" };

        if (session.UsedAt != null)
            return new CaptchaVerifyResult { Valid = false, ErrorMessage = "验证码已使用" };

        if (session.ExpiresAt < DateTimeOffset.UtcNow)
            return new CaptchaVerifyResult { Valid = false, ErrorMessage = "验证码已过期" };

        if (session.IpHash != ipHash)
            return new CaptchaVerifyResult { Valid = false, ErrorMessage = "IP 不匹配" };

        // 校验答案
        var answerHash = HashAnswer(answer.Trim().ToUpperInvariant());
        if (session.AnswerHash != answerHash)
        {
            return new CaptchaVerifyResult { Valid = false, ErrorMessage = "验证码错误" };
        }

        // 标记为已使用
        session.UsedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        return new CaptchaVerifyResult { Valid = true };
    }

    /// <summary>生成随机数字验证码</summary>
    private static string GenerateRandomCode(int length)
    {
        const string chars = "0123456789";
        var data = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(data);
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[data[i] % chars.Length];
        }
        return new string(result);
    }

    /// <summary>哈希验证码答案（SHA256）</summary>
    private static string HashAnswer(string answer)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(answer));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// 生成滑块验证码
    /// </summary>
    public async Task<SliderCaptchaResult> GenerateSliderAsync(string ipAddress)
    {
        var ipHash = HashIp(ipAddress);
        var sessionId = Guid.NewGuid().ToString("N");

        // 随机目标位置（40% - 80% 之间）
        var random = RandomNumberGenerator.Create();
        var bytes = new byte[2];
        random.GetBytes(bytes);
        var targetPercent = 0.4 + (BitConverter.ToUInt16(bytes) % 400) / 1000.0;

        // 保存到数据库（复用 CaptchaSession，answer 存百分比）
        var session = new CaptchaSession
        {
            SessionId = sessionId,
            AnswerHash = targetPercent.ToString("F4"),
            IpHash = ipHash,
            ExpiresAt = DateTimeOffset.UtcNow.Add(Expiration),
        };
        _db.CaptchaSessions.Add(session);
        await _db.SaveChangesAsync();

        return new SliderCaptchaResult
        {
            SessionId = sessionId,
            TargetPercent = targetPercent,
        };
    }

    /// <summary>
    /// 校验滑块验证码
    /// </summary>
    public async Task<CaptchaVerifyResult> VerifySliderAsync(string sessionId, double percent, string ipAddress)
    {
        var ipHash = HashIp(ipAddress);

        var session = await _db.CaptchaSessions
            .FirstOrDefaultAsync(x => x.SessionId == sessionId);

        if (session == null)
            return new CaptchaVerifyResult { Valid = false, ErrorMessage = "验证码会话不存在" };

        if (session.UsedAt != null)
            return new CaptchaVerifyResult { Valid = false, ErrorMessage = "验证码已使用" };

        if (session.ExpiresAt < DateTimeOffset.UtcNow)
            return new CaptchaVerifyResult { Valid = false, ErrorMessage = "验证码已过期" };

        if (session.IpHash != ipHash)
            return new CaptchaVerifyResult { Valid = false, ErrorMessage = "IP 不匹配" };

        // 解析目标百分比，允许 5% 误差
        if (!double.TryParse(session.AnswerHash, out var targetPercent))
            return new CaptchaVerifyResult { Valid = false, ErrorMessage = "验证码数据损坏" };

        if (Math.Abs(percent / 100.0 - targetPercent) > 0.10)
            return new CaptchaVerifyResult { Valid = false, ErrorMessage = "验证未通过" };

        // 标记为已使用
        session.UsedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        return new CaptchaVerifyResult { Valid = true };
    }

    private static SKTypeface GetCaptchaFont()
    {
        // 尝试多种字体（按优先级），Linux Docker 中 Arial 不可用
        foreach (var family in new[] { "DejaVu Sans", "Arial", "Tahoma", "Verdana", "sans-serif" })
        {
            var typeface = SKTypeface.FromFamilyName(family, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            if (typeface?.FamilyName != null && !typeface.FamilyName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                return typeface;
        }
        return SKTypeface.Default;
    }

    /// <summary>使用 SkiaSharp 绘制验证码图片</summary>
    private static string DrawCaptchaImage(string code)
    {
        using var bitmap = new SKBitmap(ImageWidth, ImageHeight);
        using var canvas = new SKCanvas(bitmap);

        // 白色背景
        canvas.Clear(SKColors.White);

        // 随机噪点
        var random = new Random();
        using var noisePaint = new SKPaint { Color = new SKColor(200, 200, 200), StrokeWidth = 1 };

        // 干扰线
        for (int i = 0; i < 3; i++)
        {
            using var linePaint = new SKPaint
            {
                Color = new SKColor((byte)random.Next(150, 220), (byte)random.Next(150, 220), (byte)random.Next(150, 220)),
                StrokeWidth = 1,
                IsAntialias = true,
            };
            canvas.DrawLine(
                random.Next(0, ImageWidth), random.Next(0, ImageHeight),
                random.Next(0, ImageWidth), random.Next(0, ImageHeight),
                linePaint);
        }

        // 噪点
        for (int i = 0; i < 80; i++)
        {
            canvas.DrawPoint(random.Next(0, ImageWidth), random.Next(0, ImageHeight), noisePaint);
        }

        // 绘制验证码字符（使用回退字体）
        using var font = new SKFont(GetCaptchaFont(), 24);
        var textWidth = code.Length * 20;
        var startX = (ImageWidth - textWidth) / 2;
        var y = ImageHeight / 2 + 8;

        for (int i = 0; i < code.Length; i++)
        {
            using var textPaint = new SKPaint
            {
                Color = new SKColor(
                    (byte)random.Next(30, 100),
                    (byte)random.Next(30, 100),
                    (byte)random.Next(30, 180)),
                IsAntialias = true,
            };
            canvas.DrawText(code[i].ToString(), startX + i * 22, y, SKTextAlign.Left, font, textPaint);
        }

        // 输出为 PNG Base64
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 80);
        using var ms = new MemoryStream();
        data.SaveTo(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());
        return $"data:image/png;base64,{base64}";
    }

    /// <summary>哈希 IP 地址</summary>
    private static string HashIp(string ip)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(ip));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}