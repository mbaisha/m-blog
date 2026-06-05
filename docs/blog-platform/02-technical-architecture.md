# 02. 技术架构设计

## 1. 架构目标

本项目的技术架构目标是：

- 前台 SEO 友好。
- 前台视觉体验优秀。
- 后台功能完整。
- 后端 API 稳定高性能。
- 数据库使用 PostgreSQL。
- 前后端职责清晰。
- 系统可部署、可维护、可扩展。

---

## 2. 总体架构

```text
访客浏览器
  ↓
Next.js 前台博客
  ↓ HTTPS / HTTP
C# ASP.NET Core Web API
  ↓
Entity Framework Core
  ↓
PostgreSQL
  ↓
本地文件存储 / 对象存储
```

后台管理员：

```text
管理员浏览器
  ↓
Vue 3 + Element Plus 管理后台
  ↓ HTTPS / HTTP
C# ASP.NET Core Web API
  ↓
Entity Framework Core
  ↓
PostgreSQL
  ↓
本地文件存储 / 对象存储
```

---

## 3. 技术栈

### 3.1 前台博客

| 项目 | 技术 |
|---|---|
| 框架 | Next.js |
| 语言 | TypeScript |
| 样式 | Tailwind CSS |
| 渲染 | SSR / SSG |
| Markdown | react-markdown / remark / rehype |
| 代码高亮 | shiki / highlight.js |
| 请求 | fetch / axios |
| 状态管理 | React Query / SWR，可选 |
| 表单校验 | zod / react-hook-form |
| SEO | next metadata API + sitemap |

---

### 3.2 后台管理

| 项目 | 技术 |
|---|---|
| 框架 | Vue 3 |
| 语言 | TypeScript |
| UI | Element Plus |
| 构建 | Vite |
| 路由 | Vue Router |
| 请求 | axios |
| 状态管理 | Pinia |
| Markdown 编辑器 | vditor / markdown-it / md-editor-v3 |
| 表单校验 | element-plus 表单规则 + 自定义校验 |

---

### 3.3 后端 API

| 项目 | 技术 |
|---|---|
| 框架 | ASP.NET Core Web API |
| 语言 | C# |
| ORM | Entity Framework Core |
| 数据库 | PostgreSQL |
| 认证 | JWT + Refresh Token |
| 参数校验 | FluentValidation |
| 日志 | Serilog |
| 文件上传 | IFormFile |
| 限流 | ASP.NET Core Rate Limiting |
| 缓存 | IMemoryCache / Redis，后续可选 |
| 映射 | AutoMapper，可选 |

---

### 3.4 数据库

| 项目 | 技术 |
|---|---|
| 数据库 | PostgreSQL |
| 主键 | UUID |
| 时间 | timestamptz |
| 扩展字段 | jsonb |
| 搜索 | tsvector，后续增强 |
| 迁移 | EF Core Migrations |

---

## 4. 模块划分

### 4.1 前台博客模块

| 模块 | 说明 |
|---|---|
| Home | 首页，支持模块化布局，可后台自定义 |
| Articles | 文章列表、文章详情，列表页支持模块配置 |
| Custom Pages | 自定义页面，支持后台创建和发布 |
| Categories | 分类页 |
| Tags | 标签页 |
| Archive | 归档页 |
| Search | 搜索页 |
| About | 关于我 |
| Projects | 项目展示 |
| Friends | 友情链接 |
| Message | 留言板 |
| Navigation | 顶部导航栏，支持后台管理菜单项 |
| Footer | 页脚，支持后台管理各区块内容 |
| Visitor | 实时访问统计、页面阅读数展示 |
| Components | 公共组件 |
| SEO | Meta、Open Graph、JSON-LD |
| Theme | 主题色、布局变体、样式配置 |
| Layout | 响应式布局引擎，适配桌面 / 平板 / 手机 |

---

### 4.2 后台管理模块

| 模块 | 说明 |
|---|---|
| Auth | 登录、退出、权限 |
| Dashboard | 仪表盘 |
| Articles | 文章管理、Markdown 编辑 |
| Categories | 分类管理 |
| Tags | 标签管理 |
| Comments | 评论审核 |
| Media | 媒体库 |
| Profile | 个人页面配置 |
| Projects | 项目展示管理 |
| Friends | 友情链接管理 |
| SEO | SEO 配置 |
| Pages | 自定义页面管理（创建 / 编辑 / 发布 / 删除） |
| Navigation | 导航栏管理（菜单项排序、可见、新窗口） |
| Footer | 页脚管理（版权、备案、社交、区块配置） |
| Layout | 首页与内页模块布局管理 |
| Theme | 主题色配置、视觉风格设置 |
| Visits | 访问者记录与阅读统计管理 |
| Statistics | 数据统计 |
| Settings | 系统设置 |

