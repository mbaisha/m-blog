# 04. C# Web API 接口规范

## 1. 文档目标

本文档用于指导 C# / ASP.NET Core Web API 开发。

后端 API 需要同时服务：

- Next.js 前台博客。
- Vue3 管理后台。
- 后续可能的移动端或其他客户端。

---

## 2. 基础规范

### 2.1 协议

```text
HTTP / HTTPS
```

生产环境必须使用 HTTPS。

---

### 2.2 数据格式

请求和响应统一使用 JSON。

```json
{
  "success": true,
  "data": {},
  "message": "ok"
}
```

---

### 2.3 路径规范

前台公开接口：

```text
/api/...
```

后台管理接口：

```text
/api/admin/...
```

认证接口：

```text
/api/auth/...
```

---

### 2.4 HTTP 方法

| 方法 | 用途 |
|---|---|
| GET | 查询 |
| POST | 创建 / 提交 / 执行动作 |
| PUT | 全量更新 |
| PATCH | 局部更新 |
| DELETE | 删除 |

---

### 2.5 状态码

| 状态码 | 说明 |
|---|---|
| 200 | 成功 |
| 201 | 创建成功 |
| 204 | 删除成功，无返回体 |
| 400 | 参数错误 |
| 401 | 未认证 |
| 403 | 无权限 |
| 404 | 资源不存在 |
| 409 | 资源冲突 |
| 422 | 业务校验失败 |
| 429 | 请求过于频繁 |
| 500 | 服务器错误 |

---

## 3. 统一响应结构

### 3.1 成功响应

```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "name": "example"
  },
  "message": "ok"
}
```

---

### 3.2 分页响应

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

---

### 3.3 错误响应

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_FAILED",
    "message": "参数校验失败",
    "details": [
      {
        "field": "title",
        "message": "标题不能为空"
      }
    ]
  }
}
```

---

## 4. 认证接口 Auth

## 4.1 管理员登录

```text
POST /api/auth/login
```

请求体：

```json
{
  "username": "admin",
  "password": "password123"
}
```

响应：

```json
{
  "success": true,
  "data": {
    "accessToken": "jwt-token",
    "refreshToken": "refresh-token",
    "expiresIn": 7200,
    "user": {
      "id": "uuid",
      "username": "admin",
      "role": "super_admin"
    }
  },
  "message": "登录成功"
}
```

失败情况：

- 用户名或密码错误。
- 账号被禁用。
- 登录失败次数过多。

---

## 4.2 刷新 Token

```text
POST /api/auth/refresh
```

请求体：

```json
{
  "refreshToken": "refresh-token"
}
```

响应：

```json
{
  "success": true,
  "data": {
    "accessToken": "new-jwt-token",
    "refreshToken": "new-refresh-token",
    "expiresIn": 7200
  },
  "message": "ok"
}
```

---

## 4.3 退出登录

```text
POST /api/auth/logout
```

请求头：

```text
Authorization: Bearer {accessToken}
```

请求体：

```json
{
  "refreshToken": "refresh-token"
}
```

响应：

```json
{
  "success": true,
  "data": null,
  "message": "已退出登录"
}
```

---

## 4.4 获取当前管理员

```text
GET /api/auth/me
```

请求头：

```text
Authorization: Bearer {accessToken}
```

响应：

```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "username": "admin",
    "email": "admin@example.com",
    "role": "super_admin",
    "avatar": null
  },
  "message": "ok"
}
```

---

## 5. 文章接口 Articles

## 5.1 前台获取文章列表

```text
GET /api/articles
```

查询参数：

| 参数 | 类型 | 必填 | 说明 |
|---|---|---|---|
| page | int | 否 | 页码，默认 1 |
| pageSize | int | 否 | 每页数量，默认 20 |
| categoryId | string | 否 | 分类 ID |
| categorySlug | string | 否 | 分类 slug |
| tagId | string | 否 | 标签 ID |
| tagSlug | string | 否 | 标签 slug |
| keyword | string | 否 | 搜索关键词 |

响应：

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "uuid",
        "title": "文章标题",
        "slug": "article-slug",
        "summary": "文章摘要",
        "coverImage": {
          "url": "https://example.com/cover.jpg",
          "thumbnailUrl": "https://example.com/thumb.jpg"
        },
        "category": {
          "name": "技术",
          "slug": "tech"
        },
        "tags": [
          {
            "name": "C#",
            "slug": "csharp"
          }
        ],
        "viewCount": 120,
        "commentCount": 3,
        "publishedAt": "2026-06-05T12:00:00Z"
      }
    ],
    "total": 100,
    "page": 1,
    "pageSize": 20,
    "totalPages": 5
  },
  "message": "ok"
}
```

