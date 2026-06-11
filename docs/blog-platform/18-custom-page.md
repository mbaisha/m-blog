# 18. 自定义页面设计规格

> 本文档是对 `05-frontend-spec.md` 中 `4.4 自定义页面` 的补充和细化。
> 设计定位：简洁内容展示页，仅保留页头 + 页面标题 + 正文 + 页脚。

---

## 1. 设计定位

自定义页面是**轻量内容页**，用于展示「关于我」、「留言板」、「隐私政策」等独立内容。

它与文章详情页的区别：

```
文章详情页：导航 → Hero → TOC → 正文 → 标签 → 上下篇 → 评论 → 页脚
自定义页面：导航 → 标题 → 正文 → 页脚
```

去掉所有"附加组件"，只保留核心阅读体验。

---

## 2. 页面布局

```
Zhu's Blog

┌─ 导航 ───────────────────────────────────────────────┐
│  首页  文章  项目  关于  留言  友链         🔍      │
└──────────────────────────────────────────────────────┘

关于我                                                    ← 页面标题
最后更新 2026-06-07                                       ← 更新时间

────────────────────────────────────────────────────

正文内容（Markdown 渲染）
最大宽度 900px，居中
与详情页正文样式一致

………
………
………

────────────────────────────────────────────────────
© 2026 Zhu's Blog
```

### 2.1 每区块规格

| 区块 | 包含 | 规格 |
|------|------|------|
| 导航栏 | 站点标题 + 导航菜单 + 搜索 | 复用首页导航组件 |
| 页面标题 | 标题文字 | 22px，font-weight: 500，`#1F2937` |
| 更新时间 | "最后更新 YYYY-MM-DD" | 12px，`#9CA3AF` |
| 分割线 | 一条细线 | 0.5px `#E5E7EB` |
| 正文 | Markdown 渲染内容 | 最大宽度 900px，居中 |
| 评论区 | 可选，后台配置 | 开启时复用详情页评论组件 |
| 页脚 | 版权 + 社交链接 | 复用首页页脚组件 |
| 回到顶部 | 浮动按钮 | 与首页/详情页一致 |

### 2.2 正文排版

完全复用文章详情页的 Markdown 渲染样式：

| 元素 | 规格 |
|------|------|
| 正文字号 | 16px / 1rem |
| 行高 | 1.75 |
| 最大宽度 | 900px |
| H2 标题 | 22px，font-weight: 600，上边距 2em |
| H3 标题 | 18px，font-weight: 600，上边距 1.5em |
| 代码块 | 深色背景 `#1E293B`，JetBrains Mono 14px |
| 图片 | 居中，圆角 12px，懒加载 |
| 引用 | 左侧 4px 主色竖线 |

### 2.3 评论区（可选）

通过后台 Layout 配置控制评论区开启/关闭：

```json
{
  "enableComment": true
}
```

开启时，评论区置于正文之后、页脚之前，复用 `CommentSection` 组件，反灌水规则与详情页一致。

---

## 3. 与文章详情页的组件对比

| 组件 | 文章详情页 | 自定义页面 |
|------|-----------|-----------|
| `Navbar` | ✅ | ✅ |
| `ReadingProgress` | ✅ | ❌ |
| `ArticleHero` | ✅ | ❌（替换为纯标题） |
| `FloatingToc` | ✅ | ❌ |
| `RightSidebar` | ✅ | ❌ |
| `ArticleTags` | ✅ | ❌ |
| `ArticleNav` | ✅ | ❌ |
| `RelatedArticles` | ✅ | ❌ |
| `CommentSection` | ✅ | ✅（可选）|
| `BackToTop` | ✅ | ✅ |
| `Footer` | ✅ | ✅ |

---

## 4. 路由

```text
GET /[customPageSlug]
```

- slug 由后台创建页面时指定，全局唯一
- 如果 slug 冲突（与已有路由匹配），自定义页面优先级低于固定路由

---

## 5. SEO

```html
<title>页面SEO标题 | 博客名</title>
<meta name="description" content="页面SEO描述" />
<link rel="canonical" href="https://example.com/[customPageSlug]" />

<meta property="og:title" content="页面SEO标题" />
<meta property="og:description" content="页面SEO描述" />
<meta property="og:type" content="website" />

<meta name="twitter:card" content="summary_large_image" />

<!-- 自动加入 Sitemap -->
```

---

## 6. 组件清单

| 组件名 | 职责 |
|--------|------|
| `CustomPage` | 自定义页面主容器 |
| `PageTitle` | 页面标题 + 更新时间 |
| `PageContent` | Markdown 正文渲染 |
| `CommentArea` | 评论区（可选，复用详情页组件）|

新建组件仅 `PageTitle`，其余均复用已有组件。
