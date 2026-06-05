# 05. Next.js 前台前端规格

## 1. 目标

前台博客端面向访客，核心目标是：

- 高颜值。
- SEO 友好。
- 阅读体验优秀。
- 首屏性能良好。
- 响应式适配移动端。
- 支持评论提交。
- 支持文章、分类、标签、归档、搜索、关于我、项目展示、友情链接、留言板。

---

## 2. 技术栈

```text
Next.js
TypeScript
Tailwind CSS
react-markdown
remark-gfm
rehype-highlight 或 shiki
next-seo 或 Next.js metadata API
SWR 或 React Query，可选
zod，可选
```

---

## 3. 路由设计

```text
/
/[customPageSlug]            # 自定义页面
/articles
/articles/[slug]
/categories
/categories/[slug]
/tags
/tags/[slug]
/archive
/search
/about
/projects
/projects/[slug]
/friends
/message
/sitemap.xml                  # 自动生成
/robots.txt                   # 自动生成
```

---

## 4. 页面规格

## 4.1 首页 `/`

### 模块（后台可自定义顺序、显示/隐藏和数据源）

- 顶部导航（后台可管理菜单项）。
- Hero 区域（可后台替换大图、副标题、按钮文本）。
- 精选文章（可后台设置数量）。
- 最新文章（可后台设置数量和布局样式：网格/列表）。
- 项目展示入口。
- 分类网格。
- 标签云。
- 个人简介模块。
- 友情链接模块。
- 页脚（后台可管理各区块内容）。

### 首页模块化渲染

首页应实现模块引擎，按 `home_layout_modules` 配置的顺序渲染每个模块组件。前端组件命名与 `module_key` 对应：

```text
home/modules/
├── HeroModule
├── FeaturedArticlesModule
├── LatestArticlesModule
├── CategoryGridModule
├── TagCloudModule
├── ProjectShowcaseModule
├── AboutBriefModule
├── FriendLinksModule
├── CustomContentModule
```

### SEO

```ts
export const metadata = {
  title: '首页 - 个人博客',
  description: '个人博客与个人品牌展示平台。',
  openGraph: {
    title: '首页 - 个人博客',
    description: '个人博客与个人品牌展示平台。',
    images: ['/og/default.jpg']
  }
};
```

---

## 4.2 文章列表 `/articles`

### 功能

- 文章卡片列表（支持网格/列表两种布局，可通过后台 Layout 配置切换）。
- 分页。
- 分类筛选。
- 标签筛选。
- 搜索入口。
- 每篇文章展示阅读量（view_count）。

### 组件

- `ArticleCard`
- `Pagination`
- `FilterBar`
- `EmptyState`
- `LoadingSkeleton`

---

## 4.3 文章详情 `/articles/[slug]`

### 功能

- Markdown 渲染。
- 代码高亮。
- 自动生成目录。
- 阅读进度。
- 上一篇 / 下一篇。
- 相关文章。
- 分享。
- 评论区。
- 文章详情页同时触发 `POST /api/visits/track` 上报访问。
- 文章详情页展示阅读数量（view_count）。

### SEO

- title 使用文章 SEO 标题。
- description 使用文章 SEO 描述。
- keywords 使用文章 SEO 关键词。
- canonical 使用文章正式 URL。
- Open Graph 使用文章封面图。

---

## 4.4 自定义页面 `/[slug]`

### 功能

- 后台创建的自定义页面通过唯一 slug 访问。
- 支持 Markdown 内容渲染。
- 支持自定义封面图和摘要。
- 支持独立 SEO 信息。
- 展示阅读数量。
- 支持评论区（通过 Layout 模块控制是否开启评论）。

### 路由

```text
GET /[customPageSlug]
```

### SEO

- title 使用页面 SEO 标题。
- description 使用页面 SEO 描述。
- canonical 使用页面正式 URL。
- 自动加入 Sitemap。

### 组件

