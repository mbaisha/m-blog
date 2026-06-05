# 10. 性能与 SEO 设计

## 1. 目标

保证前台加载快、搜索引擎友好、后台响应稳定。

---

## 2. 性能目标

| 指标 | 目标 |
|---|---|
| 首页首屏加载 | < 2 秒 |
| 文章详情页首屏加载 | < 1.5 秒 |
| 常规 API 响应 | < 300ms |
| 图片资源 | 懒加载 |
| 视频资源 | 懒加载 |
| 列表数据 | 分页 |
| 热点数据 | 缓存 |

---

## 3. 前台性能

### 3.1 图片

- 使用 `next/image`。
- 开启懒加载。
- 生成缩略图。
- 避免首屏加载大图片。

### 3.2 视频

- 视频懒加载。
- 仅在进入视口后加载。
- 使用 `preload="metadata"`。

### 3.3 列表

- 分页加载。
- 不一次性加载全部文章。
- 骨架屏优化体验。

### 3.4 路由

- 高概率页面预取。
- 低概率页面按需加载。

---

## 4. 后端性能

### 4.1 查询优化

- 分页查询。
- 高频字段索引。
- 避免 N+1 查询。
- 使用投影 DTO。

### 4.2 缓存

建议缓存：

- 首页推荐。
- 文章详情。
- 分类列表。
- 标签列表。
- 个人页面配置。
- SEO 配置。
- 友情链接。
- 热门文章。

不建议缓存：

- 登录接口。
- 评论提交接口。
- 后台管理接口。
- 权限接口。

---

## 5. SEO 目标

- 搜索引擎可抓取页面。
- 每个页面有独立 Meta。
- 自动生成 Sitemap。
- 自动生成 Robots。
- 支持 SSR / SSG。
- URL 结构清晰。
- 图片有 alt。
- 支持 Open Graph。

---

## 6. SEO 页面规范

### 6.1 首页

- title
- description
- keywords
- canonical
- og:title
- og:description
- og:image

### 6.2 文章页

- title
- description
- keywords
- canonical
- article:published_time
- article:modified_time
- article:author
- article:section
- article:tag
- og:image

### 6.3 分类页

- title
- description
- canonical
- og:title
- og:description

### 6.4 标签页

- title
- description
- canonical
- og:title
- og:description

---

## 7. Sitemap

生成内容：

- 首页。
- 所有已发布文章。
- 所有已发布自定义页面。
- 所有分类。
- 所有标签。
- 关于我。
- 项目详情。
- 友情链接。

---

## 8. SEO 自定义页面规范

### 8.1 自定义页面 SEO

- title（页面 SEO 标题）。
- description（页面 SEO 描述）。
- keywords（页面 SEO 关键词）。
- canonical（页面正式 URL）。
- og:title / og:description / og:image。

### 8.2 阅读数对 SEO 的影响

- 阅读数展示在页面中不影响首屏渲染（异步或服务端携带）。
- Sitemap 中的 `<lastmod>` 字段使用页面 `updated_at`。
- 自定义页面使用独立 URL，不与其他页面冲突。

---

## 9. Robots

允许：

```text
User-agent: *
Allow: /
Sitemap: https://example.com/sitemap.xml
```

---

## 9. 结构化数据

文章页可支持 JSON-LD：

```json
{
  "@context": "https://schema.org",
  "@type": "BlogPosting",
  "headline": "文章标题",
  "description": "文章摘要",
  "image": "封面图 URL",
  "author": {
    "@type": "Person",
    "name": "作者名"
  },
  "datePublished": "2026-06-05T12:00:00Z",
  "dateModified": "2026-06-05T12:30:00Z"
}
```

---

## 10. 验收标准

- 首页首屏小于 2 秒。
- 文章详情页首屏小于 1.5 秒。
- 所有页面有 SEO Meta。
- Sitemap 可访问。
- Robots 可访问。
- 图片有 alt。
- 文章页有 Open Graph。
- 后台接口响应稳定。
- 热点数据有缓存。