---

## 5.2 前台获取文章详情

```text
GET /api/articles/{slug}
```

响应：

```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "title": "文章标题",
    "slug": "article-slug",
    "summary": "文章摘要",
    "content": "# Markdown 内容",
    "coverImage": {
      "url": "https://example.com/cover.jpg"
    },
    "category": {
      "name": "技术",
      "slug": "tech"
    },
    "tags": [],
    "viewCount": 120,
    "commentCount": 3,
    "seoTitle": "SEO 标题",
    "seoDescription": "SEO 描述",
    "publishedAt": "2026-06-05T12:00:00Z",
    "updatedAt": "2026-06-05T12:30:00Z"
  },
  "message": "ok"
}
```

---

## 5.3 后台获取文章列表

```text
GET /api/admin/articles
```

请求头：

```text
Authorization: Bearer {accessToken}
```

查询参数：

| 参数 | 类型 | 说明 |
|---|---|---|
| page | int | 页码 |
| pageSize | int | 每页数量 |
| status | string | draft / published / archived |
| keyword | string | 标题关键词 |
| categoryId | string | 分类 ID |

---

## 5.4 后台新建文章

```text
POST /api/admin/articles
```

请求体：

```json
{
  "title": "文章标题",
  "slug": "article-slug",
  "summary": "文章摘要",
  "content": "# Markdown 内容",
  "coverImageId": "uuid",
  "categoryId": "uuid",
  "tagIds": ["uuid1", "uuid2"],
  "seoTitle": "SEO 标题",
  "seoDescription": "SEO 描述",
  "seoKeywords": "关键词1,关键词2",
  "status": "draft",
  "isTop": false,
  "isRecommend": false
}
```

响应：

```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "slug": "article-slug"
  },
  "message": "文章创建成功"
}
```

---

## 5.5 后台更新文章

```text
PUT /api/admin/articles/{id}
```

请求体同新建文章。

---

## 5.6 后台删除文章

```text
DELETE /api/admin/articles/{id}
```

说明：

- 默认软删除。
- 需要管理员权限。
- 删除后记录审计日志。

---

## 5.7 后台发布文章

```text
POST /api/admin/articles/{id}/publish
```

请求体：

```json
{
  "publishedAt": "2026-06-05T12:00:00Z"
}
```

说明：

- 如果 `publishedAt` 晚于当前时间，可设置为定时发布。
- 第一阶段可先实现立即发布。

---

## 5.8 后台下架文章

```text
POST /api/admin/articles/{id}/unpublish
```

---

## 5.9 后台预览文章

```text
POST /api/admin/articles/{id}/preview
```

说明：

- 返回预览 HTML 或预览 URL。
- 不需要文章已发布。
- 需要管理员权限。

---

## 6. 分类接口 Categories

## 6.1 前台获取分类列表

```text
GET /api/categories
```

---

## 6.2 前台获取分类详情

```text
GET /api/categories/{slug}
```

查询参数：

| 参数 | 说明 |
|---|---|
| page | 页码 |
| pageSize | 每页数量 |

---

## 6.3 后台创建分类

```text
POST /api/admin/categories
```

请求体：

```json
{
  "name": "技术",
  "slug": "tech",
  "description": "技术文章分类",
  "coverImageId": "uuid",
  "sortOrder": 1,
  "seoTitle": "技术文章",
  "seoDescription": "技术文章分类描述"
}
```

---

## 6.4 后台更新分类

```text
PUT /api/admin/categories/{id}
```

---

## 6.5 后台删除分类

```text
DELETE /api/admin/categories/{id}
```

