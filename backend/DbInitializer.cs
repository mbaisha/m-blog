using Microsoft.EntityFrameworkCore;
using Mblog.API.Data;
using Mblog.API.Models.Entities;

namespace Mblog.API;

/// <summary>
/// 数据库初始化器 — 种子数据
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // 如果已有用户，跳过种子
        if (await db.Users.AnyAsync())
            return;

        // ===== 1. 管理员用户 =====
        var admin = new User
        {
            Username = "zhuge",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "super_admin",
            Status = "active",
            Email = "admin@mblog.com",
        };
        db.Users.Add(admin);

        // ===== 2. 分类 =====
        var categories = new[]
        {
            new Category { Name = "技术实践", Slug = "tech", Description ="后端/前端/DevOps 等技术实践", SortOrder = 1 },
            new Category { Name = "产品思考", Slug = "product", Description = "产品设计、用户体验思考", SortOrder = 2 },
            new Category { Name = "读书笔记", Slug = "reading", Description = "阅读记录与心得体会", SortOrder = 3 },
            new Category { Name = "生活随笔", Slug = "life", Description = "日常生活与感悟", SortOrder = 4 },
        };
        db.Categories.AddRange(categories);

        // ===== 3. 标签 =====
        var tags = new[]
        {
            new Tag { Name = "C#", Slug = "csharp", Color = "#6366f1" },
            new Tag { Name = ".NET", Slug = "dotnet", Color = "#512BD4" },
            new Tag { Name = "Next.js", Slug = "nextjs", Color = "#000000" },
            new Tag { Name = "React", Slug = "react", Color = "#61DAFB" },
            new Tag { Name = "TypeScript", Slug = "typescript", Color = "#3178C6" },
            new Tag { Name = "PostgreSQL", Slug = "postgresql", Color = "#336791" },
            new Tag { Name = "Docker", Slug = "docker", Color = "#2496ED" },
            new Tag { Name = "设计模式", Slug = "design-patterns", Color = "#FF6B35" },
            new Tag { Name = "性能优化", Slug = "performance", Color = "#E63946" },
            new Tag { Name = "开源", Slug = "open-source", Color = "#2A9D8F" },
        };
        db.Tags.AddRange(tags);

        await db.SaveChangesAsync();

        // ===== 4. 文章 =====
        var articles = new[]
        {
            new Article
            {
                Title = "使用 .NET 10 构建现代化 Web API",
                Slug = "building-modern-web-api-with-dotnet10",
                Summary = "本文将介绍如何使用 .NET 10 的最新特性来构建高性能、可维护的 Web API，包括 Minimal API、结构化日志、认证鉴权等。",
                Content = @"# 使用 .NET 10 构建现代化 Web API

## 引言

.NET 10 带来了许多令人兴奋的新特性，本文将带你一步步构建一个现代化的 Web API。

## 环境准备

首先，确保你安装了 .NET 10 SDK：

```bash
dotnet --version
# 输出应为 10.x
```

## 创建项目

```bash
dotnet new webapi -n MyApi
cd MyApi
```

## 添加必要的包

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Serilog.AspNetCore
```

## 配置日志

使用 Serilog 提供结构化日志输出：

```csharp
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));
```

## 总结

.NET 10 提供了强大的基础架构来构建 Web API，配合好工具链可以让开发效率大幅提升。
",
                Status = "published",
                CategoryId = categories[0].Id,
                AuthorId = admin.Id,
                IsTop = true,
                IsRecommend = true,
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-2),
                ViewCount = 128,
                CommentCount = 3,
                ArticleTags = new List<ArticleTag>
                {
                    new() { TagId = tags[0].Id },
                    new() { TagId = tags[1].Id },
                    new() { TagId = tags[5].Id },
                },
            },
            new Article
            {
                Title = "Next.js 15 服务端组件深度解析",
                Slug = "nextjs-server-components-deep-dive",
                Summary = "深入理解 Next.js 15 的服务端组件（RSC）架构，探索其工作原理、最佳实践以及与传统 SPA 的区别。",
                Content = @"# Next.js 15 服务端组件深度解析

## 什么是服务端组件

React Server Components (RSC) 允许你在服务端渲染组件，减少客户端 JavaScript 体积。

## 基本用法

```tsx
// 这是一个服务端组件（默认）
async function ArticleList() {
    const articles = await fetch('https://api.example.com/articles')
        .then(r => r.json());
    
    return (
        <ul>
            {articles.map(a => (
                <li key={a.id}>{a.title}</li>
            ))}
        </ul>
    );
}
```

## 数据获取策略

1. **服务端获取** - 直接在组件内 await
2. **静态生成** - 配合 generateStaticParams
3. **增量静态再生成** - ISR 策略

## 总结

服务端组件是 Next.js 的核心优势之一，合理使用可以显著提升应用性能。
",
                Status = "published",
                CategoryId = categories[0].Id,
                AuthorId = admin.Id,
                IsRecommend = true,
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-5),
                ViewCount = 256,
                CommentCount = 5,
                ArticleTags = new List<ArticleTag>
                {
                    new() { TagId = tags[2].Id },
                    new() { TagId = tags[3].Id },
                    new() { TagId = tags[4].Id },
                },
            },
            new Article
            {
                Title = "Docker Compose 实战：从开发到部署",
                Slug = "docker-compose-from-dev-to-prod",
                Summary = "通过一个完整的 Web 应用示例，学习如何使用 Docker Compose 编排多个服务，实现开发环境与生产环境的一致性。",
                Content = @"# Docker Compose 实战：从开发到部署

## 为什么选择 Docker Compose

多服务应用的管理一直是开发运维中的痛点，Docker Compose 提供了简洁的声明式解决方案。

## docker-compose.yml 示例

```yaml
version: '3.9'