---

### 4.3 后端 API 模块

| 模块 | 说明 |
|---|---|
| Auth | 登录、刷新 Token、权限 |
| Articles | 文章查询、管理 |
| Categories | 分类查询、管理 |
| Tags | 标签查询、管理 |
| Comments | 评论提交、审核、限流 |
| Upload | 图片、视频、附件上传 |
| Media | 媒体文件管理 |
| Profile | 个人页面配置 |
| Projects | 项目展示 |
| Friends | 友情链接 |
| SEO | SEO 配置、Sitemap、Robots |
| Statistics | 访问统计、数据统计 |
| Pages | 自定义页面 CRUD，发布与预览 |
| Navigation | 导航菜单管理 |
| Footer | 页脚区块管理 |
| Layout | 首页与内页模块布局配置 |
| Theme | 主题色与视觉配置 |
| Visits | 访问者记录管理、阅读数统计、自动清理 |
| Audit | 审计日志 |
| RateLimit | 频率限制 |
| Captcha | 验证码 |

---

## 5. 核心数据流

### 5.1 文章访问数据流

```text
用户访问 /articles/slug
 ↓
Next.js 服务端请求 /api/articles/{slug}
 ↓
ASP.NET Core 查询 PostgreSQL
 ↓
返回文章详情 JSON
 ↓
Next.js 渲染 HTML
 ↓
浏览器展示文章
 ↓
同时触发访问统计 API
```

---

### 5.2 文章发布数据流

```text
管理员编辑 Markdown
 ↓
Vue 后台调用 POST /api/admin/articles
 ↓
ASP.NET Core 校验 Token 和参数
 ↓
EF Core 写入 PostgreSQL
 ↓
返回文章 ID / slug
 ↓
后台展示保存成功
 ↓
前台文章可访问
```

---

### 5.3 评论提交数据流

```text
访客提交评论
 ↓
前端获取验证码 token
 ↓
调用 POST /api/comments
 ↓
后端校验验证码
 ↓
后端执行 IP 限流
 ↓
后端执行内容过滤
 ↓
后端写入 comments 表
 ↓
返回提交结果
 ↓
后台审核后可见
```

---

### 5.4 媒体上传数据流

```text
管理员选择文件
 ↓
前端校验类型和大小
 ↓
调用 POST /api/admin/upload/image
 ↓
后端校验 MIME 和扩展名
 ↓
生成安全文件名
 ↓
保存到存储目录
 ↓
写入 media 表
 ↓
返回 URL 和媒体 ID
```

---

## 6. API 设计规范

### 6.1 基础规范

- 使用 RESTful API。
- 使用 JSON 作为请求和响应格式。
- 使用 HTTPS，生产环境必须开启。
- 后台接口使用 JWT 认证。
- 前台公开接口无需登录。
- 管理接口必须以 `/api/admin` 开头。
- 响应结构统一。

---

### 6.2 统一响应结构

成功响应：

```json
{
  "success": true,
  "data": {},
  "message": "ok"
}
```

分页响应：

```json
{
  "success": true,
  "data": {
    "items": [],
    "total": 100,
    "page": 1,
    "pageSize": 20,
    "totalPages": 5
  },
  "message": "ok"
}
```

错误响应：

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_FAILED",
    "message": "参数校验失败",
    "details": []
  }
}
```

---

### 6.3 常见错误码

| 错误码 | 说明 |
|---|---|
| UNAUTHORIZED | 未登录或 Token 无效 |
| FORBIDDEN | 无权限 |
| NOT_FOUND | 资源不存在 |
| VALIDATION_FAILED | 参数校验失败 |
| CAPTCHA_FAILED | 验证码错误 |
| RATE_LIMITED | 请求过于频繁 |
| FILE_TOO_LARGE | 文件过大 |
| FILE_TYPE_NOT_ALLOWED | 文件类型不允许 |
| SERVER_ERROR | 服务器内部错误 |

---

## 7. 安全架构

### 7.1 认证

- 后台登录使用账号密码。
- 登录成功后返回 access token。
- access token 短期有效。
- refresh token 长期有效。
- refresh token 存入数据库。
- 退出登录时使 refresh token 失效。

---

### 7.2 权限

第一阶段：

- 超级管理员。

后续扩展：

- 内容管理员。
- 评论审核员。
- 访客用户。

权限控制方式：

- 后端基于角色校验。
- 前端基于权限隐藏菜单。
- 后端校验是最终安全边界。

---

### 7.3 反灌水

评论、留言、联系表单统一接入：

- 验证码。
- 频率限制。
- 内容长度限制。
- 敏感词过滤。
- 链接限制。
- 重复内容检测。
- 审核机制。

---

### 7.4 文件安全

- 限制文件类型。
- 限制文件大小。
- 校验 MIME。
- 重新生成文件名。
- 禁止执行上传目录。
- 图片、视频、附件分目录存储。
- 不允许用户控制保存路径。

---

## 8. 缓存架构

### 8.1 前台缓存

- 静态资源缓存。
- 图片懒加载。
- 视频懒加载。
- API 请求缓存。
- 路由预取。

---

### 8.2 后端缓存

建议缓存：

- 首页推荐数据。
- 文章详情。
- 分类列表。
- 标签列表。
- 个人页面配置。
- SEO 配置。
- 友情链接。
- 热门文章。

不建议缓存：

- 后台管理接口。
- 评论提交接口。
- 登录接口。
- 权限接口。

---

## 9. 部署架构

### 9.1 第一阶段部署

```text
Nginx
 ├── 前台 Next.js 静态/SSR 服务
 ├── 后台 Vue 静态服务
 └── 后端 ASP.NET Core API