---

## 7. 标签接口 Tags

## 7.1 前台获取标签列表

```text
GET /api/tags
```

---

## 7.2 前台获取标签详情

```text
GET /api/tags/{slug}
```

---

## 7.3 后台创建标签

```text
POST /api/admin/tags
```

请求体：

```json
{
  "name": "C#",
  "slug": "csharp",
  "color": "#1677ff"
}
```

---

## 7.4 后台更新标签

```text
PUT /api/admin/tags/{id}
```

---

## 7.5 后台删除标签

```text
DELETE /api/admin/tags/{id}
```

---

## 8. 评论接口 Comments

## 8.1 前台获取文章评论

```text
GET /api/articles/{articleId}/comments
```

查询参数：

| 参数 | 说明 |
|---|---|
| page | 页码 |
| pageSize | 每页数量 |

响应：

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "uuid",
        "articleId": "uuid",
        "parentId": null,
        "nickname": "访客",
        "email": null,
        "website": null,
        "content": "写得不错",
        "avatar": null,
        "createdAt": "2026-06-05T12:00:00Z"
      }
    ],
    "total": 10,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  },
  "message": "ok"
}
```

说明：

- 只返回 `approved` 状态评论。
- 不返回 `ip_hash`。
- 不返回 `is_spam`。
- 不返回待审核评论。

---

## 8.2 前台提交评论

```text
POST /api/comments
```

请求体：

```json
{
  "articleId": "uuid",
  "parentId": null,
  "nickname": "访客",
  "email": "visitor@example.com",
  "website": null,
  "content": "这篇文章写得不错。",
  "captchaSessionId": "captcha-session-id",
  "captchaAnswer": "answer"
}
```

响应：

```json
{
  "success": true,
  "data": {
    "status": "pending",
    "message": "评论已提交，等待审核"
  },
  "message": "ok"
}
```

失败情况：

- 验证码错误。
- 评论为空。
- 评论过短。
- 评论过长。
- 频率限制。
- 文章不存在。
- 文章未发布。
- 包含敏感词。
- 包含过多链接。

---

## 8.3 后台获取评论列表

```text
GET /api/admin/comments
```

查询参数：

| 参数 | 说明 |
|---|---|
| page | 页码 |
| pageSize | 每页数量 |
| status | pending / approved / rejected / spam |
| articleId | 文章 ID |
| keyword | 搜索昵称或内容 |

---

## 8.4 后台审核通过

```text
POST /api/admin/comments/{id}/approve
```

---

## 8.5 后台拒绝评论

```text
POST /api/admin/comments/{id}/reject
```

请求体：

```json
{
  "reason": "包含广告链接"
}
```

---

## 8.6 后台删除评论

```text
DELETE /api/admin/comments/{id}
```

---

## 8.7 后台标记垃圾评论

```text
POST /api/admin/comments/{id}/spam
```

---

## 8.8 后台批量审核

```text
POST /api/admin/comments/bulk-approve
```

请求体：

```json
{
  "ids": ["uuid1", "uuid2"]
}
```

---

## 8.9 后台批量删除

```text
POST /api/admin/comments/bulk-delete
```

请求体：

```json
{
  "ids": ["uuid1", "uuid2"]
}
```

---

## 9. 验证码接口 Captcha

## 9.1 获取验证码图片

```text
GET /api/captcha/image
```

响应：

```json
{
  "success": true,
  "data": {
    "sessionId": "captcha-session-id",
    "imageBase64": "data:image/png;base64,..."
  },
  "message": "ok"
}
```

---

## 9.2 验证验证码

```text
POST /api/captcha/verify
```

请求体：

```json
{
  "sessionId": "captcha-session-id",
  "answer": "answer"
}
```

响应：

```json
{
  "success": true,
  "data": {
    "valid": true
  },
  "message": "ok"
}
```

---

## 10. 上传接口 Upload

## 10.1 上传图片

```text
POST /api/admin/upload/image
```

请求头：

```text
Authorization: Bearer {accessToken}
Content-Type: multipart/form-data
```

请求体：

```text
file: image file
```

响应：

```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "url": "https://example.com/uploads/image.jpg",
    "thumbnailUrl": "https://example.com/uploads/thumb.jpg",
    "filename": "safe-name.jpg",
    "mimeType": "image/jpeg",
    "sizeBytes": 123456
  },
  "message": "上传成功"
}
```

---

## 10.2 上传视频

```text
POST /api/admin/upload/video
```

响应：

```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "url": "https://example.com/uploads/video.mp4",
    "thumbnailUrl": "https://example.com/uploads/video-cover.jpg",
    "filename": "safe-name.mp4",
    "mimeType": "video/mp4",
    "sizeBytes": 123456789,
    "durationSeconds": 120.5
  },
  "message": "上传成功"
}
```

---

## 10.3 上传附件

```text
POST /api/admin/upload/file
```

---

## 10.4 后台获取媒体列表

```text
GET /api/admin/media
```

查询参数：

| 参数 | 说明 |
|---|---|
| page | 页码 |
| pageSize | 每页数量 |
| type | image / video / file |
| keyword | 文件名关键词 |

---

## 10.5 后台删除媒体

```text
DELETE /api/admin/media/{id}
```

---

## 11. 个人页面接口 Profile

## 11.1 前台获取个人页面配置

```text
GET /api/profile
```

响应：

```json
{
  "success": true,
  "data": {
    "sections": [
      {
        "sectionType": "hero",
        "title": "你好，我是 Zhu",
        "content": "一名开发者。",
        "metadata": {},
        "sortOrder": 1
      }
    ]
  },
  "message": "ok"
}
```

---

## 11.2 后台更新个人页面

```text
PUT /api/admin/profile
```

请求体：

```json
{
  "sections": [
    {
      "id": "uuid",
      "sectionType": "hero",
      "title": "你好，我是 Zhu",
      "content": "一名开发者。",
      "metadata": {},
      "sortOrder": 1,
      "isEnabled": true
    }
  ]
}
```

---

## 12. 项目展示接口 Projects

## 12.1 前台获取项目列表

```text
GET /api/projects
```

---

## 12.2 前台获取项目详情

```text
GET /api/projects/{slug}
```

---

## 12.3 后台创建项目

```text
POST /api/admin/projects
```

请求体：

```json
{
  "title": "个人博客系统",
  "slug": "personal-blog",
  "summary": "一个现代化个人博客系统。",
  "content": "# 项目介绍",
  "coverImageId": "uuid",
  "techStack": ["Next.js", "C#", "PostgreSQL"],
  "projectUrl": "https://example.com",
  "githubUrl": "https://github.com/example",
  "demoUrl": "https://demo.example.com",
  "sortOrder": 1,
  "isVisible": true
}
```

---

## 12.4 后台更新项目

```text
PUT /api/admin/projects/{id}
```

---

## 12.5 后台删除项目

```text
DELETE /api/admin/projects/{id}
```

---

## 13. 友情链接接口 Friends

## 13.1 前台获取友情链接

```text
GET /api/friends
```

---

## 13.2 后台创建友链

```text
POST /api/admin/friends
```

请求体：

```json
{
  "name": "示例网站",
  "url": "https://example.com",
  "description": "示例描述",
  "logoImageId": "uuid",
  "sortOrder": 1,
  "isVisible": true
}
```

---

## 13.3 后台更新友链

```text
PUT /api/admin/friends/{id}
```

---

## 13.4 后台删除友链

```text
DELETE /api/admin/friends/{id}
```

---

## 14. SEO 接口

## 14.1 前台获取 SEO 配置

```text
GET /api/seo?key=home
```

响应：

```json
{
  "success": true,
  "data": {
    "pageKey": "home",
    "title": "首页",
    "description": "个人博客首页",
    "keywords": "博客,个人品牌",
    "ogImage": {
      "url": "https://example.com/og.jpg"
    },
    "canonicalUrl": "https://example.com"
  },
  "message": "ok"
}
```

---

## 14.2 后台获取 SEO 配置

```text
GET /api/admin/seo
```

---

## 14.3 后台更新 SEO 配置

```text
PUT /api/admin/seo
```

请求体：

```json
{
  "pageKey": "home",
  "title": "首页",
  "description": "个人博客首页",
  "keywords": "博客,个人品牌",
  "ogImageId": "uuid",
  "canonicalUrl": "https://example.com"
}
```

---

## 14.4 生成 Sitemap

```text
POST /api/admin/seo/sitemap/generate
```

---

## 14.5 获取 Sitemap

```text
GET /api/sitemap.xml
```

---

## 14.6 获取 Robots

```text
GET /api/robots.txt
```

---

## 15. 统计接口 Statistics

## 15.1 后台仪表盘统计

```text
GET /api/admin/statistics/dashboard
```

响应：

```json
{
  "success": true,
  "data": {
    "articleCount": 100,
    "draftCount": 5,
    "pendingCommentCount": 12,
    "todayVisitCount": 320,
    "totalVisitCount": 10000,
    "mediaCount": 200,
    "storageBytes": 10485760
  },
  "message": "ok"
}
```

---

## 15.2 热门文章

```text
GET /api/admin/statistics/popular-articles
```

---

## 15.3 访问趋势

```text
GET /api/admin/statistics/visits
```

查询参数：

| 参数 | 说明 |
|---|---|
| start | 开始日期 |
| end | 结束日期 |
| groupBy | day / week / month |

---

## 16. 自定义页面接口 Pages

## 16.1 前台获取已发布页面

```text
GET /api/pages/{slug}
```

响应：

```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "title": "关于本站",
    "slug": "about-site",
    "content": "Markdown 内容",
    "summary": "页面摘要",
    "coverImageUrl": "https://...",
    "seoTitle": "页面 SEO 标题",
    "seoDescription": "页面 SEO 描述",
    "viewCount": 230,
    "publishedAt": "2026-06-05T12:00:00Z",
    "updatedAt": "2026-06-05T12:30:00Z"
  },
  "message": "ok"
}
```

---

## 16.2 前台获取已发布页面列表

```text
GET /api/pages
```

响应：

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "uuid",
        "title": "关于本站",
        "slug": "about-site",
        "summary": "页面摘要",
        "viewCount": 230,
        "publishedAt": "2026-06-05T12:00:00Z"
      }
    ],
    "total": 5
  },
  "message": "ok"
}
```

