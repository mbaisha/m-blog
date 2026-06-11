# 个人博客平台文档索引

## 项目定位

本项目是一个 **前后端分离的现代化个人博客与个人品牌展示平台**。

它不是传统意义上的简单博客，而是同时具备：

- 高质量个人博客能力
- 个人品牌官网能力
- 项目作品集展示能力
- Markdown 内容管理能力
- 图片、视频、附件上传能力
- 访客评论与反灌水能力
- 强大的后台管理能力
- SEO 友好的前台展示能力
- PostgreSQL 数据库支撑能力
- C# / ASP.NET Core 后端 API 能力

---

## 技术栈总览

| 层级 | 技术 |
|---|---|
| 前台博客 | Next.js + TypeScript + Tailwind CSS |
| 后台管理 | Vue 3 + TypeScript + Element Plus + Vite |
| 后端 API | C# + ASP.NET Core Web API |
| ORM | Entity Framework Core |
| 数据库 | PostgreSQL |
| 认证 | JWT + Refresh Token |
| 文件存储 | 本地存储起步，后续可接对象存储 + CDN |
| 部署 | Docker Compose |
| 日志 | Serilog |
| 校验 | FluentValidation |
| SEO | SSR / SSG + Meta + Sitemap + Robots + JSON-LD |

---

## 文档目录

| 编号 | 文档 | 说明 |
|---|---|---|
| 00 | `00-README.md` | 当前文档，项目总索引 |
| 00T | `00-design-tokens.md` | **统一设计系统**（色板、字体、间距、组件令牌、Dark 模式） |
| 00R | `00-responsive.md` | **响应式设计规范**（全端适配规则） |
| 01 | `01-product-requirements.md` | 产品需求文档 PRD |
| 02 | `02-technical-architecture.md` | 技术架构设计 |
| 03 | `03-database-schema.md` | PostgreSQL 数据库设计 |
| 04 | `04-api-specification.md` | C# Web API 接口规范 |
| 05 | `05-frontend-spec.md` | Next.js 前台前端规格（索引各页面设计） |
| 06 | `06-admin-spec.md` | Vue3 后台管理规格 |
| 07 | `07-comment-anti-spam.md` | 评论与反灌水设计 |
| 08 | `08-media-upload.md` | 图片、视频、附件上传设计 |
| 09 | `09-security-compliance.md` | 安全与合规设计 |
| 10 | `10-performance-seo.md` | 性能与 SEO 设计 |
| 11 | `11-deployment-devops.md` | 部署、运维与备份设计 |
| 12 | `12-implementation-roadmap.md` | 开发阶段、任务拆分与验收标准 |
| 13 | `13-coding-conventions.md` | AI 编程约定与代码规范 |
| 14 | `14-detail-page-redesign.md` | **详情页重新设计**（双栏+浮动TOC） |
| 15 | `15-list-page-redesign.md` | **列表页重新设计**（可折叠侧栏+双视图） |
| 16 | `16-homepage-redesign.md` | **首页重新设计**（标题行搜索+导航+Hero+精选） |
| 17 | `17-search-page.md` | **搜索页**（基于列表页扩展） |
| 18 | `18-custom-page.md` | **自定义页面**（轻量内容展示） |
| 19 | `19-archive-page.md` | **归档页**（时间线视图） |
| 20 | `20-projects-page.md` | **项目列表**（卡片网格） |
| 21 | `21-project-detail-page.md` | **项目详情**（Hero+截图+介绍） |
| 22 | `22-message-page.md` | **留言板**（表单+留言列表） |
| 23 | `23-friends-page.md` | **友情链接**（友链卡片） |
| 24 | `24-categories-page.md` | **全部分类**（分类卡片） |
| 25 | `25-tags-page.md` | **标签云**（彩色标签） |
| 26 | `26-404-page.md` | **404 页面**（错误提示+引导） |

---

## AI 编程使用建议

