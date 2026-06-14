using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Models.DTOs.Article;
using Mblog.API.Models.DTOs.Public;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

/// <summary>
/// 文章服务接口
/// </summary>
public interface IArticleService
{
    /// <summary>分页查询文章列表</summary>
    Task<PagedResponse<ArticleListItem>> GetPagedAsync(ArticleQueryParams query);

    /// <summary>根据 ID 获取文章详情</summary>
    Task<ArticleDetailResponse?> GetByIdAsync(Guid id);

    /// <summary>创建文章</summary>
    Task<ArticleDetailResponse> CreateAsync(CreateArticleRequest request, Guid authorId);

    /// <summary>更新文章</summary>
    Task<ArticleDetailResponse?> UpdateAsync(Guid id, UpdateArticleRequest request);

    /// <summary>更新文章状态</summary>
    Task<ArticleDetailResponse?> UpdateStatusAsync(Guid id, string status);

    /// <summary>切换置顶状态</summary>
    Task<bool> ToggleTopAsync(Guid id);

    /// <summary>切换推荐状态</summary>
    Task<bool> ToggleRecommendAsync(Guid id);

    /// <summary>删除文章（软删除）</summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>彻底删除文章（硬删除）</summary>
    Task<bool> HardDeleteAsync(Guid id);

    /// <summary>恢复文章（从回收站还原）</summary>
    Task<bool> RestoreAsync(Guid id);

    // ===== 前台公开 API =====

    /// <summary>前台分页查询已发布的文章列表</summary>
    Task<PagedResponse<PublicArticleListItem>> GetPublicPagedAsync(PublicArticleQueryParams query);

    /// <summary>根据 Slug 获取已发布的文章详情</summary>
    Task<PublicArticleDetailResponse?> GetPublicBySlugAsync(string slug);

    /// <summary>获取相关文章（同分类/同标签）</summary>
    Task<List<PublicArticleListItem>> GetRelatedAsync(string slug, int count = 3);

    /// <summary>增加文章阅读数</summary>
    Task<bool> IncrementViewCountAsync(string slug);

    /// <summary>获取归档数据（按年月分组）</summary>
    Task<List<ArchiveItem>> GetArchiveAsync();
}