- `PageContent`
- `PageCover`
- `CommentArea`

---

## 4.5 分类页 `/categories/[slug]`

### 功能

- 分类标题。
- 分类描述。
- 分类文章列表。
- 分页。

---

## 4.6 标签页 `/tags/[slug]`

### 功能

- 标签标题。
- 标签文章列表。
- 分页。

---

## 4.7 归档页 `/archive`

### 功能

- 按年份和月份分组。
- 时间轴展示。
- 快速跳转。

---

## 4.8 搜索页 `/search`

### 功能

- 搜索输入框。
- 搜索结果列表。
- 分页。
- 空结果提示。

---

## 4.8 关于我 `/about`

### 功能

- 个人头像。
- 个人简介。
- 技能标签。
- 工作经历。
- 教育经历。
- 联系方式。
- 社交链接。

---

## 4.9 项目展示 `/projects`

### 功能

- 项目列表。
- 项目卡片。
- 技术栈标签。
- 项目详情入口。

---

## 4.10 项目详情 `/projects/[slug]`

### 功能

- 项目封面。
- 项目介绍。
- 技术栈。
- 项目链接。
- GitHub 链接。
- Demo 链接。
- 项目图片。
- 项目视频。

---

## 4.11 友情链接 `/friends`

### 功能

- 友链 Logo。
- 友链名称。
- 友链描述。
- 点击跳转。

---

## 4.12 留言板 `/message`

### 功能

- 留言表单。
- 验证码。
- 提交状态。
- 提交成功提示。
- 提交失败提示。

---

## 5. 组件规范

### 5.1 通用组件

```text
Button
Input
Textarea
Select
Badge
Tag
Avatar
Card
Skeleton
EmptyState
Pagination
Modal
Toast
Loading
```

### 5.2 博客组件

```text
ArticleCard
ArticleList
ArticleContent
ArticleToc
CommentList
CommentForm
ShareBar
SearchBox
ArchiveTimeline
ProjectCard
FriendLinkCard
```

---

## 6. Markdown 渲染规范

### 6.1 允许内容

- 标题。
- 段落。
- 列表。
- 表格。
- 代码块。
- 引用。
- 图片。
- 链接。
- 任务列表。

### 6.2 安全要求

- 禁止执行危险 HTML。
- 链接需要校验协议。
- 图片需要懒加载。
- 代码块需要语法高亮。
- 表格需要响应式处理。

---

## 7. SEO 规范

### 7.1 每个页面必须有

- title
- description
- canonical
- openGraph title
- openGraph description
- openGraph image

### 7.2 文章页额外需要

- article:published_time
- article:modified_time
- article:author
- article:section
- article:tag

---

## 8. 性能规范

- 图片使用 `next/image`。
- 图片开启懒加载。
- 视频使用懒加载。
- 首屏避免加载大体积脚本。
- 列表分页加载。
- 静态资源使用缓存。
- 路由预取仅用于高概率访问页面。

---

## 9. 响应式设计

### 桌面端（≥ 1024px）

- 最大内容宽度：1200px。
- 左右边距：24px。
- 文章正文最佳阅读宽度：680px - 780px（居中对齐或左侧对齐）。
- 三端断点：`sm: 640px`、`md: 768px`、`lg: 1024px`、`xl: 1280px`。
- 首页多模块可并行展示（如 Hero + 精选 + 最新并存于首屏区域）。
- 目录固定在右侧（文章详情页）。
- 导航横向展示，带二级下拉菜单。

### 平板端（768px - 1023px）

- 最大内容宽度：96%。
- 左右边距：20px。
- 列表卡片改为双列网格。
- 目录折叠为可展开侧栏或顶部标签。
- 导航横向展示但限制可见项数，多余项放入 "更多" 下拉。
- 触控区域不小于 44px × 44px。

### 手机端（< 768px）