---

## 16.3 后台获取页面列表

```text
GET /api/admin/pages
```

查询参数：

| 参数 | 说明 |
|---|---|
| page | 页码 |
| pageSize | 每页数量 |
| status | draft / published / archived |

---

## 16.4 后台新建页面

```text
POST /api/admin/pages
```

请求体：

```json
{
  "title": "关于本站",
  "slug": "about-site",
  "content": "# Markdown 内容",
  "summary": "页面摘要",
  "coverImageId": "uuid",
  "seoTitle": "页面 SEO 标题",
  "seoDescription": "页面 SEO 描述",
  "seoKeywords": "关键词1,关键词2",
  "template": "default",
  "status": "published"
}
```

---

## 16.5 后台更新页面

```text
PUT /api/admin/pages/{id}
```

---

## 16.6 后台删除页面

```text
DELETE /api/admin/pages/{id}
```

---

## 16.7 后台发布 / 下架页面

```text
POST /api/admin/pages/{id}/publish
POST /api/admin/pages/{id}/unpublish
```

---

## 17. 导航栏接口 Navigation

## 17.1 前台获取导航菜单

```text
GET /api/navigation
```

响应：

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "uuid",
        "label": "首页",
        "url": "/",
        "target": "_self",
        "icon": null,
        "children": []
      },
      {
        "id": "uuid",
        "label": "博客",
        "url": "/articles",
        "target": "_self",
        "icon": null,
        "children": [
          {
            "id": "uuid",
            "label": "技术",
            "url": "/categories/tech",
            "target": "_self"
          }
        ]
      }
    ]
  },
  "message": "ok"
}
```

---

## 17.2 后台获取导航菜单列表（全量，含未显示）

```text
GET /api/admin/navigation
```

---

## 17.3 后台创建菜单项

```text
POST /api/admin/navigation
```

请求体：

```json
{
  "label": "关于我",
  "url": "/about",
  "target": "_self",
  "icon": null,
  "parentId": null,
  "sortOrder": 5,
  "isVisible": true
}
```

---

## 17.4 后台更新菜单项

```text
PUT /api/admin/navigation/{id}
```

---

## 17.5 后台删除菜单项

```text
DELETE /api/admin/navigation/{id}
```

---

## 17.6 后台批量排序

```text
PUT /api/admin/navigation/reorder
```

请求体：

```json
{
  "items": [
    { "id": "uuid", "sortOrder": 1, "parentId": null },
    { "id": "uuid", "sortOrder": 2, "parentId": null }
  ]
}
```

---

## 18. 页脚接口 Footer

## 18.1 前台获取页脚配置

```text
GET /api/footer
```

响应：

```json
{
  "success": true,
  "data": {
    "sections": [
      {
        "sectionKey": "copyright",
        "title": "版权说明",
        "content": "© 2026 Zhu. All rights reserved."
      },
      {
        "sectionKey": "icp",
        "title": "ICP 备案",
        "content": "京ICP备XXXXXXXX号"
      },
      {
        "sectionKey": "public_security",
        "title": "公安网备案",
        "content": "京公网安备 XXXXXXXXXX号"
      },
      {
        "sectionKey": "social",
        "title": "社交链接",
        "content": "[{\"platform\":\"GitHub\",\"url\":\"https://github.com/xxx\"}]"
      },
      {
        "sectionKey": "contact",
        "title": "联系方式",
        "content": "email@example.com"
      },
      {
        "sectionKey": "sitemap",
        "title": "网站地图",
        "content": "/sitemap.xml"
      },
      {
        "sectionKey": "friends",
        "title": "友情链接",
        "content": "[{\"name\":\"示例\",\"url\":\"https://example.com\"}]"
      }
    ]
  },
  "message": "ok"
}
```

---

## 18.2 后台获取页脚配置

```text
GET /api/admin/footer
```

---

## 18.3 后台更新页脚区块

```text
PUT /api/admin/footer/section/{sectionKey}
```

请求体：

```json
{
  "title": "版权说明",
  "content": "© 2026 Zhu. All rights reserved.",
  "sortOrder": 1,
  "isEnabled": true
}
```

---

## 18.4 后台新增自定义区块

```text
POST /api/admin/footer/section
```

---

## 19. 主题接口 Theme

## 19.1 前台获取主题配置

```text
GET /api/theme
```

响应：

```json
{
  "success": true,
  "data": {
    "primaryColor": "#6366f1",
    "siteName": "Zhu 的个人博客",
    "siteDescription": "分享技术与生活",
    "siteLogo": "https://...logo.png"
  },
  "message": "ok"
}
```

---

## 19.2 后台更新主题色

```text
PUT /api/admin/theme/primary-color
```

请求体：

```json
{
  "primaryColor": "#6366f1"
}
```

---

## 19.3 后台更新站点信息

```text
PUT /api/admin/theme/site-info
```

请求体：

```json
{
  "siteName": "Zhu 的个人博客",
  "siteDescription": "分享技术与生活",
  "siteLogoId": "uuid"
}
```

---

## 20. 模块布局接口 Layout

## 20.1 前台获取页面模块布局

```text
GET /api/layout?pageType=home
```

响应：

```json
{
  "success": true,
  "data": {
    "modules": [
      { "moduleKey": "hero", "title": "首屏区域", "sortOrder": 1, "isEnabled": true, "dataSource": {}, "displayOptions": {} },
      { "moduleKey": "featured_articles", "title": "精选文章", "sortOrder": 2, "isEnabled": true, "dataSource": { "count": 4 }, "displayOptions": { "style": "grid" } },
      { "moduleKey": "latest_articles", "title": "最新文章", "sortOrder": 3, "isEnabled": true, "dataSource": { "count": 6 }, "displayOptions": { "style": "list" } }
    ]
  },
  "message": "ok"
}
```

---

## 20.2 后台获取模块布局配置

```text
GET /api/admin/layout?pageType=home
```

---

## 20.3 后台更新模块布局

```text
PUT /api/admin/layout
```

请求体：

```json
{
  "pageType": "home",
  "modules": [
    { "moduleKey": "hero", "sortOrder": 1, "isEnabled": true, "dataSource": {}, "displayOptions": {} },
    { "moduleKey": "featured_articles", "sortOrder": 2, "isEnabled": true, "dataSource": { "count": 4 }, "displayOptions": { "style": "grid" } }
  ]
}
```

---

## 20.4 后台批量添加默认模块

```text
POST /api/admin/layout/defaults?pageType=home
```

---

## 21. 访问记录接口 Visits

## 21.1 记录访问（前台上报）

```text
POST /api/visits/track
```

请求体：

```text
{
  "pageType": "article",
  "pageId": "uuid",
  "pagePath": "/articles/hello-world",
  "referer": "https://google.com"
}
```

说明：

- 不要求认证。
- 服务端根据 IP 和 User-Agent 自动去重（同一 IP + 同一页面 + 5 分钟内不重复计数）。
- 同时更新对应业务表的 view_count。

---

## 21.2 阅读数获取

```text
GET /api/visits/count?pageType=article&pageId=uuid
```

响应：

```json
{
  "success": true,
  "data": {
    "viewCount": 1230
  },
  "message": "ok"
}
```

---

## 21.3 后台获取访问记录列表

```text
GET /api/admin/visits
```

查询参数：

| 参数 | 说明 |
|---|---|
| page | 页码 |
| pageSize | 每页数量 |
| start | 开始日期 |
| end | 结束日期 |
| pageType | article / page / category / tag / project |
| keyword | 路径关键词 |

响应：

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "uuid",
        "pagePath": "/articles/hello-world",
        "pageType": "article",
        "ipHash": "abc123",
        "userAgent": "Mozilla/5.0 ...",
        "referer": "https://google.com",
        "visitedAt": "2026-06-05T12:00:00Z"
      }
    ],
    "total": 50000,
    "page": 1,
    "pageSize": 20,
    "totalPages": 2500
  },
  "message": "ok"
}
```

