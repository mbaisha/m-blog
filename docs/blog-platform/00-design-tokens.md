# 00. 统一设计系统（Design Tokens）

> 本文档定义整个博客前台的设计令牌（Design Tokens），所有页面组件统一引用此规范。
> 覆盖：色彩、字体、间距、圆角、阴影、组件样式、分类品牌色、Dark 模式。

---

## 1. 色板总览

### 1.1 品牌色

| Token | Light | Dark | 用途 |
|-------|-------|------|------|
| `--color-primary` | `#6366F1` | `#818CF8` | 主色：链接、按钮、强调 |
| `--color-primary-light` | `#EEEDFE` | `rgba(99,102,241,0.12)` | 主色浅底：标签、选中态、徽章 |
| `--color-primary-dark` | `#4F46E5` | `#6366F1` | 主色深：Hover 状态 |

### 1.2 中性色

| Token | Light | Dark | 用途 |
|-------|-------|------|------|
| `--color-bg` | `#F8FAFC` | `#0F172A` | 页面背景 |
| `--color-surface` | `#FFFFFF` | `#1E293B` | 卡片/表面背景 |
| `--color-surface-secondary` | `#F8FAFC` | `#1E293B` | 表面二级（如搜索条背景） |
| `--color-text-primary` | `#1F2937` | `#F1F5F9` | 主要文字（标题） |
| `--color-text-secondary` | `#6B7280` | `#94A3B8` | 次要文字（正文/描述） |
| `--color-text-tertiary` | `#9CA3AF` | `#64748B` | 辅助文字（时间/阅读量） |
| `--color-text-placeholder` | `#9CA3AF` | `#64748B` | 输入框占位文字 |
| `--color-border` | `#E5E7EB` | `#334155` | 边框/分割线 |
| `--color-border-light` | `#F3F4F6` | `#1E293B` | 轻边框/骨架底色 |
| `--color-border-active` | `#CECBF6` | `#6366F1` | 激活态边框 |

### 1.3 语义色

| Token | Light | Dark | 用途 |
|-------|-------|------|------|
| `--color-accent` | `#F59E0B` | `#FBBF24` | 强调/警告 |
| `--color-success` | `#10B981` | `#34D399` | 成功/提示 |
| `--color-danger` | `#EF4444` | `#F87171` | 错误/删除 |
| `--color-info` | `#3B82F6` | `#60A5FA` | 信息 |

---

## 2. 分类品牌色

每个分类有独立的品牌色，用于卡片封面、分类徽章、标签、侧栏竖线等。

| 分类 | 浅色背景 `--cat-bg` | 深色文字 `--cat-text` | 深色边框 `--cat-border` |
|------|-------------------|---------------------|----------------------|
| 前端架构 | `#EEEDFE` | `#534AB7` | `#CECBF6` |
| 后端开发 | `#E1F5EE` | `#0F6E56` | `#9FE1CB` |
| 产品设计 | `#FAECE7` | `#993C1D` | `#F5C4B3` |
| 读书笔记 | `#FBEAF0` | `#993556` | `#F4C0D1` |
| AI 实践 | `#EAF3DE` | `#3B6D11` | `#C0DD97` |
| 工具效率 | `#FAEEDA` | `#854F0B` | `#FAC775` |

实现方式：后台配置分类时，每个分类可关联一个 `color` 字段，前台通过 CSS 变量映射。

---

## 3. 字体系统

| Token | 值 |
|-------|-----|
| `--font-sans` | `-apple-system, PingFang SC, Noto Sans SC, Inter, sans-serif` |
| `--font-serif` | `Georgia, Noto Serif SC, serif` |
| `--font-mono` | `JetBrains Mono, Fira Code, monospace` |

### 字号层级