/// <summary>
/// 文章服务实现
/// </summary>
public class ArticleService : IArticleService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public ArticleService(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<PagedResponse<ArticleListItem>> GetPagedAsync(ArticleQueryParams query)
    {
        var q = _db.Articles
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag)
            .Include(x => x.ArticleCategories)
                .ThenInclude(x => x.Category)
            .AsQueryable();

        // 筛选 — 回收站模式：ShowDeleted=true 时只返回已软删除的，否则只返回未删除的
        if (query.ShowDeleted.HasValue && query.ShowDeleted.Value)
            q = q.IgnoreQueryFilters().Where(x => x.DeletedAt != null);
        else
            q = q.Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            var isNpgsql = _db.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

            if (isNpgsql && keyword.Length >= 2)
            {
                // PostgreSQL 全文搜索（tsvector + tsquery）
                var tsQuery = string.Join(" | ", keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(k => k + ":*"));
                q = q.Where(x => x.SearchVector.Matches(EF.Functions.ToTsQuery("simple", tsQuery)));
            }
            else
            {
                // SQLite 降级为 LIKE 搜索
                var kw = keyword.ToLower();
                q = q.Where(x => x.Title.ToLower().Contains(kw)
                              || (x.Summary != null && x.Summary.ToLower().Contains(kw)));
            }
        }
        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(x => x.Status == query.Status);
        if (query.CategoryId.HasValue)
            q = q.Where(x => x.CategoryId == query.CategoryId.Value || x.ArticleCategories.Any(ac => ac.CategoryId == query.CategoryId.Value));
        if (query.TagId.HasValue)
            q = q.Where(x => x.ArticleTags.Any(at => at.TagId == query.TagId.Value));
        if (query.IsTop.HasValue)
            q = q.Where(x => x.IsTop == query.IsTop.Value);
        if (query.IsRecommend.HasValue)
            q = q.Where(x => x.IsRecommend == query.IsRecommend.Value);

        // 排序
        q = (query.SortBy.ToLower(), query.SortOrder.ToLower()) switch
        {
            ("publishedat", "asc") => q.OrderBy(x => x.PublishedAt),
            ("publishedat", _) => q.OrderByDescending(x => x.PublishedAt),
            ("viewcount", "asc") => q.OrderBy(x => x.ViewCount),
            ("viewcount", _) => q.OrderByDescending(x => x.ViewCount),
            ("title", "asc") => q.OrderBy(x => x.Title),
            ("title", _) => q.OrderByDescending(x => x.Title),
            ("createdat", "asc") => q.OrderBy(x => x.CreatedAt),
            _ => q.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = await q.CountAsync();

        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new ArticleListItem
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Summary = x.Summary,
                Status = x.Status,
                IsTop = x.IsTop,
                IsRecommend = x.IsRecommend,
                ViewCount = x.ViewCount,
                LikeCount = x.LikeCount,
                CommentCount = x.CommentCount,
                CategoryName = x.Category != null ? x.Category.Name : null,
                CategoryNames = x.ArticleCategories.Select(ac => ac.Category.Name).ToList(),
                AuthorName = x.Author.Username,
                Tags = x.ArticleTags.Select(at => at.Tag.Name).ToList(),
                PublishedAt = x.PublishedAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return new PagedResponse<ArticleListItem>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            Items = items
        };
    }

    public async Task<ArticleDetailResponse?> GetByIdAsync(Guid id)
    {
        return await _db.Articles
            .AsNoTracking()
            .Include(x => x.CoverImage)
            .Include(x => x.Author)
            .Include(x => x.Category)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag)
            .Include(x => x.ArticleCategories)
                .ThenInclude(x => x.Category)
            .Where(x => x.Id == id && x.DeletedAt == null)
            .Select(x => new ArticleDetailResponse
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Summary = x.Summary,
                Content = x.Content,
                CoverImageId = x.CoverImageId,
                CoverImageUrl = x.CoverImage != null ? x.CoverImage.Url : null,
                Author = new ArticleAuthorInfo
                {
                    Id = x.Author.Id,
                    Username = x.Author.Username,
                    AvatarUrl = x.Author.Avatar != null ? x.Author.Avatar.Url : null
                },
                Category = x.Category != null ? new ArticleCategoryInfo
                {
                    Id = x.Category.Id,
                    Name = x.Category.Name,
                    Slug = x.Category.Slug
                } : null,
                Categories = x.ArticleCategories.Select(ac => new ArticleCategoryInfo
                {
                    Id = ac.Category.Id,
                    Name = ac.Category.Name,
                    Slug = ac.Category.Slug
                }).ToList(),
                Tags = x.ArticleTags.Select(at => new ArticleTagInfo
                {
                    Id = at.Tag.Id,
                    Name = at.Tag.Name,
                    Color = at.Tag.Color
                }).ToList(),
                Status = x.Status,
                IsTop = x.IsTop,
                IsRecommend = x.IsRecommend,
                ViewCount = x.ViewCount,
                LikeCount = x.LikeCount,
                CommentCount = x.CommentCount,
                SeoTitle = x.SeoTitle,
                SeoDescription = x.SeoDescription,
                SeoKeywords = x.SeoKeywords,
                PublishedAt = x.PublishedAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ArticleDetailResponse> CreateAsync(CreateArticleRequest request, Guid authorId)
    {
        // 验证作者用户存在
        if (!await _db.Users.AnyAsync(x => x.Id == authorId && x.DeletedAt == null))
            throw new UnauthorizedAccessException("当前用户不存在或已被删除");

        if (await _db.Articles.AnyAsync(x => x.Slug == request.Slug))
        {
            throw new ArgumentException($"Slug '{request.Slug}' 已存在");
        }

        // 验证分类存在
        if (request.CategoryIds.Count > 0)
        {
            var validCategoryIds = await _db.Categories
                .Where(x => request.CategoryIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();
            var invalidIds = request.CategoryIds.Except(validCategoryIds).ToList();
            if (invalidIds.Count > 0)
                throw new ArgumentException($"以下分类 ID 不存在: {string.Join(", ", invalidIds)}");
        }

        // 验证标签存在
        if (request.TagIds.Count > 0)
        {
            var validTagIds = await _db.Tags
                .Where(x => request.TagIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();
            var invalidIds = request.TagIds.Except(validTagIds).ToList();
            if (invalidIds.Count > 0)
                throw new ArgumentException($"以下标签 ID 不存在: {string.Join(", ", invalidIds)}");
        }

        var now = DateTimeOffset.UtcNow;
        var article = new Models.Entities.Article
        {
            Title = request.Title,
            Slug = request.Slug,
            Summary = request.Summary,
            Content = request.Content,
            CoverImageId = request.CoverImageId,
            AuthorId = authorId,
            CategoryId = request.CategoryIds.Any() ? request.CategoryIds.First() : null,
            Status = request.Status,
            IsTop = request.IsTop,
            IsRecommend = request.IsRecommend,
            SeoTitle = request.SeoTitle,
            SeoDescription = request.SeoDescription,
            SeoKeywords = request.SeoKeywords,
            PublishedAt = request.PublishedAt ?? (request.Status == "published" ? now : null),
            CreatedAt = request.CreatedAt ?? now,
            UpdatedAt = request.UpdatedAt ?? now,
        };

        _db.Articles.Add(article);
        await _db.SaveChangesAsync();

        // 关联多分类
        foreach (var catId in request.CategoryIds)
        {
            _db.ArticleCategories.Add(new ArticleCategory
            {
                ArticleId = article.Id,
                CategoryId = catId,
                CreatedAt = now
            });
        }

        // 关联标签
        if (request.TagIds.Count > 0)
        {
            foreach (var tagId in request.TagIds)
            {
                _db.ArticleTags.Add(new ArticleTag
                {
                    ArticleId = article.Id,
                    TagId = tagId,
                    CreatedAt = now
                });
            }
        }

        await _db.SaveChangesAsync();

        return await GetByIdAsync(article.Id)
            ?? throw new InvalidOperationException("创建文章后获取详情失败");
    }

    public async Task<ArticleDetailResponse?> UpdateAsync(Guid id, UpdateArticleRequest request)
    {
        var article = await _db.Articles
            .Include(x => x.ArticleTags)
            .Include(x => x.ArticleCategories)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

        if (article == null) return null;

        if (await _db.Articles.AnyAsync(x => x.Slug == request.Slug && x.Id != id))
        {
            throw new ArgumentException($"Slug '{request.Slug}' 已存在");
        }

        // 验证分类存在
        if (request.CategoryIds.Count > 0)
        {
            var validCategoryIds = await _db.Categories
                .Where(x => request.CategoryIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();
            var invalidIds = request.CategoryIds.Except(validCategoryIds).ToList();
            if (invalidIds.Count > 0)
                throw new ArgumentException($"以下分类 ID 不存在: {string.Join(", ", invalidIds)}");
        }

        // 验证标签存在
        if (request.TagIds.Count > 0)
        {
            var validTagIds = await _db.Tags
                .Where(x => request.TagIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();
            var invalidIds = request.TagIds.Except(validTagIds).ToList();
            if (invalidIds.Count > 0)
                throw new ArgumentException($"以下标签 ID 不存在: {string.Join(", ", invalidIds)}");
        }

        var now = DateTimeOffset.UtcNow;
        var wasDraft = article.Status == "draft";
        var publishing = request.Status == "published";

        article.Title = request.Title;
        article.Slug = request.Slug;
        article.Summary = request.Summary;
        article.Content = request.Content;
        article.CoverImageId = request.CoverImageId;
        article.CategoryId = request.CategoryIds.Any() ? request.CategoryIds.First() : null;
        article.Status = request.Status;
        article.IsTop = request.IsTop;
        article.IsRecommend = request.IsRecommend;
        article.SeoTitle = request.SeoTitle;
        article.SeoDescription = request.SeoDescription;
        article.SeoKeywords = request.SeoKeywords;

        // 从草稿发布时记录发布时间
        if (wasDraft && publishing && article.PublishedAt == null)
        {
            article.PublishedAt = now;
        }

        // 更新标签关联：先删后加
        _db.ArticleTags.RemoveRange(article.ArticleTags);
        foreach (var tagId in request.TagIds)
        {
            _db.ArticleTags.Add(new ArticleTag
            {
                ArticleId = id,
                TagId = tagId,
                CreatedAt = now
            });
        }

        // 更新分类关联：先删后加
        _db.ArticleCategories.RemoveRange(article.ArticleCategories);
        foreach (var catId in request.CategoryIds)
        {
            _db.ArticleCategories.Add(new ArticleCategory
            {
                ArticleId = id,
                CategoryId = catId,
                CreatedAt = now
            });
        }

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<ArticleDetailResponse?> UpdateStatusAsync(Guid id, string status)
    {
        var article = await _db.Articles
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

        if (article == null) return null;

        var validStatuses = new[] { "draft", "published", "archived" };
        if (!validStatuses.Contains(status))
            throw new ArgumentException($"无效的状态: {status}，可选值: {string.Join(", ", validStatuses)}");

        article.Status = status;

        if (status == "published" && article.PublishedAt == null)
        {
            article.PublishedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> ToggleTopAsync(Guid id)
    {
        var article = await _db.Articles.FindAsync(id);
        if (article == null) return false;

        article.IsTop = !article.IsTop;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleRecommendAsync(Guid id)
    {
        var article = await _db.Articles.FindAsync(id);
        if (article == null) return false;

        article.IsRecommend = !article.IsRecommend;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var article = await _db.Articles.FindAsync(id);
        if (article == null) return false;

        article.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HardDeleteAsync(Guid id)
    {
        var article = await _db.Articles
            .IgnoreQueryFilters()
            .Include(x => x.ArticleTags)
            .Include(x => x.ArticleCategories)
            .Include(x => x.Comments)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (article == null) return false;

        // 移除关联数据
        _db.Set<ArticleTag>().RemoveRange(article.ArticleTags);
        _db.Set<ArticleCategory>().RemoveRange(article.ArticleCategories);
        if (article.Comments.Any())
            _db.Comments.RemoveRange(article.Comments);

        _db.Articles.Remove(article);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(Guid id)
    {
        var article = await _db.Articles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (article == null || article.DeletedAt == null) return false;

        article.DeletedAt = null;
        article.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    // ===== 前台公开 API 实现 =====

    public async Task<PagedResponse<PublicArticleListItem>> GetPublicPagedAsync(PublicArticleQueryParams query)
    {
        var q = _db.Articles
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.CoverImage)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag)
            .Include(x => x.ArticleCategories)
                .ThenInclude(x => x.Category)
            .Where(x => x.DeletedAt == null && x.Status == "published")
            .AsQueryable();

        // 关键词搜索
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            var isNpgsql = _db.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

            if (isNpgsql && keyword.Length >= 2)
            {
                // PostgreSQL 全文搜索（tsvector + tsquery）
                var tsQuery = string.Join(" | ", keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(k => k + ":*"));
                q = q.Where(x => x.SearchVector.Matches(EF.Functions.ToTsQuery("simple", tsQuery)));
            }
            else
            {
                // SQLite 降级为 LIKE 搜索
                var kw = keyword.ToLower();
                q = q.Where(x => x.Title.ToLower().Contains(kw)
                              || (x.Summary != null && x.Summary.ToLower().Contains(kw)));
            }
        }

        // 分类筛选
        if (query.CategoryId.HasValue)
            q = q.Where(x => x.CategoryId == query.CategoryId.Value || x.ArticleCategories.Any(ac => ac.CategoryId == query.CategoryId.Value));
        if (!string.IsNullOrWhiteSpace(query.CategorySlug))
            q = q.Where(x => x.Category != null && x.Category.Slug == query.CategorySlug);

        // 标签筛选
        if (query.TagId.HasValue)
            q = q.Where(x => x.ArticleTags.Any(at => at.TagId == query.TagId.Value));
        if (!string.IsNullOrWhiteSpace(query.TagSlug))
            q = q.Where(x => x.ArticleTags.Any(at => at.Tag.Slug == query.TagSlug));

        // 排序 — 置顶优先，再按其他排序
        q = query.SortBy.ToLower() switch
        {
            "views" => q.OrderByDescending(x => x.IsTop).ThenByDescending(x => x.ViewCount),
            "comments" => q.OrderByDescending(x => x.IsTop).ThenByDescending(x => x.CommentCount),
            _ => q.OrderByDescending(x => x.IsTop).ThenByDescending(x => x.PublishedAt)
        };

        var totalCount = await q.CountAsync();

        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new PublicArticleListItem
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Summary = x.Summary,
                CoverImageUrl = x.CoverImage != null ? x.CoverImage.Url : null,
                Category = x.Category != null ? new PublicCategoryInfo
                {
                    Id = x.Category.Id,
                    Name = x.Category.Name,
                    Slug = x.Category.Slug
                } : null,
                Categories = x.ArticleCategories.Select(ac => new PublicCategoryInfo
                {
                    Id = ac.Category.Id,
                    Name = ac.Category.Name,
                    Slug = ac.Category.Slug
                }).ToList(),
                Tags = x.ArticleTags.Select(at => new PublicTagInfo
                {
                    Id = at.Tag.Id,
                    Name = at.Tag.Name,
                    Slug = at.Tag.Slug,
                    Color = at.Tag.Color,
                    BgColor = at.Tag.BgColor,
                }).ToList(),
                AuthorName = x.Author.Username,
                ViewCount = x.ViewCount,
                CommentCount = x.CommentCount,
                IsRecommend = x.IsRecommend,
                IsTop = x.IsTop,
                PublishedAt = x.PublishedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return new PagedResponse<PublicArticleListItem>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            Items = items
        };
    }

    public async Task<PublicArticleDetailResponse?> GetPublicBySlugAsync(string slug)
    {
        var cacheKey = $"article_detail_{slug}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _db.Articles
            .AsNoTracking()
            .Include(x => x.CoverImage)
            .Include(x => x.Author)
            .Include(x => x.Category)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag)
            .Include(x => x.ArticleCategories)
                .ThenInclude(x => x.Category)
            .Where(x => x.Slug == slug && x.DeletedAt == null && x.Status == "published")
            .Select(x => new PublicArticleDetailResponse
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Summary = x.Summary,
                Content = x.Content,
                CoverImageUrl = x.CoverImage != null ? x.CoverImage.Url : null,
                Author = new PublicAuthorInfo
                {
                    Id = x.Author.Id,
                    Username = x.Author.Username,
                    AvatarUrl = x.Author.Avatar != null ? x.Author.Avatar.Url : null
                },
                Category = x.Category != null ? new PublicCategoryInfo
                {
                    Id = x.Category.Id,
                    Name = x.Category.Name,
                    Slug = x.Category.Slug,
                    Description = x.Category.Description
                } : null,
                Categories = x.ArticleCategories.Select(ac => new PublicCategoryInfo
                {
                    Id = ac.Category.Id,
                    Name = ac.Category.Name,
                    Slug = ac.Category.Slug
                }).ToList(),
                Tags = x.ArticleTags.Select(at => new PublicTagInfo
                {
                    Id = at.Tag.Id,
                    Name = at.Tag.Name,
                    Slug = at.Tag.Slug,
                    Color = at.Tag.Color,
                    BgColor = at.Tag.BgColor,
                }).ToList(),
                ViewCount = x.ViewCount,
                CommentCount = x.CommentCount,
                SeoTitle = x.SeoTitle,
                SeoDescription = x.SeoDescription,
                SeoKeywords = x.SeoKeywords,
                Status = x.Status,
                PublishedAt = x.PublishedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
        });
    }

    public async Task<List<PublicArticleListItem>> GetRelatedAsync(string slug, int count = 3)
    {
        // 先获取当前文章的分类和标签
        var current = await _db.Articles
            .AsNoTracking()
            .Include(x => x.ArticleTags)
            .FirstOrDefaultAsync(x => x.Slug == slug && x.DeletedAt == null && x.Status == "published");

        if (current == null) return new List<PublicArticleListItem>();

        var tagIds = current.ArticleTags.Select(at => at.TagId).ToList();

        // 查找同分类或同标签的文章（排除自身）
        var related = await _db.Articles
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.CoverImage)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag)
            .Include(x => x.ArticleCategories)
                .ThenInclude(x => x.Category)
            .Where(x => x.Id != current.Id
                     && x.DeletedAt == null
                     && x.Status == "published"
                     && (x.CategoryId == current.CategoryId
                         || x.ArticleTags.Any(at => tagIds.Contains(at.TagId))))
            .OrderByDescending(x => x.PublishedAt)
            .Take(count)
            .Select(x => new PublicArticleListItem
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Summary = x.Summary,
                CoverImageUrl = x.CoverImage != null ? x.CoverImage.Url : null,
                Category = x.Category != null ? new PublicCategoryInfo
                {
                    Id = x.Category.Id,
                    Name = x.Category.Name,
                    Slug = x.Category.Slug
                } : null,
                Categories = x.ArticleCategories.Select(ac => new PublicCategoryInfo
                {
                    Id = ac.Category.Id,
                    Name = ac.Category.Name,
                    Slug = ac.Category.Slug
                }).ToList(),
                Tags = x.ArticleTags.Select(at => new PublicTagInfo
                {
                    Id = at.Tag.Id,
                    Name = at.Tag.Name,
                    Slug = at.Tag.Slug,
                    Color = at.Tag.Color,
                    BgColor = at.Tag.BgColor,
                }).ToList(),
                AuthorName = x.Author.Username,
                ViewCount = x.ViewCount,
                CommentCount = x.CommentCount,
                IsRecommend = x.IsRecommend,
                PublishedAt = x.PublishedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return related;
    }

    public async Task<bool> IncrementViewCountAsync(string slug)
    {
        var article = await _db.Articles
            .FirstOrDefaultAsync(x => x.Slug == slug && x.DeletedAt == null && x.Status == "published");

        if (article == null) return false;

        article.ViewCount++;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<ArchiveItem>> GetArchiveAsync()
    {
        var articles = await _db.Articles
            .AsNoTracking()
            .Where(x => x.DeletedAt == null && x.Status == "published" && x.PublishedAt != null)
            .OrderByDescending(x => x.PublishedAt)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Slug,
                x.Summary,
                Year = x.PublishedAt!.Value.Year,
                Month = x.PublishedAt!.Value.Month,
                PublishedAt = x.PublishedAt!.Value
            })
            .ToListAsync();

        var archive = articles
            .GroupBy(x => new { x.Year, x.Month })
            .Select(g => new ArchiveItem
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count(),
                Articles = g.Select(a => new ArchiveArticleItem
                {
                    Id = a.Id,
                    Title = a.Title,
                    Slug = a.Slug,
                    Summary = a.Summary,
                    PublishedAt = a.PublishedAt
                }).ToList()
            })
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToList();

        return archive;
    }
}