PostgreSQL 容器
本地文件存储目录
```

---

### 9.2 后续升级部署

```text
CDN
 ├── 图片
 ├── 视频
 └── 静态资源

对象存储
 ├── 图片
 ├── 视频
 └── 附件

应用服务器
 ├── Next.js
 ├── Vue Admin
 └── ASP.NET Core API

PostgreSQL 主库
Redis，可选
日志服务，可选
监控服务，可选
```

---

## 10. 目录结构建议

### 10.1 仓库结构

```text
personal-blog-platform/
├── apps/
│   ├── web/                    # Next.js 前台
│   ├── admin/                  # Vue3 后台
│   └── api/                    # C# ASP.NET Core API
├── docs/
│   └── blog-platform/          # 当前文档目录
├── docker/
│   ├── postgres/
│   └── nginx/
├── docker-compose.yml
├── README.md
└── .env.example
```

---

### 10.2 前端目录结构

```text
apps/web/
├── app/
│   ├── (home)/
│   ├── articles/
│   ├── categories/
│   ├── tags/
│   ├── archive/
│   ├── search/
│   ├── about/
│   ├── projects/
│   ├── friends/
│   └── message/
├── components/
├── lib/
├── styles/
└── public/
```

---

### 10.3 后台目录结构

```text
apps/admin/
├── src/
│   ├── api/
│   ├── components/
│   ├── layouts/
│   ├── router/
│   ├── stores/
│   ├── views/
│   ├── utils/
│   └── styles/
├── public/
└── vite.config.ts
```

---

### 10.4 后端目录结构

```text
apps/api/
├── BlogPlatform.Api/
├── BlogPlatform.Application/
├── BlogPlatform.Domain/
├── BlogPlatform.Infrastructure/
├── BlogPlatform.Persistence/
└── BlogPlatform.Tests/
```

---

## 11. 环境配置

### 11.1 后端环境变量

```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=blog_platform;Username=blog_user;Password=blog_password
Jwt__Key=replace-with-long-random-secret
Jwt__Issuer=blog-platform
Jwt__Audience=blog-platform-client
Jwt__AccessTokenExpirationMinutes=120
Jwt__RefreshTokenExpirationDays=30
Storage__RootPath=./uploads
Cors__AllowedOrigins=http://localhost:3000,http://localhost:5173
```

---

### 11.2 前端环境变量

```env
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000
```

---

### 11.3 后台环境变量

```env
VITE_API_BASE_URL=http://localhost:5000
```

---

## 12. 技术约束

### 12.1 前端约束

- 前台必须支持 SSR 或 SSG。
- 前台不得只使用纯客户端渲染。
- 所有图片必须有 alt。
- 所有接口请求需要统一封装。
- 所有页面需要错误状态和加载状态。
- 所有表单需要校验。

---

### 12.2 后端约束

- 所有后台接口必须鉴权。
- 所有写操作必须参数校验。
- 所有异常必须统一处理。
- 所有敏感操作必须记录日志。
- 所有数据库访问必须通过 EF Core。
- 禁止 SQL 字符串拼接。
- 文件上传必须校验 MIME 和扩展名。

---

### 12.3 数据库约束

- 主键使用 UUID。
- 时间字段使用 `timestamptz`。
- 配置类字段使用 `jsonb`。
- 对外 URL 使用 `slug`。
- 高频查询字段建立索引。
- 删除策略优先软删除。

---

## 13. 架构验收标准

- 前台可以独立构建。
- 后台可以独立构建。
- 后端可以独立构建。
- 前后端通过 API 通信。
- 数据库迁移可以自动执行。
- 后台接口有权限保护。
- 前台页面有 SEO Meta。
- 评论接口有验证和限流。
- 上传接口有文件安全校验。
- 系统可以通过 Docker Compose 启动。