services:
  app:
    build: .
    ports:
      - '3000:3000'
    depends_on:
      - db

  db:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: myapp
    volumes:
      - pgdata:/var/lib/postgresql/data

volumes:
  pgdata:
```

## 常用命令

```bash
# 启动所有服务
docker compose up -d

# 查看日志
docker compose logs -f

# 重新构建
docker compose build --no-cache
```

## 总结

Docker Compose 大大简化了多服务应用的本地开发和部署流程。
",
                Status = "published",
                CategoryId = categories[0].Id,
                AuthorId = admin.Id,
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-10),
                ViewCount = 89,
                CommentCount = 2,
                ArticleTags = new List<ArticleTag>
                {
                    new() { TagId = tags[6].Id },
                    new() { TagId = tags[8].Id },
                },
            },
            new Article
            {
                Title = "产品思维：如何设计用户喜欢的功能",
                Slug = "product-thinking-design-user-loved-features",
                Summary = "从用户需求出发，探讨功能设计的核心方法论。结合实际案例，分享产品设计中的思考框架与决策原则。",
                Content = @"# 产品思维：如何设计用户喜欢的功能

## 用户需求金字塔

1. **功能性需求** - 能完成任务
2. **可靠性需求** - 稳定不崩溃
3. **易用性需求** - 直观易上手
4. **愉悦性需求** - 超出预期

## 设计流程

### 1. 用户研究
通过访谈、问卷、数据分析理解真实需求。

### 2. 定义问题
用用户故事描述：作为 [角色]，我希望 [功能]，以便 [价值]。

### 3. 原型验证
快速制作低保真原型进行可用性测试。

## 总结

好的产品设计源于对用户的深刻理解，而不是功能的数量。
",
                Status = "published",
                CategoryId = categories[1].Id,
                AuthorId = admin.Id,
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-15),
                ViewCount = 67,
                CommentCount = 1,
                ArticleTags = new List<ArticleTag>
                {
                    new() { TagId = tags[7].Id },
                },
            },
            new Article
            {
                Title = "《重构：改善既有代码的设计》读书笔记",
                Slug = "refactoring-improving-existing-code-notes",
                Summary = "Martin Fowler 经典著作的阅读笔记，总结重构的核心原则、代码坏味道识别以及常用重构手法。",
                Content = @"# 《重构：改善既有代码的设计》读书笔记

## 核心原则

- 重构前先确保有完善的测试
- 小步前进，每次只做一种重构
- 不要同时重构和添加新功能

## 常见代码坏味道

| 坏味道 | 描述 | 重构手法 |
|--------|------|----------|
| 过长函数 | 一个函数做了太多事 | 提炼函数 |
| 过大的类 | 类承担了过多职责 | 提炼类 |
| 重复代码 | 多处出现相同逻辑 | 提炼函数/子类 |
| 过长参数列表 | 参数过多难以理解 | 引入参数对象 |

## 总结

重构是一项持续的活动，应该融入日常开发习惯中。
",
                Status = "published",
                CategoryId = categories[2].Id,
                AuthorId = admin.Id,
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-20),
                ViewCount = 45,
                CommentCount = 0,
                ArticleTags = new List<ArticleTag>
                {
                    new() { TagId = tags[7].Id },
                    new() { TagId = tags[0].Id },
                },
            },
            new Article
            {
                Title = "2025 年终总结：成长与展望",
                Slug = "2025-year-end-summary",
                Summary = "回顾 2025 年的技术成长、项目经历和生活感悟，展望新一年的目标与规划。",
                Content = @"# 2025 年终总结：成长与展望

## 技术成长

今年主要深耕了以下方向：

- **全栈开发**：从前端到后端再到运维，打通了完整的全栈链路
- **云原生**：深入学习 Docker 和 K8s，实践 CI/CD  pipeline
- **性能优化**：掌握了从数据库到前端渲染的全链路优化方法

## 项目回顾

年内完成了个人博客的重构升级，从一个简单的静态站点演变为功能完整的全栈应用。

## 生活感悟

代码之外，更意识到持续学习和知识输出的重要性。

## 2026 展望

- 学习 Rust，探索系统编程
- 参与开源项目贡献
- 坚持每周写一篇技术博客
",
                Status = "published",
                CategoryId = categories[3].Id,
                AuthorId = admin.Id,
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-30),
                ViewCount = 200,
                CommentCount = 8,
                ArticleTags = new List<ArticleTag>
                {
                    new() { TagId = tags[8].Id },
                },
            },
        };
        db.Articles.AddRange(articles);
        await db.SaveChangesAsync();

        // ===== 5. 评论 =====
        var article1 = articles[0];
        var comment1 = new Comment
        {
            ArticleId = article1.Id,
            Nickname = "张三",
            Content = "写得很棒！对 .NET 10 的新特性讲解得很清楚。",
            Status = "approved",
            IpHash = "abc123",
        };
        db.Comments.Add(comment1);

        var comment2 = new Comment
        {
            ArticleId = article1.Id,
            Nickname = "李四",
            Content = "能否出一篇关于 .NET 10 性能优化的文章？",
            Status = "approved",
            IpHash = "def456",
        };
        db.Comments.Add(comment2);

        var article2 = articles[1];
        db.Comments.Add(new Comment
        {
            ArticleId = article2.Id,
            Nickname = "王五",
            Content = "服务端组件确实好用，但在某些场景下还需要配合客户端组件使用。",
            Status = "approved",
            IpHash = "ghi789",
            ParentId = null,
        });

        await db.SaveChangesAsync();

        // ===== 6. 项目 =====
        var projects = new[]
        {
            new Project
            {
                Title = "MBlog 个人博客系统",
                Slug = "mblog",
                Summary = "基于 .NET 10 + Next.js 15 + Vue 3 的全栈个人博客系统，支持 Markdown 写作、评论互动、SEO 优化、暗黑模式等特性。",
                Content = "一个功能完整的个人博客平台，包含前台展示、后台管理和 RESTful API 三端。采用现代化技术栈，注重性能、安全和用户体验。",
                TechStack = "[\".NET 10\", \"Next.js 15\", \"Vue 3\", \"PostgreSQL\", \"Docker\"]",
                ProjectUrl = "https://github.com/zhuge/mblog",
                GithubUrl = "https://github.com/zhuge/mblog",
                DemoUrl = null,
                IsVisible = true,
                SortOrder = 1,
            },
            new Project
            {
                Title = "TaskFlow 任务管理工具",
                Slug = "taskflow",
                Summary = "轻量级的团队任务管理工具，支持看板视图、甘特图、工时统计等功能，帮助团队高效协作。",
                Content = "TaskFlow 是一个开源的团队协作工具，灵感来自 Trello 和 Jira，但更加轻量易用。",
                TechStack = "[\"React\", \"Node.js\", \"MongoDB\", \"Socket.IO\"]",
                ProjectUrl = null,
                GithubUrl = "https://github.com/zhuge/taskflow",
                DemoUrl = null,
                IsVisible = true,
                SortOrder = 2,
            },
            new Project
            {
                Title = "CodeSnap 代码片段分享",
                Slug = "codesnap",
                Summary = "简洁优雅的代码片段分享平台，支持 200+ 语言的语法高亮，一键分享到社交媒体。",
                Content = "CodeSnap 让你可以快速创建和分享美观的代码片段，支持自定义主题和背景。",
                TechStack = "[\"Vue 3\", \"TypeScript\", \"Redis\", \"MinIO\"]",
                ProjectUrl = "https://codesnap.example.com",
                GithubUrl = "https://github.com/zhuge/codesnap",
                DemoUrl = "https://codesnap-demo.example.com",
                IsVisible = true,
                SortOrder = 3,
            },
        };
        db.Projects.AddRange(projects);

        // ===== 7. 友情链接 =====
        db.Friends.AddRange(
            new Friend { Name = "阮一峰的网络日志", Url = "https://ruanyifeng.com/", IsVisible = true, SortOrder = 1 },
            new Friend { Name = "酷壳", Url = "https://coolshell.cn/", IsVisible = true, SortOrder = 2 },
            new Friend { Name = "Tailwind CSS", Url = "https://tailwindcss.com/", IsVisible = true, SortOrder = 3 },
            new Friend { Name = "Vue.js", Url = "https://vuejs.org/", IsVisible = true, SortOrder = 4 }
        );

        // ===== 8. 静态页面 =====
        db.Pages.AddRange(
            new Page
            {
                Title = "关于本站",
                Slug = "about",
                Content = "这是一个基于 .NET 10 + Next.js 15 + Vue 3 构建的个人博客系统。本站旨在分享技术实践、产品思考和读书心得。",
                Summary = "关于 MBlog 个人博客的介绍",
                Status = "published",
                PublishedAt = DateTimeOffset.UtcNow,
            },
            new Page
            {
                Title = "隐私政策",
                Slug = "privacy",
                Content = "本站尊重并保护用户隐私。我们仅收集必要的浏览数据以改善服务体验（如页面访问统计），不会将您的个人信息出售或共享给第三方。",
                Summary = "隐私政策说明",
                Status = "published",
                PublishedAt = DateTimeOffset.UtcNow,
            }
        );

        // ===== 9. 导航菜单 =====
        db.NavigationItems.AddRange(
            new NavigationItem { Title = "首页", Url = "/", SortOrder = 1, IsVisible = true },
            new NavigationItem { Title = "文章", Url = "/articles", SortOrder = 2, IsVisible = true },
            new NavigationItem { Title = "分类", Url = "/categories", SortOrder = 3, IsVisible = true },
            new NavigationItem { Title = "标签", Url = "/tags", SortOrder = 4, IsVisible = true },
            new NavigationItem { Title = "归档", Url = "/archive", SortOrder = 5, IsVisible = true },
            new NavigationItem { Title = "项目", Url = "/projects", SortOrder = 6, IsVisible = true },
            new NavigationItem { Title = "关于", Url = "/about", SortOrder = 7, IsVisible = true },
            new NavigationItem { Title = "友情链接", Url = "/friends", SortOrder = 8, IsVisible = true },
            new NavigationItem { Title = "搜索", Url = "/search", SortOrder = 9, IsVisible = true }
        );

        // ===== 10. 页脚配置 =====
        db.FooterConfigs.Add(new FooterConfig
        {
            Copyright = $"© {DateTimeOffset.UtcNow.Year} MBlog. All rights reserved.",
            IsVisible = true,
        });

        // ===== 11. 主题设置（6 套预设配色） =====
        db.ThemeSettings.AddRange(
            new ThemeSetting
            {
                ThemeName = "极光紫",
                PrimaryColor = "#6366f1",
                AccentColor = "#a855f7",
                BackgroundColor = "#f8fafc",
                TextColor = "#111827",
                LinkColor = "#6366f1",
                NavbarBackground = "#ffffff",
                NavbarTextColor = "#374151",
                SurfaceColor = "#ffffff",
                TextSecondaryColor = "#6b7280",
                BorderColor = "#e5e7eb",
                SuccessColor = "#10b981",
                DangerColor = "#ef4444",
                WarningColor = "#f59e0b",
                FontFamily = "sans-serif",
                BorderRadius = "medium",
                IsActive = true,
            },
            new ThemeSetting
            {
                ThemeName = "翡翠绿",
                PrimaryColor = "#059669",
                AccentColor = "#10b981",
                BackgroundColor = "#f0fdf4",
                TextColor = "#111827",
                LinkColor = "#059669",
                NavbarBackground = "#ffffff",
                NavbarTextColor = "#374151",
                SurfaceColor = "#ffffff",
                TextSecondaryColor = "#6b7280",
                BorderColor = "#d1fae5",
                SuccessColor = "#10b981",
                DangerColor = "#ef4444",
                WarningColor = "#f59e0b",
                FontFamily = "sans-serif",
                BorderRadius = "medium",
                IsActive = false,
            },
            new ThemeSetting
            {
                ThemeName = "落日橙",
                PrimaryColor = "#f59e0b",
                AccentColor = "#f97316",
                BackgroundColor = "#fffcf5",
                TextColor = "#1c1917",
                LinkColor = "#f59e0b",
                NavbarBackground = "#ffffff",
                NavbarTextColor = "#44403c",
                SurfaceColor = "#ffffff",
                TextSecondaryColor = "#78716c",
                BorderColor = "#fef3c7",
                SuccessColor = "#10b981",
                DangerColor = "#ef4444",
                WarningColor = "#f59e0b",
                FontFamily = "sans-serif",
                BorderRadius = "medium",
                IsActive = false,
            },
            new ThemeSetting
            {
                ThemeName = "海洋蓝",
                PrimaryColor = "#2563eb",
                AccentColor = "#06b6d4",
                BackgroundColor = "#f5f9ff",
                TextColor = "#0f172a",
                LinkColor = "#2563eb",
                NavbarBackground = "#ffffff",
                NavbarTextColor = "#334155",
                SurfaceColor = "#ffffff",
                TextSecondaryColor = "#64748b",
                BorderColor = "#bfdbfe",
                SuccessColor = "#10b981",
                DangerColor = "#ef4444",
                WarningColor = "#f59e0b",
                FontFamily = "sans-serif",
                BorderRadius = "small",
                IsActive = false,
            },
            new ThemeSetting
            {
                ThemeName = "玫瑰红",
                PrimaryColor = "#ec4899",
                AccentColor = "#f43f5e",
                BackgroundColor = "#fff5f8",
                TextColor = "#1f2937",
                LinkColor = "#ec4899",
                NavbarBackground = "#ffffff",
                NavbarTextColor = "#374151",
                SurfaceColor = "#ffffff",
                TextSecondaryColor = "#9ca3af",
                BorderColor = "#fce7f3",
                SuccessColor = "#10b981",
                DangerColor = "#ef4444",
                WarningColor = "#f59e0b",
                FontFamily = "sans-serif",
                BorderRadius = "large",
                IsActive = false,
            },
            new ThemeSetting
            {
                ThemeName = "暗夜黑",
                PrimaryColor = "#818cf8",
                AccentColor = "#c084fc",
                BackgroundColor = "#0f172a",
                TextColor = "#f1f5f9",
                LinkColor = "#818cf8",
                NavbarBackground = "#1e293b",
                NavbarTextColor = "#e2e8f0",
                SurfaceColor = "#1e293b",
                TextSecondaryColor = "#94a3b8",
                BorderColor = "#334155",
                SuccessColor = "#34d399",
                DangerColor = "#fb7185",
                WarningColor = "#fbbf24",
                FontFamily = "sans-serif",
                BorderRadius = "medium",
                IsActive = false,
            }
        );

        // ===== 12. 首页布局 =====
        db.ModuleLayouts.AddRange(
            new ModuleLayout { PageKey = "home", ModuleKey = "hero", Title = "Hero 区域", SortOrder = 1, IsEnabled = true, Config = "{}" },
            new ModuleLayout { PageKey = "home", ModuleKey = "featured-articles", Title = "推荐文章", SortOrder = 2, IsEnabled = true, Config = "{\"count\":6}" },
            new ModuleLayout { PageKey = "home", ModuleKey = "recent-projects", Title = "最近项目", SortOrder = 3, IsEnabled = true, Config = "{\"count\":3}" },
            new ModuleLayout { PageKey = "home", ModuleKey = "categories", Title = "分类展示", SortOrder = 4, IsEnabled = true, Config = "{}" },
            new ModuleLayout { PageKey = "home", ModuleKey = "friends", Title = "友情链接", SortOrder = 5, IsEnabled = true, Config = "{\"count\":8}" }
        );

        // ===== 13. SEO 设置 =====
        db.SeoSettings.AddRange(
            new SeoSetting { PageKey = "home", Title = "个人博客 - 分享技术实践与产品思考", Description = "一个现代个人博客，分享技术实践、产品思考与项目复盘" },
            new SeoSetting { PageKey = "articles", Title = "文章列表 - 个人博客", Description = "浏览所有技术文章" },
            new SeoSetting { PageKey = "projects", Title = "项目展示 - 个人博客", Description = "浏览所有项目作品" }
        );

        // ===== 14. 个人页面模块 =====
        db.ProfileSections.AddRange(
            new ProfileSection
            {
                SectionType = "hero",
                Title = "关于我",
                Content = "你好！我是诸葛，一名全栈开发者。热衷于技术探索和知识分享。",
                SortOrder = 1,
                IsEnabled = true,
            },
            new ProfileSection
            {
                SectionType = "skills",
                Title = "技能栈",
                Content = "",
                SortOrder = 2,
                IsEnabled = true,
                Metadata = "{\"skills\":[\"C# / .NET\", \"TypeScript / JavaScript\", \"React / Next.js\", \"Vue 3\", \"PostgreSQL\", \"Docker\", \"Git\", \"Linux\"]}",
            },
            new ProfileSection
            {
                SectionType = "timeline",
                Title = "工作经历",
                Content = "",
                SortOrder = 3,
                IsEnabled = true,
                Metadata = "{\"items\":[{\"year\":\"2024-至今\",\"title\":\"高级全栈工程师\",\"description\":\"负责核心业务系统的架构设计与开发\"},{\"year\":\"2022-2024\",\"title\":\"全栈工程师\",\"description\":\"参与多个 Web 项目的全流程开发\"},{\"year\":\"2020-2022\",\"title\":\"前端开发工程师\",\"description\":\"负责前端 UI 开发和性能优化\"}]}",
            }
        );

        await db.SaveChangesAsync();

        Console.WriteLine("✓ 种子数据创建成功！管理员: zhuge / admin123");
    }
}