---

## 21.4 后台获取阅读数统计

```text
GET /api/admin/visits/summary
```

查询参数：

| 参数 | 说明 |
|---|---|
| start | 开始日期 |
| end | 结束日期 |
| groupBy | day / week / month |

---

## 21.5 后台清理访问记录

```text
POST /api/admin/visits/cleanup
```

请求体：

```json
{
  "retentionDays": 30
}
```

说明：

- 将删除超过 `retentionDays` 天的原始访问记录。
- 阅读数聚合数据不受影响。
- 可在 `site_settings` 中配置自动清理策略和保留天数，后台定时任务自动执行。

---

## 22. 站点配置接口 Settings

## 22.1 后台获取站点配置

```text
GET /api/admin/settings
```

---

## 22.2 后台更新站点配置

```text
PUT /api/admin/settings
```

请求体：

```json
{
  "primaryColor": "#6366f1",
  "visitorRetentionDays": 30,
  "siteName": "Zhu 的个人博客",
  "siteDescription": "分享技术与生活",
  "siteLogoId": "uuid",
  "scheduledCleanupEnabled": true
}
```

---

## 23. 权限规则

### 23.1 公开接口

无需登录：

```text
GET /api/articles
GET /api/articles/{slug}
GET /api/categories
GET /api/tags
GET /api/comments
GET /api/profile
GET /api/projects
GET /api/friends
GET /api/seo
GET /api/sitemap.xml
GET /api/robots.txt
POST /api/auth/login
POST /api/auth/refresh
POST /api/comments
GET /api/captcha/image
POST /api/captcha/verify
GET /api/pages
GET /api/pages/{slug}
GET /api/navigation
GET /api/footer
GET /api/theme
GET /api/layout?pageType=...
POST /api/visits/track
GET /api/visits/count?pageType=...&pageId=...
```

