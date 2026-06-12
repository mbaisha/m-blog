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
        try
        {
            return await GenerateAsyncInternal(ipAddress);
        }
        catch (Exception ex)
        {
            // 记录详细错误信息用于排查
            Console.Error.WriteLine($"[Captcha] GenerateAsync 失败: {ex.GetType().FullName}: {ex.Message}");
            if (ex.InnerException != null)
                Console.Error.WriteLine($"[Captcha] 内部异常: {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}");
            Console.Error.WriteLine($"[Captcha] 堆栈: {ex.StackTrace}");

            // 返回一个占位验证码，前端显示"验证码加载失败"
            return new CaptchaResult
            {
                SessionId = Guid.NewGuid().ToString("N"),
                ImageBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII="
            };
        }
    }

    /// <summary>生成验证码图片（内部方法）</summary>
    private async Task<CaptchaResult> GenerateAsyncInternal(string ipAddress)
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

    /// <summary>
    /// 使用 SkiaSharp 绘制验证码图片（字体渲染，跨平台兼容）
    /// </summary>
    private static string DrawCaptchaImage(string code)
    {
        using var bitmap = new SKBitmap(ImageWidth, ImageHeight);
        using var canvas = new SKCanvas(bitmap);

        // 浅灰背景
        canvas.Clear(new SKColor(245, 247, 250));

        var random = new Random();

        // 背景干扰点（浅色小圆点）
        using var bgDotPaint = new SKPaint
        {
            Color = new SKColor(200, 210, 220, 120),
            IsAntialias = true,
        };
        for (int i = 0; i < 30; i++)
        {
            canvas.DrawCircle(
                random.Next(0, ImageWidth), random.Next(0, ImageHeight),
                random.Next(1, 3), bgDotPaint);
        }

        // 干扰线（彩色弧线，不遮挡文字）
        for (int i = 0; i < 2; i++)
        {
            using var linePaint = new SKPaint
            {
                Color = new SKColor((byte)random.Next(180, 230), (byte)random.Next(180, 230), (byte)random.Next(200, 240)),
                StrokeWidth = 1.5f,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
            };
            var linePath = new SKPath();
            linePath.MoveTo(0, random.Next(5, ImageHeight - 5));
            linePath.CubicTo(
                ImageWidth / 3f, random.Next(5, ImageHeight - 5),
                ImageWidth * 2 / 3f, random.Next(5, ImageHeight - 5),
                ImageWidth, random.Next(5, ImageHeight - 5));
            canvas.DrawPath(linePath, linePaint);
        }

        // 获取字体（Linux 下 DejaVu Sans 已安装）
        using var typeface = GetBestTypeface();
        using var font = new SKFont(typeface, 26);

        // 测量文字总宽度
        var totalWidth = font.MeasureText(code);
        var startX = (ImageWidth - totalWidth) / 2;
        var startY = ImageHeight / 2f + 9;

        // 逐字符绘制，每个字符不同颜色和轻微旋转
        for (int i = 0; i < code.Length; i++)
        {
            var ch = code[i].ToString();

            // 字符宽度
            var charWidth = font.MeasureText(ch);

            // 随机颜色（深色系，好识别）
            var color = new SKColor(
                (byte)random.Next(20, 80),
                (byte)random.Next(20, 80),
                (byte)random.Next(80, 180));

            using var textPaint = new SKPaint
            {
                Color = color,
                IsAntialias = true,
            };

            // 轻微旋转和垂直偏移
            var angle = (random.NextDouble() - 0.5) * 15; // ±7.5 度
            var yOffset = (random.NextDouble() - 0.5) * 6; // ±3px

            canvas.Save();
            canvas.RotateDegrees((float)angle, startX + charWidth / 2, startY);
            canvas.DrawText(ch, startX, startY + (float)yOffset, font, textPaint);
            canvas.Restore();

            startX += charWidth + 3;
        }

        // 输出为 PNG Base64
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 85);
        using var ms = new MemoryStream();
        data.SaveTo(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());
        return $"data:image/png;base64,{base64}";
    }

    /// <summary>获取最佳可用字体（Linux/Windows 跨平台）</summary>
    private static SKTypeface GetBestTypeface()
    {
        // 尝试加载系统字体
        foreach (var family in new[] { "DejaVu Sans", "Arial", "Tahoma", "Verdana", "Helvetica" })
        {
            var tf = SKTypeface.FromFamilyName(family, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            if (tf != null && !string.IsNullOrEmpty(tf.FamilyName))
                return tf;
        }
        return SKTypeface.Default;
    }

    /// <summary>哈希 IP 地址</summary>
    private static string HashIp(string ip)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(ip));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}