- 最大内容宽度：100%。
- 左右边距：16px。
- 导航折叠为汉堡菜单，点击展开抽屉式导航。
- 所有列表改为单列。
- 首页模块简化显示（如 Hero 只显示标题和一张图，不显示多个模块并列）。
- 文章正文单列，目录折叠到顶部或底部。
- 评论区单列。
- 页脚垂直排列。
- 底部固定或悬浮式操作栏（如回到顶部、搜索入口）。

---

## 10. 视觉与主题设计

### 10.1 整体视觉风格

- 2026 年审美趋势：干净、克制、轻科技感、高可读性。
- 建议设计元素：
  - 柔和渐变背景（例如浅蓝到浅紫或浅咖到浅灰）。
  - 玻璃拟态卡片（`backdrop-filter: blur()` + 半透明背景）。
  - 细腻阴影（`box-shadow` 多层或 `drop-shadow`）。
  - 圆角模块（`border-radius: 12px - 20px`）。
  - 科技感几何线条或网格装饰（微妙的 SVG 背景图案）。
  - 高对比文字层级（正文 #1f2937，二级 #4b5563，辅助 #9ca3af）。
  - 浅色模式为主，可配合深色模式。

### 10.2 主色调自定义

- 后台配置主色值，前台通过 CSS 变量应用：

```css
:root {
  --color-primary: #6366f1;          /* 后台配置 */
  --color-primary-light: #a5b4fc;
  --color-primary-dark: #4f46e5;
  --color-bg: #f8fafc;
  --color-surface: #ffffff;
  --color-text: #1f2937;
  --color-text-secondary: #4b5563;
  --color-border: #e5e7eb;
  --color-accent: #f59e0b;
}
```

- 导航栏、按钮、链接、选中态、标签、滚动条等统一应用 `--color-primary`。
- 主色变化不应影响可读性和可访问性（保证对比度）。

### 10.3 字体

- 中文：系统字体（`-apple-system`, `PingFang SC`, `Noto Sans SC` 等）。
- 英文/代码：`Inter`、`JetBrains Mono`。
- 字号层级：标题 2rem-2.5rem，副标题 1.25rem-1.5rem，正文 1rem-1.125rem，辅助 0.875rem。

---

## 11. 导航与页脚组件规范

### 11.1 导航栏

- 组件 `Navbar` 从 `GET /api/navigation` 获取菜单树。
- 桌面端：水平排版，支持二级下拉菜单。
- 移动端：汉堡菜单展开为全屏或侧边抽屉。
- 当前页面高亮。
- 管理员可见编辑入口（可选）。

### 11.2 页脚

- 组件 `Footer` 从 `GET /api/footer` 获取所有区块。
- 必需区块：
  - 版权说明。
  - ICP 备案号（跳转至工信部链接）。
  - 公安网备案号（跳转至公安备案链接）。
  - 友情链接区块（如有）。
  - 社交链接图标。
  - 联系方式。
  - 站点地图链接。
  - RSS 订阅链接（如有）。
  - 隐私政策 / 免责声明（如后台配置）。
- 区块支持后台启/停和自定义顺序。

---

## 12. 访问统计

### 12.1 阅读数展示

- 文章详情页、自定义页面、分类页等展示阅读数量。
- 数据来源：业务表 `view_count` 字段。

### 12.2 前台访问上报

- 页面加载后异步调用 `POST /api/visits/track`。
- 使用 `navigator.sendBeacon()` 或在 Next.js 服务端中间件中上报。
- 不阻塞页面渲染，不影响 SEO。
- 服务端做去重（同 IP + 同页面 + 5 分钟内不重复计数）。

---

## 13. 前台验收标准

- 首页可正常访问。
- 所有页面有 SEO Meta。
- 文章详情渲染正确。
- 图片懒加载正常。
- 移动端适配正常。
- 搜索可用。
- 评论可提交。
- 评论提交有验证反馈。
- 404 页面存在。
- sitemap.xml 可访问。