---

### 23.2 管理接口

必须登录：

```text
/api/admin/*
```

---

### 23.3 角色权限

| 权限 | super_admin | admin | content_admin | comment_moderator |
|---|---|---|---|---|
| 文章管理 | 是 | 是 | 是 | 否 |
| 分类标签 | 是 | 是 | 是 | 否 |
| 评论审核 | 是 | 是 | 否 | 是 |
| 自定义页面 | 是 | 是 | 否 | 否 |
| 导航管理 | 是 | 是 | 否 | 否 |
| 页脚管理 | 是 | 是 | 否 | 否 |
| 布局配置 | 是 | 是 | 否 | 否 |
| 主题配置 | 是 | 是 | 否 | 否 |
| 媒体上传 | 是 | 是 | 是 | 否 |
| 访问记录 | 是 | 是 | 否 | 否 |
| SEO 配置 | 是 | 是 | 否 | 否 |
| 用户管理 | 是 | 否 | 否 | 否 |
| 系统设置 | 是 | 否 | 否 | 否 |

---

## 24. 参数校验规则

### 17.1 文章

| 字段 | 规则 |
|---|---|
| title | 必填，1-255 字符 |
| slug | 必填，唯一，小写字母数字和短横线 |
| summary | 0-500 字符 |
| content | 必填，Markdown 文本 |
| status | draft / published / archived |
| tagIds | 数组，最多 10 个 |
| seoTitle | 0-80 字符 |
| seoDescription | 0-200 字符 |