| Token | 大小 | 字重 | 行高 | 用途 |
|-------|------|------|------|------|
| `--text-xs` | 10px / 0.625rem | 400 | 1.4 | 小标签、辅助信息 |
| `--text-sm` | 11px / 0.6875rem | 400 | 1.4 | 元信息、标签块文字 |
| `--text-base` | 13px / 0.8125rem | 400 | 1.5 | 正文小字、按钮文字 |
| `--text-md` | 14px / 0.875rem | 500 | 1.5 | 文章列表标题、导航 |
| `--text-lg` | 16px / 1rem | 500 | 1.5 | 页面标题、卡片标题 |
| `--text-xl` | 20px / 1.25rem | 500 | 1.4 | 大标题（Hero/页面） |
| `--text-2xl` | 24px / 1.5rem | 500 | 1.3 | 文章标题 |
| `--text-3xl` | 30px / 1.875rem | 500 | 1.2 | Hero 主标题 |

### 正文排版（文章详情/自定义页面）

| 属性 | 值 |
|------|-----|
| 字号 | 16px / 1rem |
| 行高 | 1.75 |
| 最大宽度 | 900px |
| 段间距 | 1.2em |
| H2 | 22px，600，上边距 2em |
| H3 | 18px，600，上边距 1.5em |
| 引用 | 左侧 4px `#6366F1` 竖线 |
| 代码块 | 14px JetBrains Mono，背景 `#1E293B` |

---

## 4. 间距与尺寸

| Token | 值 | 用途 |
|-------|-----|------|
| `--space-xs` | 4px | 极小间距 |
| `--space-sm` | 8px | 内边距、标签间距 |
| `--space-md` | 12px | 组件内间距 |
| `--space-base` | 16px | 卡片内边距、表单位置 |
| `--space-lg` | 20px | 网格间距、卡片间距 |
| `--space-xl` | 24px | 页面左右边距 |
| `--space-2xl` | 40px | 模块间距 |
| `--space-3xl` | 60px | 大模块间距 |

### 圆角

| Token | 值 | 用途 |
|-------|-----|------|
| `--radius-sm` | 4px | 标签、小徽章 |
| `--radius-md` | 8px | 按钮、输入框、小卡片 |
| `--radius-lg` | 12px | 卡片、大卡片 |
| `--radius-xl` | 16px | 大区块、Hero |
| `--radius-full` | 50% | 圆形头像、胶囊 |

### 阴影

| Token | 值 | 用途 |
|-------|-----|------|
| `--shadow-sm` | `0 1px 2px rgba(0,0,0,0.04)` | 轻阴影 |
| `--shadow-md` | `0 2px 6px rgba(0,0,0,0.04)` | 卡片默认 |
| `--shadow-lg` | `0 4px 12px rgba(0,0,0,0.06)` | 卡片 Hover、浮层 |

---

## 5. 组件设计令牌

### 5.1 导航栏

| 属性 | 值 |
|------|-----|
| 高度 | 44px |
| 背景 | `#F8FAFC`（surface-secondary） |
| 边框 | 0.5px `#E5E7EB` |
| 圆角 | 10px |
| 内边距 | 左右 16px |
| 菜单项间距 | 8px |
| 当前项 | 背景 `#EEEDFE`，文字 `#534AB7` |
| 其他项 | 文字 `#6B7280` |

### 5.2 卡片

| 属性 | 值 |
|------|-----|
| 背景 | `#FFFFFF` |
| 边框 | 0.5px `#E5E7EB` |
| 圆角 | 12px |
| 阴影 | `--shadow-md` |
| Hover | translateY(-2px) + `--shadow-lg`，200ms |
| 内边距 | 16px |
| 封面图 | 卡片总高度的 50%（圆角仅顶部）|

### 5.3 按钮

| 属性 | 值（主要） | 值（次要） |
|------|-----------|-----------|
| 背景 | `#1F2937` | `#FFFFFF` |
| 文字 | `#FFFFFF` | `#6B7280` |
| 边框 | 无 | 0.5px `#D3D1C7` |
| 圆角 | 8px | 8px |
| 内边距 | 12px 20px | 12px 20px |
| Hover | opacity 0.9 | 背景 `#F9FAFB` |

### 5.4 标签

