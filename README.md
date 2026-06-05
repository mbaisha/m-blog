# 个人博客平台项目

这是一个 **前后端分离、SEO 友好、具备完整后台管理能力的现代化个人博客与个人品牌展示平台**。

项目目标不是做一个简单的文章列表，而是构建一个可以长期运营的个人内容平台：前台用于展示文章、项目、个人介绍、分类标签、归档、搜索、评论与留言板；后台用于管理文章、Markdown 内容、媒体资源、评论审核、SEO 配置、数据统计与个人品牌模块。

---

## 一、项目定位

本项目同时具备以下能力：

- 个人博客系统。
- 个人品牌官网。
- Markdown 内容管理系统。
- 项目作品集展示平台。
- 图片、视频、附件媒体库。
- 访客评论与反灌水系统。
- SEO 友好的前台展示系统。
- 后台内容管理能力。
- PostgreSQL 数据底座。
- C# / ASP.NET Core Web API 后端。

---

## 二、技术栈

| 层级 | 技术选型 | 说明 |
|---|---|---|
| 前台博客 | Next.js + TypeScript + Tailwind CSS | SSR / SSG、SEO 友好、响应式高颜值前台 |
| 后台管理 | Vue 3 + TypeScript + Element Plus + Vite | 内容管理、媒体管理、评论审核、SEO 配置 |
| 后端 API | C# + ASP.NET Core Web API | 前后端分离 REST API |
| ORM | Entity Framework Core | PostgreSQL 数据访问与迁移 |
| 数据库 | PostgreSQL | UUID、`timestamptz`、`jsonb`、索引、外键约束 |
| 认证 | JWT + Refresh Token | Access Token 短期有效，Refresh Token 可撤销 |
| 参数校验 | FluentValidation | 后端统一请求校验 |
| 日志 | Serilog | 结构化日志与审计追踪 |
| 文件存储 | 本地存储起步 | 后续可接入对象存储与 CDN |
| 部署 | Docker Compose | 本地与生产部署统一编排 |

---

## 三、核心模块

### 1. 前台博客模块

前台面向访客和搜索引擎，重点强调视觉、性能、SEO 和内容浏览体验。

主要页面包括：

- 首页。
- 文章列表。
- 文章详情。
- 分类页。
- 标签页。
- 归档页。
- 搜索页。
- 关于我。
- 项目展示。
- 项目详情。
- 友情链接。
- 留言板。
- 评论提交。

前台设计要求：

- 高颜值、响应式布局（桌面 ≥1024px、平板 768-1023px、手机 <768px）。
- 导航栏支持后台管理菜单项、排序、二级下拉。
- 页脚支持版权、ICP 备案、公安网备案、社交链接等后台管理。
- 首页采用模块引擎架构，模块由后台配置顺序和数据源。
- 自定义页面支持后台创建和发布。
- 每个页面展示阅读数。
- 支持移动端、平板、桌面端。
- 每个页面具备独立 SEO metadata。
- 支持 Open Graph、Twitter Card、JSON-LD。
- 支持 sitemap 与 robots。
- 图片懒加载、视频按需加载。
- Markdown 文章支持代码高亮、目录、表格、引用、任务列表等能力。
- 主题色可由后台自定义，自动应用于导航、按钮、链接等。

### 2. 后台管理模块

后台面向博主和管理员，是完整的 CMS 管理端。

主要页面包括：

- 后台登录。
- 仪表盘。
- 文章列表。
- 新建 / 编辑文章。
- Markdown 编辑器。
- 分类管理。
- 标签管理。
- 评论管理。
- 媒体库。
- 个人页面配置。
- 项目展示管理。
- 友情链接管理。
- SEO 配置。
- 自定义页面管理（创建 / 编辑 / 发布 / 删除）。
- 导航栏管理（菜单树增删改、排序、目标窗口）。
- 页脚管理（区块启停、内容编辑、排序）。
- 首页与内页布局管理（模块配置、排序、数据源）。
- 主题色配置与站点信息管理。
- 访问者记录管理（列表、筛选、清理策略配置）。
- 数据统计。
- 系统设置。

后台设计要求：