---

### 17.2 评论

| 字段 | 规则 |
|---|---|
| articleId | 必填，文章必须存在且已发布 |
| nickname | 必填，1-64 字符 |
| email | 可选，合法邮箱 |
| website | 可选，合法 URL |
| content | 必填，5-1000 字符 |
| captchaSessionId | 必填 |
| captchaAnswer | 必填 |

---

### 17.3 上传

| 类型 | 允许格式 | 单文件限制 |
|---|---|---|
| 图片 | jpg / jpeg / png / webp / gif | 10MB |
| 视频 | mp4 / mov / webm | 500MB |
| 附件 | pdf / doc / docx / zip | 50MB |

---

## 18. 错误码规范

| 错误码 | HTTP 状态 | 说明 |
|---|---|---|
| UNAUTHORIZED | 401 | 未认证 |
| FORBIDDEN | 403 | 无权限 |
| NOT_FOUND | 404 | 资源不存在 |
| VALIDATION_FAILED | 400/422 | 参数校验失败 |
| CAPTCHA_FAILED | 422 | 验证码错误 |
| RATE_LIMITED | 429 | 请求过于频繁 |
| FILE_TOO_LARGE | 413 | 文件过大 |
| FILE_TYPE_NOT_ALLOWED | 415 | 文件类型不允许 |
| RESOURCE_CONFLICT | 409 | 资源冲突 |
| SERVER_ERROR | 500 | 服务器错误 |

