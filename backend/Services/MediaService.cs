using Microsoft.EntityFrameworkCore;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Models.DTOs.Media;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

/// <summary>
/// 存储配置选项
/// </summary>
public class StorageSettings
{
    public const string SectionName = "Storage";
    public string RootPath { get; set; } = "./uploads";
}

/// <summary>
/// 媒体服务接口
/// </summary>
public interface IMediaService
{
    /// <summary>上传图片，返回媒体记录</summary>
    Task<UploadResponse> UploadImageAsync(IFormFile file, Guid userId);

    /// <summary>上传视频，返回媒体记录</summary>
    Task<UploadResponse> UploadVideoAsync(IFormFile file, Guid userId);

    /// <summary>上传附件，返回媒体记录</summary>
    Task<UploadResponse> UploadFileAsync(IFormFile file, Guid userId);

    /// <summary>分页查询媒体列表</summary>
    Task<PagedResponse<MediaListItem>> GetPagedAsync(MediaQueryParams query);

    /// <summary>删除媒体（软删除）</summary>
    Task<bool> DeleteAsync(Guid id);
}

/// <summary>
/// 媒体服务实现，处理文件上传和数据库记录
/// </summary>
public class MediaService : IMediaService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly StorageSettings _settings;

    public MediaService(AppDbContext db, IWebHostEnvironment env, IConfiguration configuration)
    {
        _db = db;
        _env = env;
        _settings = configuration.GetSection(StorageSettings.SectionName).Get<StorageSettings>()
            ?? new StorageSettings();
    }

    public async Task<UploadResponse> UploadImageAsync(IFormFile file, Guid userId)
    {
        // 验证用户存在
        if (!await _db.Users.AnyAsync(x => x.Id == userId && x.DeletedAt == null))
            throw new UnauthorizedAccessException("当前用户不存在或已被删除");

        // 验证文件大小
        if (file.Length > 10 * 1024 * 1024)
            throw new ArgumentException("图片大小不能超过 10MB");

        // 验证文件扩展名
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
        if (!allowedExts.Contains(ext))
            throw new ArgumentException("不支持的文件格式");

        // 生成安全文件名
        var safeFilename = $"{Guid.NewGuid()}{ext}";

        // 按日期分目录存储
        var dateDir = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var relativePath = Path.Combine("images", dateDir);
        var storageDir = Path.Combine(_settings.RootPath, relativePath);
        var fullDir = Path.Combine(_env.ContentRootPath, storageDir);

        if (!Directory.Exists(fullDir))
            Directory.CreateDirectory(fullDir);

        var fullPath = Path.Combine(fullDir, safeFilename);

        // 保存文件
        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 构建访问 URL
        var url = $"/uploads/{relativePath}/{safeFilename}".Replace("\\", "/");

        // 创建媒体记录
        var media = new Media
        {
            Type = "image",
            Url = url,
            Filename = safeFilename,
            OriginalFilename = file.FileName,
            StoragePath = fullPath,
            SizeBytes = file.Length,
            MimeType = file.ContentType,
            CreatedBy = userId
        };

        _db.Media.Add(media);
        await _db.SaveChangesAsync();

        return new UploadResponse
        {
            Id = media.Id,
            Url = url,
            Filename = safeFilename,
            OriginalFilename = file.FileName,
            SizeBytes = file.Length,
            MimeType = file.ContentType,
            Type = "image"
        };
    }

    public async Task<UploadResponse> UploadVideoAsync(IFormFile file, Guid userId)
    {
        // 验证用户存在
        if (!await _db.Users.AnyAsync(x => x.Id == userId && x.DeletedAt == null))
            throw new UnauthorizedAccessException("当前用户不存在或已被删除");

        if (file.Length > 500 * 1024 * 1024)
            throw new ArgumentException("视频大小不能超过 500MB");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExts = new[] { ".mp4", ".webm", ".mov", ".avi", ".mkv" };
        if (!allowedExts.Contains(ext))
            throw new ArgumentException("不支持的视频格式，仅支持 mp4/webm/mov/avi/mkv");

        var safeFilename = $"{Guid.NewGuid()}{ext}";
        var dateDir = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var relativePath = Path.Combine("videos", dateDir);
        var storageDir = Path.Combine(_settings.RootPath, relativePath);
        var fullDir = Path.Combine(_env.ContentRootPath, storageDir);

        if (!Directory.Exists(fullDir))
            Directory.CreateDirectory(fullDir);

        var fullPath = Path.Combine(fullDir, safeFilename);
        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"/uploads/{relativePath}/{safeFilename}".Replace("\\", "/");

        var media = new Media
        {
            Type = "video",
            Url = url,
            Filename = safeFilename,
            OriginalFilename = file.FileName,
            StoragePath = fullPath,
            SizeBytes = file.Length,
            MimeType = file.ContentType,
            CreatedBy = userId
        };

        _db.Media.Add(media);
        await _db.SaveChangesAsync();

        return new UploadResponse
        {
            Id = media.Id,
            Url = url,
            Filename = safeFilename,
            OriginalFilename = file.FileName,
            SizeBytes = file.Length,
            MimeType = file.ContentType,
            Type = "video"
        };
    }

    public async Task<UploadResponse> UploadFileAsync(IFormFile file, Guid userId)
    {
        // 验证用户存在
        if (!await _db.Users.AnyAsync(x => x.Id == userId && x.DeletedAt == null))
            throw new UnauthorizedAccessException("当前用户不存在或已被删除");

        if (file.Length > 100 * 1024 * 1024)
            throw new ArgumentException("附件大小不能超过 100MB");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExts = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".zip", ".rar", ".7z", ".txt", ".csv" };
        if (!allowedExts.Contains(ext))
            throw new ArgumentException("不支持的文件格式");

        var safeFilename = $"{Guid.NewGuid()}{ext}";
        var dateDir = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var relativePath = Path.Combine("files", dateDir);
        var storageDir = Path.Combine(_settings.RootPath, relativePath);
        var fullDir = Path.Combine(_env.ContentRootPath, storageDir);

        if (!Directory.Exists(fullDir))
            Directory.CreateDirectory(fullDir);

        var fullPath = Path.Combine(fullDir, safeFilename);
        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"/uploads/{relativePath}/{safeFilename}".Replace("\\", "/");

        var media = new Media
        {
            Type = "file",
            Url = url,
            Filename = safeFilename,
            OriginalFilename = file.FileName,
            StoragePath = fullPath,
            SizeBytes = file.Length,
            MimeType = file.ContentType,
            CreatedBy = userId
        };

        _db.Media.Add(media);
        await _db.SaveChangesAsync();

        return new UploadResponse
        {
            Id = media.Id,
            Url = url,
            Filename = safeFilename,
            OriginalFilename = file.FileName,
            SizeBytes = file.Length,
            MimeType = file.ContentType,
            Type = "file"
        };
    }

    public async Task<PagedResponse<MediaListItem>> GetPagedAsync(MediaQueryParams query)
    {
        var q = _db.Media
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Type))
            q = q.Where(x => x.Type == query.Type);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.ToLower();
            q = q.Where(x => x.OriginalFilename!.ToLower().Contains(keyword)
                          || x.Filename.ToLower().Contains(keyword));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new MediaListItem
            {
                Id = x.Id,
                Type = x.Type,
                Url = x.Url,
                ThumbnailUrl = x.ThumbnailUrl,
                Filename = x.Filename,
                OriginalFilename = x.OriginalFilename,
                SizeBytes = x.SizeBytes,
                MimeType = x.MimeType,
                Width = x.Width,
                Height = x.Height,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return new PagedResponse<MediaListItem>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total,
            Items = items
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _db.Media.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
        if (entity == null) return false;

        entity.DeletedAt = DateTimeOffset.UtcNow;

        // 尝试删除物理文件
        try
        {
            if (File.Exists(entity.StoragePath))
                File.Delete(entity.StoragePath);
        }
        catch
        {
            // 忽略文件删除失败
        }

        await _db.SaveChangesAsync();
        return true;
    }
}