- 后台接口必须鉴权。
- Markdown 编辑支持实时预览。
- 支持草稿、预览、发布、定时发布。
- 支持封面图、分类、标签、SEO 字段。
- 支持图片、视频、附件上传。
- 评论支持审核、拒绝、标记垃圾、批量处理。
- 媒体库支持分类、搜索、删除、预览。
- 自定义页面、导航、页脚、布局、主题色、访问记录均有独立管理页面。
- 所有管理页支持筛选、搜索、分页。

### 3. 后端 API 模块

后端采用 C# / ASP.NET Core Web API，提供前台与后台共用的数据接口。

核心能力包括：

- 用户认证。
- JWT 与 Refresh Token。
- 文章管理。
- 分类标签管理。
- 自定义页面管理。
- 导航菜单管理。
- 页脚配置管理。
- 模块布局配置。
- 主题与站点配置。
- 评论提交与审核。
- 验证码校验。
- 媒体上传。
- 个人页面配置。
- 项目展示。
- 友情链接。
- SEO 配置。
- 访问统计。
- 访问记录管理与自动清理。
- 审计日志。

后端要求：

- RESTful API。
- JSON 请求与响应。
- 统一响应结构。
- 统一错误码。
- Controller 只负责请求响应。
- 业务逻辑放入 Application Service。
- 所有外部输入必须校验。
- 所有后台接口必须鉴权。
- 敏感操作写入审计日志。

### 4. 评论与反灌水模块

评论系统必须避免“随便提交、随便灌水”。

评论提交链路：

```text
用户打开文章
  ↓
填写评论
  ↓
前端基础校验
  ↓
获取验证码
  ↓
提交评论 API
  ↓
后端验证码校验
  ↓
后端 IP 频率限制
  ↓
后端内容长度校验
  ↓
后端敏感词过滤
  ↓
后端链接限制
  ↓
后端重复内容检测
  ↓
写入数据库
  ↓
返回提交结果
  ↓
后台审核后可见
```

反灌水策略包括：

- 验证码校验。
- IP 频率限制。
- 同一文章短时间重复提交限制。
- 内容长度校验。
- 纯链接、纯表情、纯空格拒绝。
- 敏感词过滤。
- 链接数量限制。
- 重复内容检测。
- 首次评论默认进入审核。
- 包含链接、邮箱、手机号等内容默认进入审核。
- 后台可标记垃圾评论。

### 5. 媒体上传模块

媒体上传支持图片、视频和附件。

| 类型 | 允许扩展名 | 单文件限制 |
|---|---|---|
| 图片 | jpg, jpeg, png, webp, gif | 10MB |
| 视频 | mp4, mov, webm | 500MB |
| 附件 | pdf, doc, docx, zip | 50MB |

上传安全要求：

- 上传接口必须管理员登录。
- 不允许上传可执行文件。
- 不允许上传脚本文件。
- MIME 类型与扩展名必须一致。
- 不允许用户控制保存路径。
- 不允许覆盖已有文件。
- 上传目录禁止执行脚本。
- 图片建议生成缩略图。
- 视频建议生成封面图。

### 6. 安全与合规模块

安全要求：

- 密码使用强哈希。
- JWT Access Token 短期有效。
- Refresh Token 存入数据库并可撤销。
- 后台接口必须鉴权。
- Markdown 与评论必须清洗。
- 禁止 SQL 拼接。
- 上传目录禁止脚本执行。
- IP 不建议存储明文，建议存储 IP Hash。
- 敏感操作写入审计日志。
- 所有外部输入必须校验。

### 7. 性能与 SEO 模块

性能目标：

| 指标 | 目标 |
|---|---|
| 首页首屏加载 | < 2 秒 |
| 文章详情页首屏加载 | < 1.5 秒 |
| 常规 API 响应 | < 300ms |

SEO 要求：

- 每个页面独立 title。
- 每个页面独立 description。
- canonical URL。
- Open Graph title / description / image。
- Twitter Card。
- JSON-LD。
- sitemap。
- robots。
- 文章页支持 `article:published_time`、`article:modified_time`、`article:author`、`article:section`、`article:tag`。

### 8. 部署与运维模块

第一阶段部署架构：

```text
Nginx
  ├── 前台 Next.js
  ├── 后台 Vue Admin
  └── 后端 ASP.NET Core API

PostgreSQL
本地文件存储
```

后续可扩展：

- CDN。
- 对象存储。
- Redis。
- 日志服务。
- 监控服务。

备份策略：