---

## 19. C# 后端目录建议

```text
BlogPlatform.Api/
├── Controllers/
│   ├── AuthController.cs
│   ├── ArticlesController.cs
│   ├── CategoriesController.cs
│   ├── TagsController.cs
│   ├── CommentsController.cs
│   ├── CaptchaController.cs
│   ├── UploadController.cs
│   ├── MediaController.cs
│   ├── ProfileController.cs
│   ├── ProjectsController.cs
│   ├── FriendsController.cs
│   ├── SeoController.cs
│   └── StatisticsController.cs
├── Filters/
├── Middleware/
└── Program.cs

BlogPlatform.Application/
├── Features/
├── DTOs/
├── Validators/
├── Interfaces/
└── Services/

BlogPlatform.Domain/
├── Entities/
├── Enums/
└── Events/

BlogPlatform.Infrastructure/
├── Auth/
├── Storage/
├── Captcha/
├── RateLimit/
└── Email/

BlogPlatform.Persistence/
├── DbContext/
├── Configurations/
└── Migrations/
```

---

## 20. API 验收标准

- 所有接口响应结构统一。
- 所有后台接口必须鉴权。
- 所有写接口必须参数校验。
- 评论提交接口必须验证码校验。
- 评论提交接口必须频率限制。
- 上传接口必须文件类型和大小校验。
- 文章接口支持分页。
- 文章详情接口支持 slug 查询。
- 评论接口只返回已审核评论。
- SEO 接口可返回 sitemap 和 robots。
- 所有异常统一处理。
- 所有敏感操作写入审计日志。
