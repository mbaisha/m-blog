using Microsoft.EntityFrameworkCore;
using Mblog.API.Data;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

public interface ILayoutService
{
    Task<List<ModuleLayoutResponse>> GetByPageKeyAsync(string pageKey);
    Task<List<ModuleLayoutResponse>> BatchSaveAsync(BatchSaveLayoutRequest request);
    Task<List<ModuleLayoutResponse>> ResetAsync(string pageKey);
    Task<List<ModuleLayoutResponse>> AddDefaultsAsync(string pageKey);
}

public class LayoutService : ILayoutService
{
    private readonly AppDbContext _db;

    public LayoutService(AppDbContext db) => _db = db;

    public async Task<List<ModuleLayoutResponse>> GetByPageKeyAsync(string pageKey)
    {
        var modules = await _db.ModuleLayouts
            .Where(m => m.PageKey == pageKey)
            .OrderBy(m => m.SortOrder)
            .Select(m => new ModuleLayoutResponse
            {
                Id = m.Id,
                PageKey = m.PageKey,
                ModuleKey = m.ModuleKey,
                Title = m.Title,
                SortOrder = m.SortOrder,
                IsEnabled = m.IsEnabled,
                Config = m.Config
            })
            .ToListAsync();

        // 自动衬底：如果该页面没有布局配置，自动创建默认布局
        if (modules.Count == 0)
        {
            var defaults = GetDefaultModules(pageKey);
            if (defaults.Count > 0)
            {
                _db.ModuleLayouts.AddRange(defaults);
                await _db.SaveChangesAsync();

                // 重新查询返回
                return await _db.ModuleLayouts
                    .Where(m => m.PageKey == pageKey)
                    .OrderBy(m => m.SortOrder)
                    .Select(m => new ModuleLayoutResponse
                    {
                        Id = m.Id,
                        PageKey = m.PageKey,
                        ModuleKey = m.ModuleKey,
                        Title = m.Title,
                        SortOrder = m.SortOrder,
                        IsEnabled = m.IsEnabled,
                        Config = m.Config
                    })
                    .ToListAsync();
            }
        }

        return modules;
    }

    public async Task<List<ModuleLayoutResponse>> BatchSaveAsync(BatchSaveLayoutRequest request)
    {
        var existing = await _db.ModuleLayouts
            .Where(m => m.PageKey == request.PageKey)
            .ToListAsync();

        _db.ModuleLayouts.RemoveRange(existing);

        var modules = request.Modules.Select((m, i) => new ModuleLayout
        {
            PageKey = request.PageKey,
            ModuleKey = m.ModuleKey,
            Title = m.Title,
            SortOrder = i,
            IsEnabled = m.IsEnabled,
            Config = m.Config
        }).ToList();

        _db.ModuleLayouts.AddRange(modules);
        await _db.SaveChangesAsync();

        return await GetByPageKeyAsync(request.PageKey);
    }

    public async Task<List<ModuleLayoutResponse>> ResetAsync(string pageKey)
    {
        var existing = await _db.ModuleLayouts
            .Where(m => m.PageKey == pageKey)
            .ToListAsync();

        _db.ModuleLayouts.RemoveRange(existing);
        await _db.SaveChangesAsync();

        return new List<ModuleLayoutResponse>();
    }

    public async Task<List<ModuleLayoutResponse>> AddDefaultsAsync(string pageKey)
    {
        var existing = await _db.ModuleLayouts
            .Where(m => m.PageKey == pageKey)
            .AnyAsync();

        if (existing)
        {
            return await GetByPageKeyAsync(pageKey);
        }

        var defaults = GetDefaultModules(pageKey);
        _db.ModuleLayouts.AddRange(defaults);
        await _db.SaveChangesAsync();

        return await GetByPageKeyAsync(pageKey);
    }