| 属性 | 值 |
|------|-----|
| 背景 | `#F3F4F6` |
| 文字 | `#6B7280` |
| 圆角 | 3px（小标签） / 4px（大标签） |
| 内边距 | 4px 6px |
| 字号 | 7px（小） / 8px（中） / 9px（大） |
| Hover | 背景加深 |

### 5.5 分类徽章（卡片封面图上的）

| 属性 | 值 |
|------|-----|
| 背景 | `rgba(255,255,255,0.88)` |
| 文字 | 对应分类 `--cat-text` |
| 圆角 | 4px |
| 位置 | 卡片封面图左上角，距边缘 12px |
| 内边距 | 4px 8px |

### 5.6 分类竖线卡片（全部分类页）

| 属性 | 值 |
|------|-----|
| 竖线宽度 | 6px |
| 竖线圆角 | 3px |
| 竖线颜色 | 对应分类 `--cat-text` |
| 卡片高度 | 72px |
| 卡片内边距 | 16px |

### 5.7 阅读进度条

| 属性 | 值 |
|------|-----|
| 高度 | 3px |
| 填充色 | `#6366F1`（primary） |
| 背景 | `#F1EFE8` |
| 位置 | `position: fixed`，导航栏下方 |
| z-index | 低于导航栏 |

### 5.8 浮动 TOC

| 属性 | 值 |
|------|-----|
| 按钮大小 | 28×28px |
| 面板宽度 | 140px |
| 面板背景 | `rgba(255,255,255,0.95)` |
| 面板阴影 | `--shadow-lg` |
| 面板圆角 | 8px |
| 当前高亮 | 背景 `#EEEDFE`，文字 `#534AB7` |

---

## 6. 不一致项的修正

### 6.1 文字颜色统一

| 场景 | 旧值（不一致） | 统一后 |
|------|--------------|--------|
| 大标题 | `#2C2C2A` / `#1F2937` / `#374151` | **`#1F2937`** |
| 正文/描述 | `#5F5E5A` / `#6B7280` / `#4B5563` | **`#6B7280`** |
| 辅助文字 | `#888780` / `#9CA3AF` | **`#9CA3AF`** |
| 主色 | `#6366F1` / `#534AB7` | `#6366F1`（按钮/链接），`#534AB7`（深色文字场景）|

### 6.2 卡片边框统一

| 场景 | 旧值 | 统一后 |
|------|------|--------|
| 卡片边框 | `#E5E7EB` / `#D3D1C7` | **`#E5E7EB`**，0.5px |
| 导航栏边框 | `#E5E7EB` / 无 | **`#E5E7EB`**，0.5px |

### 6.3 圆角统一

| 场景 | 旧值 | 统一后 |
|------|------|--------|
| 卡片 | 10px / 12px / 14px | **12px** |
| 按钮 | 6px / 8px | **8px** |
| 搜索条 | 8px / 10px | **10px** |
| Hero 区块 | 16px / 18px / 20px | **18px** |
| 分页按钮 | 6px | **6px** |
| 标签 | 3px / 4px / 5px | **4px**（小标签3px） |

### 6.4 骨架屏统一

所有页面的骨架屏使用一致的样式：

```css
.skeleton {
  background: linear-gradient(90deg, #F3F4F6 25%, #E5E7EB 50%, #F3F4F6 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 12px;
}
```

---

## 7. Dark 模式色值映射

| CSS 变量 | Light | Dark |
|----------|-------|------|
| `--color-bg` | `#F8FAFC` | `#0F172A` |
| `--color-surface` | `#FFFFFF` | `#1E293B` |
| `--color-text-primary` | `#1F2937` | `#F1F5F9` |
| `--color-text-secondary` | `#6B7280` | `#94A3B8` |
| `--color-text-tertiary` | `#9CA3AF` | `#64748B` |
| `--color-border` | `#E5E7EB` | `#334155` |
| `--color-primary` | `#6366F1` | `#818CF8` |
| `--color-primary-light` | `#EEEDFE` | `rgba(99,102,241,0.12)` |