这些文档适合直接交给 AI 编程助手使用。推荐执行顺序：

1. 先阅读 `01-product-requirements.md`，理解业务目标。
2. 再阅读 `02-technical-architecture.md`，确认整体架构。
3. 根据 `03-database-schema.md` 创建数据库（含自定义页面、导航、页脚、布局、主题、访问记录等新表）。
4. 根据 `04-api-specification.md` 编写 C# Web API（含 Pages、Navigation、Footer、Theme、Layout、Visits、Settings 接口）。
5. 根据 `05-frontend-spec.md` 编写 Next.js 前台（含自定义页面路由、模块引擎、导航栏组件、页脚组件、主题色应用、访问统计上报、响应式三端适配）。
6. 根据 `06-admin-spec.md` 编写 Vue3 管理后台（含自定义页面管理、导航管理、页脚管理、布局配置、主题配置、访问记录管理）。
7. 根据 `07-comment-anti-spam.md` 和 `08-media-upload.md` 实现关键安全能力。
8. 根据 `09-security-compliance.md` 做安全检查（含数据保留、访问记录自动清理、页脚链接安全）。
9. 根据 `10-performance-seo.md` 做性能与 SEO 优化（含自定义页面 SEO、阅读数、Sitemap）。
10. 根据 `11-deployment-devops.md` 部署。
11. 根据 `12-implementation-roadmap.md` 分阶段验收。
12. 根据 `13-coding-conventions.md` 保持代码风格一致。

---

## 全局设计原则

### 0. 三端适配

前台必须同时适配桌面端（≥1024px）、平板端（768-1023px）和手机端（<768px），保证内容在所有设备上浏览体验一致、布局正常、导航可用。

### 1. 模块化布局

首页、文章列表页、自定义页面等均采用模块引擎架构。后台配置每个页面类型的模块列表、顺序、显示参数和数据源，前台根据配置动态渲染。不需要修改代码即可调整首页展示内容。

### 2. 前台 SEO 优先

前台必须保证搜索引擎可抓取：

- 使用 SSR 或 SSG。
- 每个页面具备独立 SEO 信息。
- 自动生成 sitemap。
- 自动生成 robots。
- 图片具备 alt。
- URL 结构清晰。

### 2. 后台能力完整

后台不只是文章编辑器，而是完整 CMS：

- 文章
- 分类
- 标签
- 评论
- 媒体
- 个人页面
- 项目展示
- 友情链接
- SEO
- 数据统计
- 权限

### 3. 安全默认开启

所有用户输入都必须视为不可信：

- 评论需要验证。
- 上传需要校验。
- Markdown 需要清洗。
- 后台接口需要鉴权。
- 敏感操作需要日志。

### 4. PostgreSQL 作为长期数据底座

数据库设计要面向生产环境：

- 使用 UUID 主键。
- 使用外键约束。
- 使用索引优化查询。
- 使用 `jsonb` 存储可扩展配置。
- 使用 `timestamptz` 存储时间。
- 使用迁移管理结构变化。

### 5. 适合 AI 编程

所有文档都遵循以下原则：

- 模块边界清晰。
- 数据结构明确。
- API 行为明确。
- 校验规则明确。
- 异常处理明确。
- 验收标准明确。
- 开发任务可拆分。

---

## 项目最终目标

项目完成后应达到以下状态：

- 访客可以流畅浏览博客、文章、分类、标签、项目和个人介绍。
- 访客可以提交评论，但必须经过验证、限流、过滤和审核机制。
- 博主可以在后台完成所有内容和展示配置。
- 后台支持 Markdown 编辑、图片上传、视频上传、评论审核、SEO 配置和数据统计。
- 后端 API 使用 C# / ASP.NET Core 实现。
- 数据库使用 PostgreSQL。
- 前台 SEO 友好，首屏性能良好。
- 系统具备后续扩展能力，例如对象存储、CDN、全文搜索、多角色权限、邮件通知等。