    private static List<ModuleLayout> GetDefaultModules(string pageKey)
    {
        return pageKey.ToLower() switch
        {
            // ===== 首页 =====
            "home" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "hero", Title = "首屏横幅", SortOrder = 0, IsEnabled = true, Config = "{\"subtitle\":\"INDEPENDENT CREATOR\",\"title\":\"\",\"description\":\"\",\"subDescription\":\"\",\"buttons\":[{\"text\":\"开始阅读\",\"link\":\"/articles\",\"style\":\"solid\"},{\"text\":\"联系我\",\"link\":\"/about\",\"style\":\"outline\"}],\"backgroundMode\":\"none\",\"gradientColors\":\"primary-to-accent\",\"carouselImages\":[{\"url\":\"https://picsum.photos/1200/600?random=1\"},{\"url\":\"https://picsum.photos/1200/600?random=2\"}],\"carouselInterval\":5000}" },
                new() { PageKey = pageKey, ModuleKey = "hot_tags", Title = "热门标签", SortOrder = 1, IsEnabled = true, Config = "{\"count\":10,\"displayStyle\":\"inline\"}" },
                new() { PageKey = pageKey, ModuleKey = "pinned_posts", Title = "置顶文章", SortOrder = 2, IsEnabled = true, Config = "{\"displayStyle\":\"card\",\"count\":5,\"columns\":3}" },
                new() { PageKey = pageKey, ModuleKey = "featured_posts", Title = "精选文章", SortOrder = 3, IsEnabled = true, Config = "{\"count\":3,\"displayStyle\":\"card\",\"columns\":3}" },
                new() { PageKey = pageKey, ModuleKey = "recent_posts", Title = "最新文章", SortOrder = 4, IsEnabled = true, Config = "{\"count\":5,\"displayStyle\":\"list\"}" },
                new() { PageKey = pageKey, ModuleKey = "projects", Title = "项目展示", SortOrder = 5, IsEnabled = true, Config = "{\"count\":4}" },
                new() { PageKey = pageKey, ModuleKey = "subscription", Title = "邮件订阅", SortOrder = 6, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "image_carousel", Title = "图片轮播", SortOrder = 7, IsEnabled = false, Config = "{\"images\":[{\"url\":\"https://picsum.photos/800/400?random=1\",\"link\":\"/articles\",\"title\":\"欢迎来到我的博客\"},{\"url\":\"https://picsum.photos/800/400?random=2\",\"link\":\"/projects\",\"title\":\"查看我的项目\"}],\"autoPlay\":true,\"interval\":4000,\"height\":400}" },
                new() { PageKey = pageKey, ModuleKey = "statistics", Title = "统计面板", SortOrder = 8, IsEnabled = false, Config = "{\"showArticles\":true,\"showProjects\":true,\"showViews\":true,\"showTags\":true}" },
                new() { PageKey = pageKey, ModuleKey = "video_player", Title = "视频播放", SortOrder = 9, IsEnabled = false, Config = "{\"url\":\"https://www.youtube.com/embed/dQw4w9WgXcQ\",\"aspectRatio\":\"16/9\",\"autoplay\":false}" },
                new() { PageKey = pageKey, ModuleKey = "call_to_action", Title = "号召按钮", SortOrder = 10, IsEnabled = false, Config = "{\"text\":\"联系我\",\"link\":\"/about\",\"description\":\"有任何问题或合作意向，欢迎联系\",\"style\":\"primary\"}" },
                new() { PageKey = pageKey, ModuleKey = "divider", Title = "分割线", SortOrder = 11, IsEnabled = false, Config = "{\"style\":\"solid\",\"margin\":32}" },
                new() { PageKey = pageKey, ModuleKey = "spacer", Title = "间距占位", SortOrder = 12, IsEnabled = false, Config = "{\"height\":40}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 13, IsEnabled = false, Config = "{\"html\":\"<div style=\\\"text-align:center;padding:20px\\\"><h3>自定义区域</h3><p>你可以在这里添加任何 HTML 内容</p></div>\"}" },
            },
            // ===== 文章列表页 =====
            "articles" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "page_hero", Title = "", SortOrder = 0, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "pinned_posts", Title = "置顶文章", SortOrder = 1, IsEnabled = true, Config = "{\"displayStyle\":\"card\",\"count\":5,\"columns\":3}" },
                new() { PageKey = pageKey, ModuleKey = "image_carousel", Title = "顶部轮播", SortOrder = 2, IsEnabled = false, Config = "{\"images\":[{\"url\":\"https://picsum.photos/800/400?random=3\",\"title\":\"示例\"}],\"autoPlay\":true,\"interval\":4000}" },
                new() { PageKey = pageKey, ModuleKey = "search_bar", Title = "搜索筛选条", SortOrder = 3, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "article_list", Title = "文章列表", SortOrder = 4, IsEnabled = true, Config = "{\"viewMode\":\"card\",\"columns\":3,\"pageSize\":12}" },
                new() { PageKey = pageKey, ModuleKey = "sidebar", Title = "右侧边栏", SortOrder = 5, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 6, IsEnabled = false, Config = "{\"html\":\"<p>自定义内容</p>\"}" },
                new() { PageKey = pageKey, ModuleKey = "divider", Title = "分割线", SortOrder = 7, IsEnabled = false, Config = "{\"style\":\"solid\",\"margin\":24}" },
            },
            // ===== 文章详情页 =====
            "article_detail" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "progress_bar", Title = "阅读进度条", SortOrder = 0, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "article_hero", Title = "文章标题区", SortOrder = 1, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "content", Title = "正文区", SortOrder = 2, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "sidebar", Title = "右侧分享栏", SortOrder = 3, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "toc", Title = "浮动目录", SortOrder = 4, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "tags", Title = "标签区", SortOrder = 5, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "prev_next", Title = "上下篇文章", SortOrder = 6, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "related", Title = "相关文章", SortOrder = 7, IsEnabled = true, Config = "{\"count\":3}" },
                new() { PageKey = pageKey, ModuleKey = "comments", Title = "评论区", SortOrder = 8, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 9, IsEnabled = false, Config = "{\"html\":\"<p>自定义内容</p>\"}" },
                new() { PageKey = pageKey, ModuleKey = "divider", Title = "分割线", SortOrder = 10, IsEnabled = false, Config = "{\"style\":\"solid\",\"margin\":24}" },
            },
            // ===== 归档页 =====
            "archive" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "page_hero", Title = "", SortOrder = 0, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "timeline", Title = "时间线列表", SortOrder = 1, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 2, IsEnabled = false, Config = "{\"html\":\"<p>自定义</p>\"}" },
            },
            // ===== 搜索页 =====
            "search" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "search_bar", Title = "搜索条", SortOrder = 0, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "result_stats", Title = "搜索结果统计", SortOrder = 1, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "search_results", Title = "搜索结果", SortOrder = 2, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 3, IsEnabled = false, Config = "{\"html\":\"<p>自定义</p>\"}" },
            },
            // ===== 项目列表 =====
            "projects" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "page_hero", Title = "", SortOrder = 0, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "project_grid", Title = "项目卡片网格", SortOrder = 1, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 2, IsEnabled = false, Config = "{\"html\":\"<p>自定义</p>\"}" },
            },
            // ===== 项目详情 =====
            "project_detail" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "project_hero", Title = "项目 Hero", SortOrder = 0, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "cover", Title = "封面图", SortOrder = 1, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "project_content", Title = "项目介绍", SortOrder = 2, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "tech_stack", Title = "技术栈", SortOrder = 3, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 4, IsEnabled = false, Config = "{\"html\":\"<p>自定义</p>\"}" },
            },
            // ===== 全部分类 =====
            "categories" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "page_hero", Title = "", SortOrder = 0, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "category_grid", Title = "分类卡片", SortOrder = 1, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 2, IsEnabled = false, Config = "{\"html\":\"<p>自定义</p>\"}" },
            },
            // ===== 分类文章 =====
            "category_detail" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "page_hero", Title = "", SortOrder = 0, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "pinned_posts", Title = "置顶文章", SortOrder = 1, IsEnabled = true, Config = "{\"displayStyle\":\"card\",\"count\":5,\"columns\":3}" },
                new() { PageKey = pageKey, ModuleKey = "article_list", Title = "文章列表", SortOrder = 2, IsEnabled = true, Config = "{\"columns\":3}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 3, IsEnabled = false, Config = "{\"html\":\"<p>自定义</p>\"}" },
            },
            // ===== 标签云 =====
            "tags" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "page_hero", Title = "", SortOrder = 0, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "tag_cloud", Title = "标签云", SortOrder = 1, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 2, IsEnabled = false, Config = "{\"html\":\"<p>自定义</p>\"}" },
            },
            // ===== 标签文章 =====
            "tag_detail" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "page_hero", Title = "", SortOrder = 0, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "article_list", Title = "文章列表", SortOrder = 1, IsEnabled = true, Config = "{\"columns\":3}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 2, IsEnabled = false, Config = "{\"html\":\"<p>自定义</p>\"}" },
            },
            // ===== 关于我 =====
            "about" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "page_hero", Title = "", SortOrder = 0, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "profile_sections", Title = "个人模块", SortOrder = 1, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 2, IsEnabled = false, Config = "{\"html\":\"<p>自定义</p>\"}" },
            },
            // ===== 友情链接 =====
            "friends" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "page_hero", Title = "", SortOrder = 0, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "friend_grid", Title = "友链卡片", SortOrder = 1, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 2, IsEnabled = false, Config = "{\"html\":\"<p>自定义</p>\"}" },
            },
            // ===== 留言板 =====
            "guestbook" => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "page_hero", Title = "", SortOrder = 0, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "message_form", Title = "留言表单", SortOrder = 1, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "message_list", Title = "留言列表", SortOrder = 2, IsEnabled = true, Config = "{}" },
                new() { PageKey = pageKey, ModuleKey = "custom", Title = "自定义区域", SortOrder = 3, IsEnabled = false, Config = "{\"html\":\"<p>自定义</p>\"}" },
            },
            _ => new List<ModuleLayout>
            {
                new() { PageKey = pageKey, ModuleKey = "content", Title = "内容区域", SortOrder = 0, IsEnabled = true, Config = "{}" },
            }
        };
    }
}