- 数据库每日备份。
- 保留 7 天。
- 每周保留 4 份。
- 每月保留 12 份。
- 图片、视频、附件单独备份。

---

## 四、文档索引

详细设计文档位于 `docs/blog-platform/` 目录。

| 编号 | 文档 | 说明 |
|---|---|---|
| 00 | `docs/blog-platform/00-README.md` | 文档体系索引 |
| 01 | `docs/blog-platform/01-product-requirements.md` | 产品需求文档 PRD |
| 02 | `docs/blog-platform/02-technical-architecture.md` | 技术架构设计 |
| 03 | `docs/blog-platform/03-database-schema.md` | PostgreSQL 数据库设计 |
| 04 | `docs/blog-platform/04-api-specification.md` | C# Web API 接口规范 |
| 05 | `docs/blog-platform/05-frontend-spec.md` | Next.js 前台前端规格 |
| 06 | `docs/blog-platform/06-admin-spec.md` | Vue3 后台管理规格 |
| 07 | `docs/blog-platform/07-comment-anti-spam.md` | 评论与反灌水设计 |
| 08 | `docs/blog-platform/08-media-upload.md` | 图片、视频、附件上传设计 |
| 09 | `docs/blog-platform/09-security-compliance.md` | 安全与合规设计 |
| 10 | `docs/blog-platform/10-performance-seo.md` | 性能与 SEO 设计 |
| 11 | `docs/blog-platform/11-deployment-devops.md` | 部署、运维与备份设计 |
| 12 | `docs/blog-platform/12-implementation-roadmap.md` | 开发阶段、任务拆分与验收标准 |
| 13 | `docs/blog-platform/13-coding-conventions.md` | AI 编程约定与代码规范 |

---

## 五、推荐仓库结构

```text
personal-blog-platform/
├── apps/
│   ├── web/                 # Next.js 前台博客
│   ├── admin/               # Vue 3 后台管理
│   └── api/                 # C# ASP.NET Core Web API
├── docs/
│   └── blog-platform/       # 产品、架构、数据库、API、前端、后台、部署等文档
├── docker/
│   ├── nginx/
│   ├── api/
│   └── postgres/
├── docker-compose.yml
├── README.md
├── .env.example
└── .gitignore
```

---

## 六、AI 编程执行顺序

后续进入代码生成阶段时，建议按以下顺序执行：

1. 阅读 `docs/blog-platform/01-product-requirements.md`，确认业务目标。
2. 阅读 `docs/blog-platform/02-technical-architecture.md`，确认整体架构。
3. 根据 `docs/blog-platform/03-database-schema.md` 生成 PostgreSQL 初始化 SQL 或 EF Core 实体。
4. 根据 `docs/blog-platform/04-api-specification.md` 生成 C# ASP.NET Core API 项目骨架。
5. 根据 `docs/blog-platform/05-frontend-spec.md` 生成 Next.js 前台项目骨架。
6. 根据 `docs/blog-platform/06-admin-spec.md` 生成 Vue3 + Element Plus 后台项目骨架。
7. 根据 `docs/blog-platform/07-comment-anti-spam.md` 实现评论、验证码、频率限制、内容过滤。
8. 根据 `docs/blog-platform/08-media-upload.md` 实现图片、视频、附件上传与媒体库。
9. 根据 `docs/blog-platform/09-security-compliance.md` 做安全检查。
10. 根据 `docs/blog-platform/10-performance-seo.md` 做性能与 SEO 优化。
11. 根据 `docs/blog-platform/11-deployment-devops.md` 生成 Docker Compose 与部署配置。
12. 根据 `docs/blog-platform/12-implementation-roadmap.md` 分阶段验收。
13. 根据 `docs/blog-platform/13-coding-conventions.md` 保持代码风格一致。

---

## 七、下一步可以做什么

当前文档阶段已经完成。下一步建议进入代码阶段，可选方向：

1. 根据数据库设计生成完整 PostgreSQL SQL 初始化脚本。
2. 生成 C# / ASP.NET Core Web API 项目骨架。
3. 生成 Entity Framework Core 实体与 DbContext。
4. 生成 API DTO、Controller、Service、Repository。
5. 生成 Next.js 前台博客项目骨架。
6. 生成 Vue3 + Element Plus 后台管理项目骨架。
7. 生成 Docker Compose 部署配置。
8. 生成 Phase 0 / Phase 1 的具体开发任务清单。
