<div align="center">

[English](README_EN.md) | **中文**

</div>

---

<div align="center">

# MBlog 📝

**一个 SEO 友好、功能完备的现代化个人博客与作品集平台，支持前后端分离架构与完整后台管理**

[![Docker](https://img.shields.io/badge/deploy-Docker%20Compose-2496ED?logo=docker&logoColor=white)](https://docs.docker.com/compose/)
[![Next.js](https://img.shields.io/badge/frontend-Next.js%2016-000000?logo=next.js)](https://nextjs.org/)
[![Vue 3](https://img.shields.io/badge/admin-Vue%203%20%2F%20Element%20Plus-4FC08D?logo=vue.js)](https://vuejs.org/)
[![ASP.NET](https://img.shields.io/badge/backend-ASP.NET%20Core-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/database-PostgreSQL-4169E1?logo=postgresql)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

</div>

---

## 📖 项目简介

MBlog 是一个**可直接上线的开源个人博客平台**，采用三层架构：**Next.js** 前台（SSR/SSG 提升 SEO），**Vue 3 + Element Plus** 后台管理系统，以及 **C# ASP.NET Core Web API** 后端。适合希望自建、可扩展、高颜值个人网站的开发者与内容创作者。

---

## ✨ 功能特性

### 🌐 前台博客

| 功能 | 说明 |
|---|---|
| **首页模块引擎** | 首页模块化展示（Hero、最新文章、项目、分类、标签、订阅、统计面板），后台可拖拽配置顺序与启停 |
| **文章列表与详情** | 分页文章列表，详情页支持 Markdown 渲染、代码高亮、目录、阅读进度条、上下篇导航、相关文章 |
| **分类与标签页** | 分类树与标签云导航文章 |
| **日期归档** | 按年份/月份归档时间线 |
| **全文搜索** | 实时文章搜索，带结果统计 |
| **项目作品集** | 展示项目封面、技术栈、Markdown 详细介绍 |
| **关于/个人页面** | 模块化个人介绍页（简介、技能、经历、教育、项目） |
| **留言板** | 访客留言板，支持验证码与后台审核 |
| **友情链接** | 友链展示 |
| **自定义页面** | 后台创建并发布独立页面（隐私政策、条款等） |
| **SEO 优化** | 每页独立标题/描述/关键词、Open Graph、Twitter Card、JSON-LD、Canonical URL，内置 sitemap.xml 与 robots.txt |
| **响应式布局** | 自适应桌面、平板、移动端，支持深色模式 |
| **动态导航** | 多级导航菜单，支持新窗口打开，后台可管理排序 |
| **动态页脚** | 可配置页脚区块（版权、ICP 备案、公安网备案、社交链接等） |
| **评论系统** | 验证码校验、IP 频率限制、内容过滤、后台审核可见 |
| **阅读统计** | 隐私友好的页面阅读数统计 |

### 🛠 后台管理

| 功能 | 说明 |
|---|---|
| **仪表盘** | 概览统计：文章数、项目数、评论数、访问趋势 |
| **文章管理** | 完整 CRUD，Markdown 编辑器，草稿/发布/定时发布，封面图，分类标签，自定义 Slug，SEO 字段 |
| **Markdown 编辑器** | 实时预览，图片/视频/附件上传，媒体库集成，内置 AI 工具栏（7 个功能按钮） |
| **AI 写作助手** | 一键写文、AI 润色（可选择是否插入配图）、SEO 提取、封面图生成 |
| **AI 批量填充** | 批量处理所选文章：自动提取 SEO → 生成封面 → 更新文章，含进度弹窗与自动重试（3 次） |
| **AI 对话** | 后台内置 AI 对话助手 |
| **AI 文生图** | 独立文生图页面，可配置模型 |
| **分类与标签管理** | 支持多级分类（父子级） |
| **媒体库** | 图片/视频/附件上传、预览、搜索、分类管理 |
| **评论管理** | 审核、拒绝、标记垃圾、批量操作，展示 IP 与地理位置 |
| **留言管理** | 留言列表、回复、删除、审核 |
| **自定义页面管理** | 创建编辑发布/草稿独立页面，支持 SEO 字段 |
| **个人页面配置** | 管理关于页的各个区块（简介、技能、经历等） |
| **项目管理** | 作品集 CRUD，支持封面、技术栈、Markdown 内容 |
| **友情链接管理** | 管理友链 Logo、描述、排序 |
| **导航管理** | 树形编辑器，拖拽排序（SortableJS），多级菜单，支持新窗口打开 |
| **页脚管理** | 启停、编辑、排序各个页脚区块 |
| **SEO 配置** | 全局与每页 SEO 设置，一键生成 Sitemap |
| **主题配置** | 自定义主色/背景色/文字色、深色模式配色、站点名称、Logo、Favicon、社交链接 |
| **布局管理** | 每页独立模块布局配置（首页、文章列表、文章详情、归档等），拖拽排序 |
| **系统设置** | 站点名称/描述、ICP 备案号、公安网备案号、评论自动审核开关、订阅开关、访问记录保留天数 |
| **访问记录** | 分页访客记录（IP、地理位置、浏览器、访问页面、时间），自动清理策略 |
| **数据统计** | 按日/周/月的图表趋势数据 |
| **邮件系统** | SMTP 设置、HTML 邮件模板编辑、发送记录、订阅者管理、每周摘要 |
| **订阅管理** | 邮件订阅者列表、状态管理、退订 |
| **大模型设置** | 可配置 LLM 提供商（腾讯混元、OpenAI 兼容）、模型、温度、最大 Token、超时时间 |
| **文生图设置** | 可配置文生图提供商 |
| **修改密码** | 已认证用户的密码更新 |
| **角色鉴权** | 基于 JWT + Refresh Token 的后台接口权限控制 |

### ⚙️ 后端 API（30+ 个控制器）

| 模块 | 控制器 |
|---|---|
| **认证** | `AuthController` — 登录、刷新 Token、退出、当前用户 |
| **文章** | `ArticleController`, `PublicArticleController` — 完整 CRUD + 公开列表、详情、上下篇、相关文章、阅读计数 |
| **分类** | `CategoryController`, `PublicCategoryController` — CRUD、树形结构、公开列表 |
| **标签** | `TagController`, `PublicTagController` — CRUD、公开列表 |
| **评论** | `CommentController`, `AdminCommentController` — 提交、列表、审核、拒绝、垃圾标记、批量操作 |
| **留言** | `MessageController`, `AdminMessageController` — 提交、列表、回复、删除 |
| **媒体上传** | `UploadController` — 图片/视频/附件上传（带校验） |
| **自定义页面** | `PageController`, `PublicPageController` — CRUD、发布、公开访问 |
| **个人页面** | `ProfileController`, `PublicProfileController` — 关于页区块管理 |
| **项目** | `ProjectController`, `PublicProjectController` — CRUD、公开列表+详情 |
| **导航** | `NavigationController`, `PublicNavigationController` — 树形 CRUD、按位置公开查询 |
| **页脚** | `FooterController`, `PublicFooterController` — 配置管理、公开查询 |
| **SEO** | `SeoController`, `AdminSeoController`, `SitemapController` — 设置管理、sitemap.xml、robots.txt |
| **站点设置** | `SiteSettingController` — 系统配置 CRUD |
| **主题** | `ThemeController` — 主题设置 CRUD |
| **布局** | `LayoutController`, `PublicLayoutController` — 模块布局 CRUD |
| **仪表盘** | `DashboardController` — 聚合统计 |
| **访问统计** | `VisitController`, `PublicVisitController` — 上报、计数、分页记录、统计、清理 |
| **归档** | `PublicArchiveController` — 年月时间线 |
| **友情链接** | `FriendController`, `PublicFriendController` — CRUD、公开列表 |
| **验证码** | `CaptchaController` — 验证码生成与校验 |
| **大模型/AI** | `LlmController` — 模型配置、AI 文章/SEO/封面/润色 |
| **文生图** | `ImageGenController` — 文生图配置与生成 |
| **邮件** | `EmailSettingController`, `EmailLogController` — SMTP 配置、模板、发送记录 |
| **订阅** | `SubscriptionController` — 订阅、退订、列表、每周摘要触发 |

### 🤖 AI 集成

| 功能 | 说明 |
|---|---|
| **AI 一键写文** | 根据标题/主题通过大模型一键生成完整文章 |
| **AI 润色** | 改写和增强现有内容，可选择是否插入配图（保留原图） |
| **AI SEO** | 自动从文章内容提取关键词与描述 |
| **AI 封面图** | 根据文章内容通过文生图 API 自动生成封面 |
| **AI 批量填充** | 批量处理文章：SEO 提取 → 封面生成 → 更新，含进度弹窗与自动重试（3 次） |
| **AI 对话** | 后台助手中的 AI 对话功能 |
| **AI 文生图** | 后台独立文生图页面 |
| **提供商支持** | 可配置腾讯混元、OpenAI 兼容接口，LLM 与文生图独立配置 |

### 🛡️ 安全

| 功能 | 说明 |
|---|---|
| **密码强哈希** | 用户凭证使用强哈希算法 |
| **JWT 认证** | 短期 Access Token + 数据库中可撤销的 Refresh Token |
| **角色授权** | 后台接口必须鉴权，支持 super_admin/admin 角色 |
| **输入校验** | 所有外部输入使用 FluentValidation 校验 |
| **防 SQL 注入** | EF Core 参数化查询 |
| **评论反灌水** | 验证码、IP 频率限制、内容过滤、链接限制、重复检测 |
| **防盗链** | Nginx valid_referers + 警告 SVG 保护上传媒体 |
| **上传安全** | MIME 校验、上传目录禁止执行脚本 |
| **审计日志** | Serilog 记录敏感操作审计轨迹 |
| **IP 隐私** | 访客 IP 哈希存储 |
| **跨域配置** | 可配置的 CORS 策略 |
| **频率限制** | 内置 API 频率限制 |

### 🔧 性能与 SEO

| 功能 | 说明 |
|---|---|
| **SSR / SSG** | Next.js 服务端渲染与静态生成 |
| **每页独立 SEO** | 每页独立标题、描述、关键词 |
| **Open Graph** | og:title, og:description, og:image, og:type 社交分享标签 |
| **Twitter Card** | Twitter 卡片元标签 |
| **JSON-LD** | 文章与页面的结构化数据 |
| **Sitemap** | 自动生成 sitemap.xml（含静态+动态路由） |
| **Robots.txt** | 可配置的爬虫规则 |
| **图片懒加载** | 原生图片懒加载 |
| **视频按需加载** | 视频在可见时加载 |
| **缓存策略** | 静态资源不可变缓存、响应缓存 |
| **旧地址跳转** | `/index.php/archives/:id/` → `/articles/show-:id` 重定向 |

### 📦 部署运维

| 功能 | 说明 |
|---|---|
| **Docker Compose** | 多服务编排（Nginx、前台、后台、API、PostgreSQL） |
| **Nginx 反向代理** | SSL 终止、负载均衡、防盗链、上传大小限制（210MB） |
| **健康检查** | 所有服务就绪/存活探针 |
| **数据库备份** | 自动备份脚本，备份轮转（每日 7 份、每周 4 份、每月 12 份） |
| **环境配置** | `.env.example` 模板，包含所有可配置变量 |
| **容器化构建** | 多阶段 Dockerfile 优化镜像体积 |

---

## 🚀 快速开始

### 前置要求

- [Docker](https://docs.docker.com/get-docker/) & [Docker Compose](https://docs.docker.com/compose/install/)

### 一键部署

```bash
# 克隆仓库
git clone https://github.com/mbaisha/m-blog.git
cd m-blog

# 复制并配置环境变量
cp .env.example .env
# 编辑 .env 文件，填入数据库密码、JWT 密钥、AI API 密钥等配置

# 启动所有服务
docker compose up -d

# 服务访问地址:
# 前台博客: http://localhost:3000
# 后台管理: http://localhost:3001/admin
# 后端 API: http://localhost:5000/api
```

完整的环境变量配置请参考 [deploy/.env.example](deploy/.env.example)。

> **默认管理员账号**: 请查看 `backend/DbInitializer.cs` 中的数据库初始化脚本。

---

## 🛠 技术栈

| 层级 | 技术 | 用途 |
|---|---|---|
| **前台** | [Next.js 16](https://nextjs.org/) + TypeScript + [Tailwind CSS 4](https://tailwindcss.com/) | SSR/SSG 博客前台，SEO 优化 |
| **后台** | [Vue 3](https://vuejs.org/) + TypeScript + [Element Plus](https://element-plus.org/) + [Vite](https://vitejs.dev/) | 功能完整的后台管理界面 |
| **后端** | [C#](https://dotnet.microsoft.com/) + [ASP.NET Core Web API](https://dotnet.microsoft.com/en-us/apps/aspnet/apis) | RESTful API 层 |
| **ORM** | [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) | 数据访问与迁移（15+ 迁移） |
| **数据库** | [PostgreSQL](https://www.postgresql.org/) | 主数据存储（UUID、timestamptz、jsonb） |
| **认证** | JWT + Refresh Token | 无状态 API 认证 |
| **校验** | [FluentValidation](https://docs.fluentvalidation.net/) | 请求参数校验 |
| **日志** | [Serilog](https://serilog.net/) | 结构化日志与审计 |
| **图表** | [ECharts](https://echarts.apache.org/) | 仪表盘数据可视化 |
| **部署** | [Docker Compose](https://docs.docker.com/compose/) | 多服务编排 |
| **反向代理** | [Nginx](https://nginx.org/) | 负载均衡、SSL、防盗链 |
| **编辑器** | [md-editor-v3](https://imzbf.github.io/md-editor-v3/) | Markdown 编辑，集成自定义 AI 工具栏 |
| **拖拽排序** | [SortableJS](https://sortablejs.org/) | 导航与布局拖拽排序 |

---

## 📁 项目结构

```
m-blog/
├── frontend/                      # Next.js 博客前台 (SSR/SSG)
│   ├── src/
│   │   ├── app/                   # App Router 页面
│   │   │   ├── page.tsx           # 首页
│   │   │   ├── articles/          # 文章列表 + 详情
│   │   │   ├── categories/        # 分类页
│   │   │   ├── tags/              # 标签页
│   │   │   ├── archive/           # 归档
│   │   │   ├── search/            # 搜索
│   │   │   ├── projects/          # 项目集
│   │   │   ├── about/             # 关于/个人页
│   │   │   ├── friends/           # 友链
│   │   │   ├── guestbook/         # 留言板
│   │   │   ├── [slug]/            # 自定义页面
│   │   │   ├── sitemap.ts         # 动态站点地图
│   │   │   └── layout.tsx         # 根布局（导航栏+页脚）
│   │   ├── components/            # 可复用 UI 组件
│   │   └── lib/                   # API 客户端与工具
│   └── Dockerfile
│
├── admin/                         # Vue 3 后台管理
│   ├── src/
│   │   ├── views/                 # 33 个页面视图
│   │   ├── components/            # 可复用组件（MarkdownEditor 等）
│   │   ├── api/                   # API 客户端模块
│   │   ├── stores/                # Pinia 状态管理
│   │   └── router/                # 路由定义
│   └── Dockerfile
│
├── backend/                       # ASP.NET Core Web API
│   ├── Mblog.API/
│   │   ├── Controllers/           # 30+ API 控制器
│   │   ├── Common/                # ApiResponse、ExceptionMiddleware 等
│   │   ├── Models/
│   │   │   ├── Entities/          # 30+ 实体类
│   │   │   └── DTOs/              # 请求/响应 DTO
│   │   ├── Services/              # 27+ 应用服务
│   │   ├── Data/                  # DbContext、EF Core 迁移
│   │   ├── Validators/            # FluentValidation 校验器
│   │   └── Program.cs             # 启动与 DI 配置
│   └── Dockerfile
│
├── deploy/                        # 部署配置
│   ├── nginx.conf                 # Nginx 反向代理配置
│   ├── .env.example               # 生产环境变量模板
│   ├── hotlink-warning.svg        # 防盗链警告图片
│   └── backup.sh                  # 数据库备份脚本
│
├── docs/                          # 设计文档
│   └── blog-platform/             # 14 份详细设计文档
│
├── docker-compose.yml             # 生产编排
├── .env.example                   # 根目录环境变量模板
└── README.md
```

---

## 🗄️ 数据模型（30+ 实体）

```
User（用户）, RefreshToken（刷新令牌）, AuditLog（审计日志）
Article（文章）, ArticleCategory（文章分类关联）, ArticleTag（文章标签关联）
Category（分类，自引用父子级）, Tag（标签）
Comment（评论）, Message（留言）, CaptchaSession（验证码会话）
Media（媒体文件，图片/视频/附件）
Page（自定义页面）, ProfileSection（个人页面区块）
Project（项目）, Friend（友情链接）, NavigationItem（导航项）
FooterConfig（页脚配置）, SiteSetting（站点设置）, SeoSetting（SEO 设置）
ThemeSetting（主题设置）, ModuleLayout（模块布局）
Visit（访问记录）, Like（点赞）
LlmConfig（大模型配置）, ImageGenConfig（文生图配置）
Subscriber（订阅者）, EmailSetting（邮件设置）, EmailTemplate（邮件模板）, EmailLog（邮件日志）
```

---

## 🧩 核心模块详解

### 🎨 主题系统
- 可自定义主色、背景色、文字颜色
- 深色模式独立颜色配置
- 站点名称、Logo、Favicon、社交链接（GitHub、Twitter、微信等）
- ICP 备案号、公安网备案号

### 📧 邮件系统
- SMTP 邮件服务器配置（支持 SSL/TLS）
- HTML 邮件模板编辑器（支持变量）
- 邮件发送记录追踪
- 前台邮件订阅表单
- 每周摘要定时任务

### 📐 布局引擎
- 每页独立模块配置（首页、文章列表、文章详情、分类、标签、归档、搜索、项目、项目详情）
- 拖拽排序
- 模块级配置项（数量、列数、显示样式等）
- 模块启停控制

### 🧹 定时任务
- **访问记录清理**：自动清理超过保留天数的访问记录
- **每周摘要**：生成并发送每周摘要邮件给订阅者

---

## 📊 架构图

```
                    ┌──────────────┐
                    │    浏览器     │
                    └──────┬───────┘
                           │
                    ┌──────▼───────┐
                    │    Nginx     │  反向代理、SSL、防盗链
                    └──┬───────┬───┘
                       │       │
              ┌────────▼─┐  ┌──▼──────────┐
              │  Next.js  │  │  Vue Admin  │
              │  前台博客  │  │  后台管理    │
              │  :3000    │  │  :3001      │
              └────────┬──┘  └──┬──────────┘
                       │        │
              ┌────────▼────────▼──┐
              │  ASP.NET Core API  │
              │  :5000             │
              └────────┬───────────┘
                       │
              ┌────────▼───────────┐
              │    PostgreSQL      │
              │    :5432           │
              └────────────────────┘
```

---

## 🔧 配置说明

### 环境变量（Docker Compose 部署使用）

| 变量 | 说明 |
|---|---|
| `DB_HOST` | PostgreSQL 数据库主机地址 |
| `DB_PORT` | PostgreSQL 数据库端口（默认 5432） |
| `DB_NAME` | PostgreSQL 数据库名称 |
| `DB_USER` | PostgreSQL 数据库用户 |
| `DB_PASSWORD` | PostgreSQL 数据库密码 |
| `JWT_KEY` | JWT 签名密钥（至少 32 个字符） |
| `SITE_URL` | 站点访问 URL（如 `http://blog.example.com`） |
| `SITE_NAME` | 站点名称 |
| `API_BASE_URL` | API 地址（如 `http://localhost:5000/api`） |

### 环境变量（后端直接配置，使用 `__` 代替 `:`）

| 变量 | 对应配置 | 说明 |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | `ConnectionStrings:DefaultConnection` | PostgreSQL 连接字符串（设置此项将覆盖 DB_HOST 等独立变量） |
| `Jwt__Key` | `Jwt:Key` | JWT 签名密钥 |
| `Jwt__Issuer` | `Jwt:Issuer` | JWT 签发者 |
| `Jwt__Audience` | `Jwt:Audience` | JWT 受众 |
| `Jwt__AccessTokenExpirationMinutes` | `Jwt:AccessTokenExpirationMinutes` | Access Token 过期时间（分钟，默认 120） |
| `Jwt__RefreshTokenExpirationDays` | `Jwt:RefreshTokenExpirationDays` | Refresh Token 过期时间（天，默认 7） |
| `Storage__RootPath` | `Storage:RootPath` | 上传文件存储路径（默认 `./uploads`） |
| `Cors__AllowedOrigins` | `Cors:AllowedOrigins` | 允许的 CORS 来源（逗号分隔） |

### 数据库实体配置（通过后台管理界面设置）

以下配置项**不是环境变量**，而是存储在数据库中的实体，通过后台管理页面进行配置：

- **大模型配置（LLM）** — 进入后台「大模型设置」页面配置提供商、模型、API 密钥等
- **文生图配置（ImageGen）** — 进入后台「文生图设置」页面配置
- **邮件配置（SMTP）** — 进入后台「邮件设置」页面配置服务器、端口、账号密码
- **站点设置** — 进入后台「系统设置」页面配置站点名称、备案号、评论策略等
- **主题设置** — 进入后台「主题配置」页面自定义颜色、Logo 等

完整配置请参考 [backend/.env.example](backend/.env.example)、[deploy/.env.example](deploy/.env.example) 与 [.env.example](.env.example)。

---

## 🤝 贡献指南

欢迎贡献代码！

1. Fork 本仓库
2. 创建功能分支：`git checkout -b feat/your-feature`
3. 提交变更：`git commit -m 'feat: add some feature'`
4. 推送到分支：`git push origin feat/your-feature`
5. 提交 Pull Request

请遵循现有的代码风格与项目结构。

---

## 📄 许可证

本项目采用 **MIT 许可证**，详情请参阅 [LICENSE](LICENSE) 文件。

---

## 🌟 支持

如果您觉得这个项目有帮助，请在 GitHub 上点个 ⭐ 吧！

问题反馈与功能建议请提交 [GitHub Issues](https://github.com/mbaisha/m-blog/issues)。