切换方式：在 `<html>` 上添加 `class="dark"`，所有组件引用 CSS 变量。

---

## 8. 二级导航菜单

### 8.1 数据结构

导航菜单支持两级，后端 API `GET /api/navigation` 返回：

```json
[
  { "id": 1, "label": "首页", "slug": "/", "children": [] },
  {
    "id": 2,
    "label": "文章",
    "slug": "/articles",
    "children": [
      { "id": 21, "label": "前端架构", "slug": "/categories/frontend", "children": [] },
      { "id": 22, "label": "后端开发", "slug": "/categories/backend", "children": [] },
      { "id": 23, "label": "读书笔记", "slug": "/categories/reading", "children": [] }
    ]
  },
  { "id": 3, "label": "项目", "slug": "/projects", "children": [] },
  { "id": 4, "label": "关于", "slug": "/about", "children": [] },
  { "id": 5, "label": "留言", "slug": "/message", "children": [] },
  { "id": 6, "label": "友链", "slug": "/friends", "children": [] }
]
```

### 8.2 桌面端交互

```
首页  文章 ▼  项目  关于  留言  友链
      ├─ 前端架构
      ├─ 后端开发
      └─ 读书笔记
```

- 有子菜单的项显示 `▼` 箭头
- Hover/点击展开下拉面板
- 面板宽度自适应，最小 140px
- 面板背景 `#FFFFFF`，边框 `#E5E7EB`，圆角 8px
- 子项 Hover 时背景 `#F3F4F6`

### 8.3 移动端交互

- 抽屉式导航中，有子菜单的项点击后展开子列表
- 子列表缩进 16px

---

## 9. CSS 变量声明

```css
:root {
  /* Brand */
  --color-primary: #6366F1;
  --color-primary-light: #EEEDFE;
  --color-primary-dark: #4F46E5;

  /* Neutral */
  --color-bg: #F8FAFC;
  --color-surface: #FFFFFF;
  --color-surface-secondary: #F8FAFC;
  --color-text-primary: #1F2937;
  --color-text-secondary: #6B7280;
  --color-text-tertiary: #9CA3AF;
  --color-border: #E5E7EB;
  --color-border-light: #F3F4F6;

  /* Typography */
  --font-sans: -apple-system, PingFang SC, Noto Sans SC, Inter, sans-serif;
  --font-serif: Georgia, Noto Serif SC, serif;
  --font-mono: JetBrains Mono, Fira Code, monospace;

  /* Spacing */
  --space-xs: 4px;
  --space-sm: 8px;
  --space-md: 12px;
  --space-base: 16px;
  --space-lg: 20px;
  --space-xl: 24px;
  --space-2xl: 40px;

  /* Radius */
  --radius-sm: 4px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --radius-xl: 16px;

  /* Shadow */
  --shadow-sm: 0 1px 2px rgba(0,0,0,0.04);
  --shadow-md: 0 2px 6px rgba(0,0,0,0.04);
  --shadow-lg: 0 4px 12px rgba(0,0,0,0.06);
}

.dark {
  --color-primary: #818CF8;
  --color-primary-light: rgba(99,102,241,0.12);
  --color-bg: #0F172A;
  --color-surface: #1E293B;
  --color-surface-secondary: #1E293B;
  --color-text-primary: #F1F5F9;
  --color-text-secondary: #94A3B8;
  --color-text-tertiary: #64748B;
  --color-border: #334155;
  --color-border-light: #1E293B;
}
```

---

## 10. 受影响文档

本文档发布后，以下设计文档中的色值、圆角、间距等应统一引用此规范：

- `14-detail-page-redesign.md` → 颜色变量替换为 CSS 变量引用
- `15-list-page-redesign.md` → 同上
- `16-homepage-redesign.md` → 同上
- `17-search-page.md` → 同上
- `18-custom-page.md` → 同上
- `19~26` → 直接使用此规范中的 Token

实际代码中只需在 CSS 中声明一次 `:root` 变量，所有组件引用变量即可。
