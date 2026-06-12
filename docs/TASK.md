# 个人博客平台 - 实施计划与任务清单

> 项目文档索引：[docs/blog-platform/](blog-platform/)

项目名称：m-blog
- 严格按文件执行：按照文件中的任务顺序逐步开发，完成一个标记一个 [x]，开发中的标记 [~]
- 每次开发完成做记录：在"开发记录"表中写入日期、阶段、完成内容
- 新需求处理：提出新需求后，先添加到"新增需求记录"表中，与我确认优先级后再并入任务清单
- 代码中必须写好注释，包括函数、类、接口等，注释要中文，参数要英文，异常返回值要英文

---

## 状态图例

- [ ] 待开发
- [x] 已完成
- [~] 开发中

---

## Phase 0：项目初始化（已完成）

**目标：** 建立仓库结构，初始化三端项目，配置基础环境。

- [x] 0.1 创建项目根目录结构（monorepo 布局：`/backend`、`/frontend`、`/admin`、`/docs`）
- [x] 0.2 初始化 ASP.NET Core Web API 项目（`/backend`，目标框架 .NET 10）
- [x] 0.3 初始化 Next.js 前台项目（`/frontend`，TypeScript + Tailwind CSS）
- [x] 0.4 初始化 Vue 3 后台管理项目（`/admin`，TypeScript + Element Plus + Vite）
- [x] 0.5 配置 PostgreSQL（Docker Compose）
- [x] 0.6 配置各端环境变量模板（`.env.example`）
- [x] 0.7 配置 `.gitignore`
- [x] 0.8 初始化 Git 仓库并提交

---

## Phase 1：后端基础（已完成）

**目标：** 搭建 ASP.NET Core API 骨架，连接数据库，实现认证。

- [x] 1.1 配置 EF Core + PostgreSQL 连接（支持环境变量 DB_HOST/DB_PORT/DB_NAME/DB_USER/DB_PASSWORD）
- [x] 1.2 创建所有数据库实体
- [x] 1.3 创建数据库迁移（EF Core Migrations）
- [x] 1.4 实现统一响应结构（ApiResponse）
- [x] 1.5 实现全局异常处理中间件
- [x] 1.6 配置 Serilog 结构化日志
- [x] 1.7 实现 JWT 登录（/api/auth/login）
- [x] 1.8 实现 Refresh Token（/api/auth/refresh + /api/auth/logout）
- [x] 1.9 实现 JWT 中间件与鉴权
- [x] 1.10 实现 FluentValidation 全局校验
- [x] 1.11 实现基础 CORS 配置
- [x] 1.12 实现健康检查接口（/api/health）

---

## Phase 2：后台管理基础（已完成）

**目标：** 搭建 Vue3 管理后台框架，实现登录和基础布局。

- [x] 2.1 配置 Vue Router 路由
- [x] 2.2 封装 Axios 请求客户端（拦截器、Token 管理）
- [x] 2.3 实现 Pinia 状态管理（auth store）
- [x] 2.4 实现登录页（/login）
- [x] 2.5 实现路由守卫（未登录重定向）
- [x] 2.6 实现后台基础布局（侧边栏菜单 + 顶部栏 + 内容区）
- [x] 2.7 实现仪表盘页面（文章总数、评论、访问量等统计卡片）
- [x] 2.8 实现菜单按权限展示

---

## Phase 3：内容管理（已完成）

**目标：** 实现文章、分类、标签的完整管理功能。

- [x] 3.1 实现分类 API（CRUD + 排序）
- [x] 3.2 实现标签 API（CRUD + 排序）
- [x] 3.3 实现文章 API（CRUD + 草稿/发布/下架/置顶/推荐）
- [x] 3.4 实现图片上传 API（/api/admin/upload/image）
- [x] 3.5 实现分类管理页面
- [x] 3.6 实现标签管理页面
- [x] 3.7 实现文章列表页面（搜索、筛选、分页）
- [x] 3.8 实现新建/编辑文章页面（Markdown 编辑器 + 封面图 + 分类/标签 + SEO）

---

## Phase 4：前台博客基础（已完成）

**目标：** 实现面向访客的基础前台博客系统。

- [x] 4.1 实现前台文章公开 API（列表 + 详情 + 相关文章）
- [x] 4.2 实现前台分类 API
- [x] 4.3 实现前台标签 API
- [x] 4.4 实现前台归档 API
- [x] 4.5 实现前台搜索 API
- [x] 4.6 封装统一 API 请求客户端
- [x] 4.7 实现基础首页（Hero + 文章 + 分类 + 标签 + 项目入口）
- [x] 4.8 实现文章列表页（卡片网格）
- [x] 4.9 实现文章详情页（Markdown 渲染 + 阅读数）
- [x] 4.10 实现分类页
- [x] 4.11 实现标签页
- [x] 4.12 实现归档页
- [x] 4.13 实现搜索页
- [x] 4.14 实现响应式三端适配（桌面 ≥1024px / 平板 768-1023px / 手机 <768px）
- [x] 4.15 实现 SEO Meta（每个页面独立 metadata）
- [x] 4.16 实现导航栏组件
- [x] 4.17 实现页脚组件

---

## Phase 5：评论与反灌水（已完成）

**目标：** 实现访客评论功能，具备反灌水能力。

- [x] 5.1 实现图形验证码生成与校验 API
- [x] 5.2 实现评论提交 API（验证码校验、IP 频率限制、内容校验）
- [x] 5.3 实现评论审核 API（通过/拒绝/标记垃圾）
- [x] 5.4 实现评论列表 API（按文章、按状态）
- [x] 5.5 实现 ASP.NET Core Rate Limiting 配置
- [x] 5.6 实现后台评论管理页面（按状态筛选、批量操作）
- [x] 5.7 实现前台评论提交组件（验证码 + 表单）
- [x] 5.8 实现前台评论列表展示组件

---

## Phase 6：统一设计系统与全局重构（高优先级）

**目标：** 基于新的设计规范（docs 00-design-tokens, 00-responsive），建立统一的设计系统，重构全局样式。

**参考文档：** [00-design-tokens.md](blog-platform/00-design-tokens.md)、[00-responsive.md](blog-platform/00-responsive.md)

### 子任务

#### 6.1 设计系统 —— CSS 自定义属性

- [x] 6.1.1 创建全局 CSS 变量文件（--color-primary、--color-bg、--text-* 等）
- [x] 6.1.2 实现 Light / Dark 双主题切换逻辑
- [x] 6.1.3 实现分类品牌色映射（前端架构/后端开发/产品设计/读书笔记等）
- [x] 6.1.4 统一全站字体系统（--font-sans、--font-mono、字号层级）
- [x] 6.1.5 统一全站间距/圆角/阴影 Token
- [x] 6.1.6 应用设计 Token 到现有组件（导航、卡片、按钮、标签等）

#### 6.2 导航栏重构（符合响应式规范）

- [x] 6.2.1 顶部标题行（网站标题 + 搜索悬停展开交互）
- [x] 6.2.2 导航菜单从后台 API 获取（支持二级下拉）
- [x] 6.2.3 桌面端全宽显示 + 平板端"更多"下拉折叠
- [x] 6.2.4 移动端汉堡菜单 + 抽屉式导航（二级缩进）
- [x] 6.2.5 Dark Mode 切换按钮
- [x] 6.2.6 搜索悬停展开/点击展开交互

#### 6.3 页脚重构

- [x] 6.3.1 版权信息
- [x] 6.3.2 社交链接（GitHub、邮箱、RSS 等）
- [x] 6.3.3 响应式适配（桌面多列 / 手机单列）

#### 6.4 响应式全局适配

- [x] 6.4.1 统一断点：桌面 ≥1024px / 平板 768-1023px / 手机 <768px
- [x] 6.4.2 各页面内容容器 max-width: 1200px
- [x] 6.4.3 移动端底部悬浮操作栏（搜索/目录/回顶部）
- [x] 6.4.4 骨架屏加载状态统一实现

---

## Phase 7：前台页面全面升级（高优先级）

**目标：** 按照新设计规范（docs 14-26）全面升级所有前台页面的视觉和交互。

### 子任务

#### 7.1 首页全新设计（doc 16）

- [x] 7.1.1 Hero 区块（个人品牌文案 + 统计方块）
- [x] 7.1.2 热门标签模块
- [x] 7.1.3 精选文章（3 列卡片，封面图 + 分类徽章）
- [x] 7.1.4 最新文章（列表形式，分类徽章 + 标签块 + 阅读量 + 阅读时长）
- [x] 7.1.5 项目展示（2 列卡片网格）
- [x] 7.1.6 邮件订阅模块（可选）
- [x] 7.1.7 回到顶部按钮（滚动后出现）
- [x] 7.1.8 滚动渐入动画（prefers-reduced-motion 尊重）
- [x] 7.1.9 后台可配置首页模块布局（LayoutManage.vue + ModuleLayout 后端 API）

#### 7.2 文章列表页全新设计（doc 15）

- [x] 7.2.1 极简 Hero（标题 + 描述 + 统计）
- [x] 7.2.2 搜索条（粘性定位，搜索框 + 分类下拉 + 排序下拉 + 搜索按钮）
- [x] 7.2.3 可折叠右侧边栏（分类树 + 标签云，展开 150px / 收起 28px）
- [x] 7.2.4 卡片 / 列表双视图切换
- [x] 7.2.5 分类品牌色应用（卡片左侧竖线/顶部色条）
- [x] 7.2.6 标签徽章 + 阅读时长显示
- [x] 7.2.7 当前筛选条件指示行
- [x] 7.2.8 筛选条件 URL 参数持久化

#### 7.3 文章详情页全新设计（doc 14）

- [x] 7.3.1 阅读进度条（3px 细条，顶部全宽）
- [x] 7.3.2 文章 Hero（分类标签 + 标题 + 元信息行）
- [x] 7.3.3 正文区（最大宽度 900px，行高 1.75）
- [x] 7.3.4 右侧栏（220px，分享/收藏/复制按钮 + 分类导航）
- [x] 7.3.5 浮动 TOC 按钮（左侧 28×28px，点击展开 140px 面板）
- [x] 7.3.6 导读卡片 / 提示框（Tip / Warning / Info）
- [x] 7.3.7 标签区（彩色标签）
- [x] 7.3.8 上一篇 / 下一篇导航
- [x] 7.3.9 相关文章（3 列小卡片）
- [x] 7.3.10 评论区集成

#### 7.4 搜索页（doc 17，基于列表页扩展）

- [x] 7.4.1 复用列表页完整布局和组件
- [x] 7.4.2 搜索关键词高亮（`<mark>` 标签）
- [x] 7.4.3 搜索条预填关键词
- [x] 7.4.4 搜索结果统计行（共 X 篇 · 用时 Y 秒）
- [x] 7.4.5 空状态处理

#### 7.5 归档页（doc 19）

- [x] 7.5.1 时间线布局（年份 → 月份 → 文章列表）
- [x] 7.5.2 年份标题 + 文章数
- [x] 7.5.3 月份节点（主色圆点）+ 灰色竖线
- [x] 7.5.4 文章行（标题 + 日期）可点击跳转

#### 7.6 分类页（doc 24）

- [x] 7.6.1 2 列卡片网格
- [x] 7.6.2 左侧 6px 品牌色竖线
- [x] 7.6.3 分类名 + 描述 + 文章数

#### 7.7 标签云页（doc 25）

- [x] 7.7.1 彩色标签云容器
- [x] 7.7.2 按文章数决定标签大小（热门标签更大）
- [x] 7.7.3 随机品牌色分配（hover 加深）

#### 7.8 自定义页面（doc 18）

- [x] 7.8.1 轻量内容页（导航 → 标题 → 正文 → 页脚）
- [x] 7.8.2 "最后更新 YYYY-MM-DD" 时间戳
- [x] 7.8.3 可选评论区（后台配置控制，Page.EnableComments 字段 + 前台联系入口）
- [x] 7.8.4 动态 slug 路由 `/[slug]`

#### 7.9 项目列表页（doc 20）

- [x] 7.9.1 2 列卡片网格（290×200px）
- [x] 7.9.2 封面图区域（品牌色铺底）
- [x] 7.9.3 技术栈标签 + 查看详情/GitHub 链接

#### 7.10 项目详情页（doc 21）

- [x] 7.10.1 Hero 区块（品牌色浅块 + 图标 + 描述 + 按钮）
- [x] 7.10.2 截图区（可在 Markdown 内容中自由嵌入）
- [x] 7.10.3 项目介绍（Markdown 渲染）
- [x] 7.10.4 核心功能列表（可在 Markdown 内容中自由嵌入）

#### 7.11 留言板（doc 22）

- [x] 7.11.1 留言表单（昵称必填 + 邮箱选填 + 内容 + 验证码）
- [x] 7.11.2 留言列表（头像首字母 + 用户名 + 时间 + 内容）
- [x] 7.11.3 "加载更多"分页
- [x] 7.11.4 反灌水规则（与评论区一致）

#### 7.12 友情链接页（doc 23）

- [x] 7.12.1 2 列卡片网格（290×56px）
- [x] 7.12.2 圆形头像（首字母 + 品牌色背景）
- [x] 7.12.3 站点名 + 描述 + 访问链接

#### 7.13 404 页面（doc 26）

- [x] 7.13.1 大号 404 文字（淡紫色，不刺眼）
- [x] 7.13.2 友好错误提示
- [x] 7.13.3 建议卡片（最新文章 + 热门标签 + 全部分类）
- [x] 7.13.4 返回首页 + 查看文章链接

---

## Phase 8：个人品牌模块（中优先级）

**目标：** 实现关于我、媒体库完整的后台管理和前台展示。

**参考文档：** [06-admin-spec.md](blog-platform/06-admin-spec.md#49-个人页面配置-profile)

### 子任务

#### 后端 API

- [x] 8.1 实现个人页面配置 API（CRUD，头像/昵称/简介/技能/经历/社交链接/模块排序）
- [x] 8.2 实现视频/附件上传 API
- [x] 8.3 实现媒体文件管理 API（列表/删除/搜索/分组）

#### 后台管理

- [x] 8.4 实现个人页面配置页面（头像/昵称/简介/技能标签/工作经历/教育经历/社交链接）
- [x] 8.5 实现媒体库页面（上传/预览/删除/路径复制/插入文章）

#### 前台

- [x] 8.6 实现关于我页面（按模块渲染：Hero/介绍/技能/经历等，后台可配置顺序和显隐）

---

## Phase 9：自定义页面与站点配置（中优先级）

**目标：** 实现自定义页面管理、导航/页脚管理、主题配置、模块布局配置。

**参考文档：** [06-admin-spec.md](blog-platform/06-admin-spec.md#413-自定义页面管理-pages)

### 子任务

#### 后端 API

- [x] 9.1 实现自定义页面 API（CRUD + 发布/下架/预览）
- [x] 9.2 实现导航菜单 API（CRUD + 排序 + 树形 + 可见性）
- [x] 9.3 实现页脚配置 API（版权/备案/社交/联系方式/自定义区块）
- [x] 9.4 实现主题色配置 API
- [x] 9.5 实现模块布局配置 API（各页面模块顺序/显隐/数据源）
- [x] 9.6 实现站点设置 API（站点名/描述/Logo/Favicon/评论审核开关/访问记录保留天数）

#### 后台管理

- [x] 9.7 实现自定义页面管理页面（创建/编辑/发布/删除/预览/列表筛选）
- [x] 9.8 实现导航菜单管理页面（树形展示、拖拽排序、无限层级）
- [x] 9.9 实现页脚管理页面（区块编辑/排序/启用关闭/自定义区块新增删除）
- [x] 9.10 实现主题配置页面（主色选择器、站点名/Logo/Favicon 配置）
- [x] 9.11 实现首页模块布局配置页面（各模块启用/排序/数据源配置）
- [x] 9.12 实现系统设置页面（站点基本信息 + 评论审核开关 + 访问记录保留天数）

#### 前台

- [x] 9.13 实现自定义页面动态路由渲染（`/[slug]`）
- [x] 9.14 实现模块引擎（ModuleRenderer 根据 module_key 匹配组件）
- [x] 9.15 实现后台配置的导航栏动态渲染
- [x] 9.16 实现后台配置的页脚动态渲染
- [x] 9.17 实现主题色自动应用（导航、按钮、链接等）

---

## Phase 10：访问统计与访客记录（中优先级）

**目标：** 实现访问上报、阅读数统计与访客记录管理。

**参考文档：** [06-admin-spec.md](blog-platform/06-admin-spec.md#418-访问者记录-visits)

### 子任务

#### 后端 API

- [x] 10.1 实现访问记录上报接口（POST /api/visits/track）
- [x] 10.2 实现阅读数去重逻辑（同一访客短时间不重复计数）
- [x] 10.3 实现访客记录查询 API（时间/路径/设备/类型筛选）
- [x] 10.4 实现访问统计聚合 API（今日/昨日/总访问量、热门页面、趋势数据）
- [x] 10.5 实现访问记录自动清理定时任务（默认保留 30 天，后台可配置）
- [x] 10.6 实现阅读数统计图表数据 API（按日/周/月趋势）

#### 后台管理

- [x] 10.7 实现访客记录管理页面（列表 + 筛选 + 详情 + 手动清理）
- [x] 10.8 实现数据统计页面（访问趋势图表、热门文章、评论趋势、设备分布）

#### 前台

- [x] 10.9 实现页面访问上报（所有页面挂载时自动上报）
- [x] 10.10 实现阅读数展示

---

## Phase 11：SEO、性能、部署与安全（中优先级）

**目标：** 全端 SEO 完善、性能优化、Docker Compose 生产部署、安全检查。

**参考文档：** [10-performance-seo.md](blog-platform/10-performance-seo.md)、[11-deployment-devops.md](blog-platform/11-deployment-devops.md)、[09-security-compliance.md](blog-platform/09-security-compliance.md)

### 子任务

#### SEO

- [x] 11.1 自动生成 Sitemap（含文章、分类、标签、自定义页面、项目）
- [x] 11.2 自动生成 Robots.txt
- [x] 11.3 实现 Open Graph / Twitter Card / JSON-LD 结构化数据（全页面覆盖）
- [x] 11.4 图片自动添加 alt

#### 性能

- [x] 11.5 配置图片懒加载（next/image，使用原生 loading="lazy"）
- [x] 11.6 视频按需加载（当前无视频内容，预配置备用）
- [x] 11.7 配置后端缓存（IMemoryCache，缓存首页/文章详情/分类/标签等热点数据）
- [x] 11.8 首页首屏性能优化（< 2 秒，使用 next/font + CSS 变量 + 骨架屏）

#### 部署

- [x] 11.9 配置 Docker Compose（postgres + api + frontend + admin + nginx）
- [x] 11.10 配置 Nginx 反向代理（安全头 + Gzip + 路由代理）
- [x] 11.11 配置数据库备份策略（cron 定时备份到 /backup，保留 30 天）
- [x] 11.12 配置持久化存储（上传目录、日志）
- [x] 11.13 健康检查与监控

#### 安全

- [x] 11.14 Markdown XSS 清洗（前台渲染，使用 rehype-sanitize）
- [x] 11.15 评论 XSS 清洗（评论为纯文本渲染，无危险 HTML）
- [x] 11.16 页脚链接安全校验（rel="noopener noreferrer" + 协议校验）
- [x] 11.17 安全验收（参照 09-security-compliance.md）

---

## Phase 12：功能增强与编辑器升级（已完成）

**目标：** 文章分类多选、标签显示优化、Markdown 编辑器增强、分类支持二级子分类、编辑器改为独立页面、布局管理修复。

### 子任务

#### 后端 API

- [x] 12.1 创建 ArticleCategory 多对多关联实体
- [x] 12.2 配置 AppDbContext ArticleCategories 表（唯一索引 + 级联删除）
- [x] 12.3 更新 ArticleService CreateAsync/UpdateAsync 支持多分类关联
- [x] 12.4 更新 ArticleService GetPagedAsync/GetByIdAsync 返回多分类数据
- [x] 12.5 Category 实体添加 ParentId + Level 字段支持二级分类（原三级改为二级）
- [x] 12.6 更新 CategoryService GetAllAsync 树形结构 + GetFlatListAsync 平面列表
- [x] 12.7 创建数据库迁移 AddMultiCategoryAndSubCategory

#### 后台管理

- [x] 12.8 ArticleEditor.vue 分类改为 el-select multiple 多选
- [x] 12.9 ArticleEditor.vue 标签移除 collapse-tags 全部显示
- [x] 12.10 ArticleEditor.vue 添加视频上传按钮 + 媒体库选择
- [x] 12.11 Categories.vue 树形表格展示二级分类 + 父级选择 + 层级标签
- [x] 12.12 Pages.vue + PageEditor.vue 自定义页面列表/编辑器分离为独立页面
- [x] 12.13 Projects.vue + ProjectEditor.vue 项目列表/编辑器分离为独立页面
- [x] 12.14 Profile.vue 编辑器宽度 100% + TypeScript 类型修复
- [x] 12.15 LayoutManage.vue 添加模块选择对话框 + 分类关联配置

---

## Phase 13：错误修复与编辑器增强（已完成）

**目标：** 修复文章保存时未选分类导致的外键错误、AuthorId/CreatedBy 用户不存在导致的外键错误、Markdown 编辑器图片上传无响应、编辑器集成视频上传。

### 子任务

#### 后端修复

- [x] 13.1 修复未选分类时 CategoryId 赋值 Guid.Empty 问题（改为 null）
- [x] 13.2 ArticleService.CreateAsync 添加作者用户存在校验
- [x] 13.3 MediaService 所有上传方法添加 CreatedBy 用户存在校验
- [x] 13.4 ArticleService.UpdateAsync 修复 CategoryId 同样问题

#### 编辑器增强

- [x] 13.5 修复 MdEditor 图片上传无响应（添加 @on-upload-img 事件处理器）
- [x] 13.6 MdEditor 工具栏添加自定义视频上传按钮（NormalToolbar 组件）
- [x] 13.7 保留原有外部视频上传按钮和媒体库选择功能不变

---

## Phase 14：URL 标识自动生成、编辑器增强与 Bug 修复（已完成）

**目标：** 修复分类管理二级分类重复显示问题、所有 URL 标识（slug）支持中文转拼音自动生成、新建页面编辑器增加视频上传和媒体库选择功能、Markdown 编辑器工具栏全面升级。

### 子任务

#### Bug 修复

- [x] 14.1 修复分类管理二级分类重复显示问题（renderTreeRows 添加 ID 去重，消除可能的树形数据重复）

#### URL 标识自动生成（拼音转换）

- [x] 14.2 创建 Slug 工具函数（`utils/slug.ts`，基于 `pinyin-pro` 库，中文转无声调拼音，特殊字符转连字符）
- [x] 14.3 文章编辑器 URL 标识自动生成（ArticleEditor.vue，标题变化时自动生成 slug）
- [x] 14.4 分类管理 URL 标识自动生成（Categories.vue，名称变化时自动生成 slug）
- [x] 14.5 标签管理 URL 标识自动生成（Tags.vue，名称变化时自动生成 slug）
- [x] 14.6 页面编辑器 URL 标识自动生成（PageEditor.vue，标题变化时自动生成 slug）

#### 页面编辑器增强

- [x] 14.7 PageEditor.vue 添加图片拖拽上传（@on-upload-img 事件处理器）
- [x] 14.8 PageEditor.vue 添加编辑器工具栏视频上传按钮（NormalToolbar 组件）
- [x] 14.9 PageEditor.vue 添加外部视频上传按钮（el-upload 组件）
- [x] 14.10 PageEditor.vue 添加媒体库图片选择对话框
- [x] 14.11 PageEditor.vue 添加媒体库视频选择对话框

#### Markdown 编辑器工具栏升级

- [x] 14.12 ArticleEditor.vue MdEditor 工具栏升级（添加上下标、高亮、任务列表、撤销/重做、保存、预览切换、目录、KaTeX、Mermaid）
- [x] 14.13 PageEditor.vue MdEditor 工具栏升级（同上，保持与文章编辑器一致）

---

## Phase 15：认证持久化、分类管理重写与媒体库对话框升级（已完成）

**目标：** 修复后台刷新页面自动跳转登录页问题；重写分类管理页面改用 el-table 原生 tree-props 渲染彻底解决二级分类重复显示；所有编辑页面添加视频上传与媒体库按钮并定位到编辑器右上角；媒体库对话框全面升级支持分页、上传按钮、多文件上传。

### 子任务

#### Bug 修复

- [x] 15.1 修复后台刷新页面跳转登录页（auth store user 持久化到 localStorage，页面加载时从 localStorage 恢复用户信息）
- [x] 15.2 重写分类管理页面（Categories.vue，改用 el-table `:tree-props` 原生树形渲染替代手动 flatten，从根本上避免重复问题）

#### 编辑器增强

- [x] 15.3 ProjectEditor.vue 添加视频上传按钮、从媒体库加载图片/视频按钮，置于编辑器右上角
- [x] 15.4 PageEditor.vue 媒体库按钮移到编辑器右上角（justify-content: flex-end）
- [x] 15.5 ArticleEditor.vue 媒体库按钮已在编辑器右上角位置（未变动）

#### 媒体库对话框升级

- [x] 15.6 创建共享 MediaSelector.vue 组件（替代各页面独立实现的媒体对话框）
- [x] 15.7 MediaSelector 添加分页支持（使用后端分页 API）
- [x] 15.8 MediaSelector 添加上传按钮（图片/视频），支持多文件同时上传
- [x] 15.9 上传完成后自动刷新到第一页，最新上传的文件显示在最前面（后端已按 CreatedAt DESC 排序）
- [x] 15.10 替换 PageEditor.vue、ArticleEditor.vue、ProjectEditor.vue 中的旧媒体对话框

#### 附件类型支持

- [x] 15.11 MediaSelector.vue 新增 file 类型支持（附件，排除可执行文件 .exe/.bat/.sh 等）
- [x] 15.12 MediaSelector.vue 网格改为 3 列（`:span="8"`，之前 `:md="6"` 导致 4-5 列）
- [x] 15.13 ArticleEditor.vue 添加上传附件按钮和媒体库附件按钮（`handleFileUpload` 函数）
- [x] 15.14 PageEditor.vue / ProjectEditor.vue 同步添加附件上传和媒体库附件按钮
- [x] 15.15 修复 Media.vue 查询条件多行显示问题（`layout="inline"` 改为 `:inline="true"`）

---

## Phase 16：媒体库搜索、每页行数调整与附件支持收尾（已完成）

**目标：** MediaSelector 添加搜索功能、每页显示改为 3 行（pageSize=9）、TASK.md 记录收尾。

### 子任务

- [x] 16.1 MediaSelector.vue 添加关键词搜索功能（keyword 输入框 + API 参数传递）
- [x] 16.2 MediaSelector.vue 每页显示改为 3 行（3 列 × 3 行 = pageSize=9）
- [x] 16.3 更新 TASK.md 记录 Phase 15/16 开发记录和新增需求

---

## Phase 17：媒体库增强、修改密码与图片预览优化（已完成）

**目标：** Media.vue 添加视频/附件上传按钮；后台添加管理员修改密码功能；MediaSelector 图片预览改为完整显示+16:9；媒体库插入改为插入到编辑器光标位置。

### 子任务

#### 媒体库增强

- [x] 17.1 Media.vue 添加上传视频（`uploadVideoApi`）和上传附件（`uploadFileApi`）按钮
- [x] 17.2 三个按钮独立 loading 状态（`uploadingImage` / `uploadingVideo` / `uploadingFile`）

#### 修改密码

- [x] 17.3 后端 AuthDtos.cs 新增 `ChangePasswordRequest` DTO
- [x] 17.4 后端 IAuthService + AuthService 新增 `ChangePasswordAsync` 方法（验证当前密码 + 新密码长度校验 + BCrypt 加密）
- [x] 17.5 后端 AuthController 新增 `POST /api/auth/change-password` 端点（`[Authorize]`）
- [x] 17.6 前端 `api/auth.ts` 新增 `changePasswordApi`
- [x] 17.7 创建 `ChangePassword.vue` 页面（当前密码 + 新密码 + 确认新密码）
- [x] 17.8 router 添加 `/change-password` 路由
- [x] 17.9 AdminLayout.vue 用户下拉菜单添加"修改密码"项 + 侧边栏系统设置分组添加"修改密码"

#### 图片预览优化

- [x] 17.10 MediaSelector.vue 图片预览改为 `fit="scale-down"`（完整显示不裁剪）
- [x] 17.11 图片容器改为 `aspect-ratio: 16/9`（原 4:3），视频/附件占位符统一 16:9
- [x] 17.12 Media.vue `.media-preview` 统一改为 `aspect-ratio: 16/9`

#### 编辑器光标插入

- [x] 17.13 ArticleEditor/PageEditor/ProjectEditor `insertMediaUrl` 改为优先使用 `editorRef.insertText()`（插入到编辑器光标位置），降级追加末尾
- [x] 17.14 添加 `ElMessage.success('已插入到编辑器')` 反馈提示

#### Slug 生成逻辑修复

- [x] 17.15 修复 `toSlug()` 混合中文+英文时英文被逐字拆分问题（"我的Blog" 之前输出 "wo-deblog"，改为 "wo-de-blog"）
  - 原因：`pinyin-pro` 的 `pinyin()` 将英文也逐字拆分（"Blog" → "B l o g"）
  - 修复：按中文/非中文分段，仅中文段过 `pinyin()`，非中文段原样保留，段间用空格拼接

#### 媒体库插入光标位置修复

- [x] 17.16 修复 MediaSelector 插入内容到 Markdown 编辑器时始终追加到末尾的问题
  - 原因：`md-editor-v3` 没有 expose `insertText()` 方法，`editorRef.value?.insertText` 始终为 `undefined`，降级到 `form.value.content +=`（追加末尾）
  - 修复：改用 `editorRef.value?.getEditorView()` 拿到 CodeMirror `EditorView`，通过 `view.dispatch({ changes: { from: pos, insert } })` 在光标位置插入，最后 `view.focus()` 恢复焦点
  - 涉及：ArticleEditor.vue、PageEditor.vue、ProjectEditor.vue

---

## Phase 18：标签配色扩展、项目封面/Markdown 修复与评论 IP 记录（已完成）

**目标：** 后台标签配色从 8 种扩展到 18 种；项目详情页修复 Markdown 渲染；后台编辑项目时封面图和技术栈丢失修复；评论记录真实 IP + 城市级地理定位。

### 子任务

#### 标签配色扩展

- [x] 18.1 Tags.vue 预设配色方案从 8 种扩展到 18 种（新增玫红、天蓝、茶棕、松绿、靛蓝、珊瑚、灰蓝、薄荷、紫灰、米白）

#### 项目详情 Markdown 渲染

- [x] 18.2 项目详情页改用 MarkdownContent 组件（复用文章详情的 react-markdown 渲染管线），替代 DOMPurify + dangerouslySetInnerHTML

#### 项目编辑修复

- [x] 18.3 后端 ProjectDetailResponse DTO 添加 CoverImageId 字段
- [x] 18.4 后端 GetByIdAsync 不再使用 EF Select 投影，改为 post-query JSON 解析 TechStack，映射 CoverImageId
- [x] 18.5 前端 ProjectDetail 类型添加 coverImageId 字段
- [x] 18.6 前端 loadProject 正确读取 data.coverImageId（之前硬编码为 null 导致编辑时封面图丢失）

#### 评论 IP 记录

- [x] 18.7 Comment 实体添加 IpAddress（真实IP）和 Location（地理位置）字段
- [x] 18.8 CommentController 提取 X-Forwarded-For/X-Real-IP 请求头
- [x] 18.9 CommentService 新增 ExtractRealIp() / GetGeoLocationAsync()（ip-api.com 城市级定位，带缓存）
- [x] 18.10 创建 AddIpAndLocationToComment 数据库迁移
- [x] 18.11 后台 Comments.vue 新增 IP/位置列显示

---

## Phase 19：后台设置与前台 SEO 同步（已完成）

**目标：** 后台 SEO 配置和站点设置能够正确应用到前台各页面，不再使用硬编码值。

### 子任务

#### 类型与 API

- [x] 19.1 types/index.ts 添加 PublicSiteSettingResponse（站点名/描述/Logo/Favicon）和 PublicSeoSettingResponse（标题/描述/关键词/OG图/权威链接）接口
- [x] 19.2 lib/api.ts 添加 fetchSiteSettings() 和 fetchSeoSettings(pageKey) 函数，从后端 API 获取数据

#### 根布局站点设置同步

- [x] 19.3 layout.tsx 用 generateMetadata 替代静态 metadata，动态从 GET /api/settings 获取站点名/描述/Favicon/OG 图片
- [x] 19.4 layout.tsx RootLayout 组件改为 async，从 API 获取站点设置填充 JSON-LD 结构化数据

#### 所有页面 SEO 同步

- [x] 19.5 首页 page.tsx → fetchSeoSettings("home")
- [x] 19.6 文章列表 articles/page.tsx → fetchSeoSettings("articles")
- [x] 19.7 搜索 search/page.tsx → fetchSeoSettings("search")
- [x] 19.8 归档 archive/page.tsx → fetchSeoSettings("archive")
- [x] 19.9 分类 categories/page.tsx → fetchSeoSettings("category")
- [x] 19.10 标签 tags/page.tsx → fetchSeoSettings("tag")
- [x] 19.11 关于 about/page.tsx → fetchSeoSettings("about")
- [x] 19.12 项目 projects/page.tsx → fetchSeoSettings("project")
- [x] 19.13 友链 friends/page.tsx → fetchSeoSettings("friends")
- [x] 19.14 留言板 guestbook/page.tsx → fetchSeoSettings("message")
- [x] 19.15 全部页面 Fallback：后台未配置时使用硬编码默认值，不影响正常渲染

#### 导航栏站点 Logo/Favicon/名称同步

- [x] 19.16 DynamicNavbar.tsx 新增 /api/settings 客户端获取站点名、Logo URL、Favicon URL
- [x] 19.17 桌面端标题行改用站点名（有 Logo 图片时显示图片，无图时显示首字符品牌色 + 其余文字）
- [x] 19.18 移动端 Logo 区同样从 API 获取站点名/Logo
- [x] 19.19 useEffect 动态更新 <link rel="icon"> 和 <link rel="shortcut icon"> 为后台配置的 Favicon URL

---

## Phase 20：邮件订阅系统（已完成）

**目标：** 实现完整的邮件订阅系统，包括 SMTP 配置、邮件模板管理、订阅确认流程和退订功能。

### 子任务

#### 后端 API

- [x] 20.1 创建 EmailSetting 实体（SMTP 服务器/端口/用户名/密码/发件人/SSL）
- [x] 20.2 创建 EmailTemplate 实体（模板键/主题/HTML 内容/描述）
- [x] 20.3 创建 EmailLog 实体（发送日志记录）
- [x] 20.4 创建 EmailSettingController（GET/PUT 配置 + 测试发送）
- [x] 20.5 创建 EmailTemplateController（GET/PUT/POST 重置模板）
- [x] 20.6 扩展 SubscriptionController（确认订阅/退订验证码/确认退订）
- [x] 20.7 实现 EmailService（SMTP 发送 + 模板管理 + 3 个默认模板）
- [x] 20.8 扩展 SubscriptionService（token 确认 + 验证码退订 + 退订确认邮件）

#### 后台管理

- [x] 20.9 创建 EmailSettings.vue（SMTP 配置表单 + 测试发送对话框）
- [x] 20.10 创建 EmailTemplates.vue（3 个模板编辑 + 预览 + 重置默认）
- [x] 20.11 更新 Subscribers.vue（显示确认状态）
- [x] 20.12 更新 Router 添加邮件设置和邮件模板路由
- [x] 20.13 更新 AdminLayout.vue 侧边栏添加邮件设置/邮件模板菜单

#### 前台页面

- [x] 20.14 创建订阅确认页面（/subscribe/confirm，含加载/成功/过期/无效状态）
- [x] 20.15 创建退订页面（/unsubscribe，两步流程：邮箱前缀 → 验证码 → 确认退订）
- [x] 20.16 设计 3 个默认邮件模板（确认订阅/订阅详情/已退订）

---

| 日期 | 阶段 | 完成内容 | 备注 |
|------|------|----------|------|
| 2026-06-10 | Phase 20 | 完整邮件订阅系统：后端 EmailSetting/EmailTemplate/EmailLog 实体+Service+Controller；前台确认订阅+退订流程页面；后台邮件设置+邮件模板管理界面；3 个默认邮件模板（确认订阅含3天有效按钮、订阅详情含退订链接、已退订含重新订阅）；退订验证码流程（邮箱前缀匹配+5分钟缓存+发送验证码+输入确认） | 后端+后台 构建通过 |

---

| 日期 | 阶段 | 完成内容 | 备注 |
|------|------|----------|------|
| 2026-06-08 | Phase 19 | 前端全站 SEO/站点设置与后台同步：layout.tsx 改用 generateMetadata 从 API 获取站点名/描述/Favicon/OG 图片；首页/文章/搜索/归档/分类/标签/关于/项目/友链/留言板 10 个页面改用 generateMetadata 从后端 SEO 配置获取标题/描述/关键词 | 前端构建通过 |
| 2026-06-08 | Phase 19 补完 | DynamicNavbar 从 /api/settings 获取站点名、Logo URL、Favicon URL，替换硬编码的 "Zhu's Blog"/"MBlog"；桌面端/移动端品牌区支持 Logo 图片显示或首字符着色；useEffect 动态更新 Favicon 为后台配置值 | 前端构建通过 |
| 2026-06-05 | Phase 0 | 项目初始化全部完成（三端项目骨架、Docker Compose、环境变量模板、.gitignore、Git 提交） | - |
| 2026-06-05 | Phase 1.1-1.2 | 配置 EF Core + PostgreSQL 连接（支持环境变量），创建 17 个实体类及 AppDbContext | - |
| 2026-06-05 | Phase 1.3-1.12 | 创建 EF Core 初始迁移，实现 ApiResponse 统一响应结构、全局异常中间件、Serilog 日志、JWT 认证、FluentValidation 全局校验、CORS、健康检查接口 | - |
| 2026-06-06 | Phase 2 | 后台管理基础框架搭建完成（Vue Router 20 个路由、Axios 拦截器含 Token 自动刷新、Pinia auth store、登录页、路由守卫、后台布局、仪表盘、菜单按角色展示） | 完成 8 个子任务 |
| 2026-06-06 | Phase 3 | 内容管理系统全部完成（分类/标签 CRUD+排序、文章 CRUD+草稿/发布/下架/置顶/推荐、图片上传、文章编辑器含 Markdown 编辑器+封面图上传+分类/标签选择+SEO 信息配置） | 前后端均构建通过 |
| 2026-06-06 | Phase 4 | 前台博客系统全部完成（文章/分类/标签/归档/搜索公开 API、首页/文章列表/详情/分类/标签/归档/搜索页面、SEO Meta、响应式三端适配、导航栏+页脚组件） | 前后端均构建通过 |
| 2026-06-06 | Phase 5 | 评论与反灌水系统全部完成（CaptchaController、CommentService、AdminCommentController、Rate Limiting、后台评论管理页、前台 CommentForm/CommentList 组件） | 后端和前台均构建通过 |
| 2026-06-06 | Phase 0-5 验证 | 全面验证 Phase 0-5 所有开发内容，修复缺失的封面图上传功能，确认三端构建通过 | 所有子任务已完成 |
| 2026-06-07 | Phase 6 | 统一设计系统与全局重构全部完成：CSS 自定义属性 Light/Dark 双主题（tokens.css）、分类品牌色映射、字体/间距/圆角/阴影 Token；导航栏重构（两行布局+后台API菜单+桌面/平板/移动端三端响应式+Dark Mode切换+搜索交互）；页脚重构（版权+社交链接+响应式适配）；MobileBottomNav 设计 Token 化；全局响应式适配（断点、容器、骨架屏） | 构建通过 |
| 2026-06-07 | Phase 7 | 前台页面全面升级全部完成：首页 Hero+标签+精选/最新/项目模块；文章列表页粘性搜索条+可折叠侧栏+双视图切换；文章详情页阅读进度条+浮动TOC+右侧栏+900px正文宽+Markdown导读卡片；搜索页；归档页时间线；分类页品牌色卡片；标签云页彩色标签；自定义页面新设计；项目列表+详情页新设计；留言板；友情链接；404页面 | TypeScript 构建通过 |
| 2026-06-07 | Phase 8 | 个人品牌模块全部完成：个人页面配置 API（ProfileSection CRUD）；视频/附件/图片上传 API 及媒体文件管理 API（UploadController+MediaController+MediaService）；后台个人页面配置页面（Profile.vue，模块增删/排序/类型选择/启用开关）；后台媒体库页面（Media.vue，上传/预览/分页/筛选/删除/复制URL）；前台关于我页面（about/page.tsx，按模块渲染 Hero/简介/技能/经历/联系+默认回退内容+Phase 7 设计 Tokens） | 前端+后台 TypeScript 均构建通过 |
| 2026-06-07 | Phase 9 | 自定义页面与站点配置全部完成：后端 PageController CRUD+发布、NavigationController 树形+位置+可见性、FooterController 版权/备案/社交/样式、ThemeController 主色/强调色/圆角/自定义CSS、LayoutController 模块按页面Key批量保存、SiteSettingController 站点名/描述/评论审核/访问保留；后台 Pages.vue 自定义页面管理、Navigation.vue 树形菜单管理、Footer.vue 页脚配置含Logo上传、Theme.vue 颜色选择器+实时预览、LayoutManage.vue 模块排序+启用开关、Settings.vue 站点设置；前台 DynamicNavbar API 获取菜单+二级下拉、DynamicFooter API 获取版权+ICP+社交、ModuleRenderer 模块引擎、[slug] 动态路由、主题色 CSS 变量 | 后端+后台+前端 三端构建均通过 |
| 2026-06-07 | Phase 10 | 访问统计与访客记录全部完成：访问上报接口（POST /api/visits/track）+ 阅读数去重 + 查询/聚合/清理 API；后台访客管理页面（筛选+详情+清理）+ 数据统计页面（趋势图表）；前台 VisitTracker 组件页面挂载自动上报 + 阅读数展示 | 三端构建通过 |
| 2026-06-07 | Phase 11 | SEO、性能、部署与安全全部完成：Sitemap + Robots.txt + OG/Twitter Card/JSON-LD 全页面覆盖 + 图片 alt；图片原生 loading="lazy" 懒加载 + 后端 IMemoryCache 缓存；Docker Compose 三端+Nginx 编排 + Nginx 安全头/Gzip/代理 + 数据库备份脚本 + 持久化存储 + 健康检查；Markdown rehype-sanitize XSS 清洗 + 页脚链接 rel="noopener noreferrer" 协议校验 | 三端构建均通过 |
| 2026-06-07 | Phase 7 补完 | 补充实现缺失需求：404 页面建议卡片（最新文章+热门标签+全部分类）；自定义页面可选评论区（Page.EnableComments 字段+后台开关+前台联系入口）；标记 7.1.9 后台首页模块布局为已实现（LayoutManage.vue 已存在） | 三端构建均通过 |
| 2026-06-08 | Phase 11 补完 | 修复仪表盘 DateTimeOffset PostgreSQL 兼容性问题；重构后台菜单为二级菜单分组结构（4 组：数据统计/内容管理/页面管理/系统设置）；Profile/Projects/Pages 页面文本编辑器替换为 md-editor-v3 Markdown 编辑器；实现完整 SEO 配置管理页面（列表+编辑+Sitemap 生成） | 后端+后台 构建通过 |
| 2026-06-08 | Phase 12 | 文章分类改为多选（ArticleCategories 多对多关系 + ArticleEditor 分类多选）；标签选择全部显示（去掉+N）；Markdown 编辑器增强（视频上传 + 媒体库选择）；分类管理支持三级子分类（ParentId + 树形表格）；自定义页面编辑弹窗改为全屏宽度；个人页面编辑器宽度 100%；项目编辑弹窗加宽；布局管理添加模块选择对话框 + 分类关联配置；Profile.vue TypeScript 修复 | 后端+后台 构建通过 |
| 2026-06-08 | Phase 12 补完 | 分类由三级改为最多二级（后端 Level 限制 + 前端 UI 调整）；自定义页面编辑和项目编辑从弹窗改为独立页面（PageEditor.vue + ProjectEditor.vue），避免内容过长需要滚动的问题 | 后端+后台 构建通过 |
| 2026-06-08 | Phase 12 优化 | 左侧菜单默认折叠（仅当前访问的分组自动展开）；分类管理列表显示修复；文章编辑分类选择改为 el-tree-select 树形下拉（子分类在父级下缩进显示）；编辑页面（文章/自定义页面/项目）左侧菜单自动高亮对应父级菜单项 | 后端+后台 构建通过 |
| 2026-06-08 | Phase 13 | 修复文章未选分类 CategoryId Guid.Empty FK 错误；ArticleService/MediaService 添加用户存在校验防止 AuthorId/CreatedBy FK 错误；修复 MdEditor 图片上传无响应（@on-upload-img）；工具栏添加自定义视频上传按钮（NormalToolbar）；保留原有外部视频上传和媒体库选择功能 | 后端+后台 构建通过 |
| 2026-06-08 | Phase 14 | 修复分类管理二级分类重复显示（renderTreeRows 去重）；所有 URL 标识支持中文转拼音自动生成（安装 pinyin-pro 库，创建 toSlug 工具函数，更新 ArticleEditor/Categories/Tags/PageEditor）；PageEditor 添加视频上传+媒体库插入功能（与 ArticleEditor 一致）；MdEditor 工具栏全面升级（上下标、高亮、任务列表、撤销/重做、KaTeX、Mermaid 等） | 后端+后台 TypeScript 均无错误 |
| 2026-06-08 | Phase 15 | 修复后台刷新跳转登录页（auth store user localStorage 持久化）；重写分类管理（tree-props 原生树形渲染）；编辑器增强（视频上传+媒体库+附件上传按钮右上角）；MediaSelector 共享组件（分页+上传+多文件+file 类型）；Media.vue 查询条件一行修复；网格 3 列 | 后端+后台构建通过 |
| 2026-06-08 | Phase 16 | MediaSelector 添加关键词搜索（300ms 防抖+清除按钮）；每页显示改为 3 行（pageSize=9）；TASK.md 记录更新 | 后台 TypeScript 构建无错误 |
| 2026-06-08 | Phase 17 | Media.vue 添加上传视频+附件按钮（独立 loading）；后端修改密码 API（ChangePassword DTO+Service+Controller）；前端 ChangePassword.vue 页面+路由+侧边栏/下拉菜单入口；MediaSelector 图片预览 scale-down + 16:9（原 4:3）；编辑器光标插入（insertText 优先+降级追加）；修复 toSlug 混合中英文拆分问题；修复 MediaSelector 插入始终到末尾（nextTick 延迟插入） | 后端+后台 构建通过 |
| 2026-06-10 | 邮件订阅系统 | 完整邮件订阅系统：SMTP 配置管理页面（SmtpSettings.vue + SmtpTest + 邮件发送测试）+ 3 个邮件模板 CRUD（EmailTemplates.vue + EmailTemplateEditor.vue）+ 订阅 API（前台 /api/subscribe + 后台 /api/admin/subscribers CRUD）+ 确认订阅（确认令牌邮件 + GET /api/subscribe/confirm）+ 退订验证码流程（前台 /subscribe/unsubscribe 页面 + 验证码验证）+ 默认模板自动初始化 + 发送日志记录（EmailLog 实体记录） | 后端+后台 构建通过 |
| 2026-06-10 | 每周订阅发送 | 每周一 6:00 定时发送周报（WeeklyDigestJob BackgroundService）+ 周报内容：本周新文章/热门文章/推荐文章 + 邮件模板（subscription_detail） + 发送日志记录 + 无内容可发送时跳过 | 后端 构建通过 |
| 2026-06-10 | 发送记录管理 | 后台邮件发送记录查看：EmailLogs.vue + 路由 /email-logs + 侧边栏菜单入口 + 分页列表（收件人/主题/状态/时间/错误信息） | 后端+后台 构建通过 |
| 2026-06-10 | 补发周报功能 | WeeklyDigestService 抽取独立服务（IWeeklyDigestService），重构 WeeklyDigestJob 委托调用；新增 /api/admin/subscribers/resend-weekly（全部订阅者）+ /api/admin/subscribers/{id}/resend-weekly（单用户）API；Subscribers.vue 右上角"📬 补发周报"按钮 + 每行"补发"按钮（loading/disabled 状态）；ISubscriptionService 新增 GetByIdAsync；SubscriptionService 新增 GetByIdAsync 实现 | 后端+后台 构建通过 |

---

## 新增需求记录

| 日期 | 需求描述 | 优先级 | 状态 |
|------|----------|--------|------|
| 2026-06-10 | 新增完整邮件订阅系统（SMTP 设置+3个邮件模板+订阅确认+退订验证码流程+前台确认/退订页面） | 高 | 已完成 |
| 2026-06-06 | 新增完整设计系统（Design Tokens：色板/字体/间距/圆角/阴影/Dark 模式） | 高 | 已完成 |
| 2026-06-06 | 新增响应式设计规范（三端断点、导航栏适配、各页面适配规则） | 高 | 已完成 |
| 2026-06-06 | 首页全新重新设计（标题行搜索、Hero 品牌展示、精选/最新/项目模块化布局） | 高 | 已完成 |
| 2026-06-06 | 文章详情页全新设计（阅读进度条、浮动 TOC、右侧栏、900px 正文宽） | 高 | 已完成 |
| 2026-06-06 | 文章列表页全新设计（粘性搜索条、可折叠侧栏、卡片/列表双视图切换） | 高 | 已完成 |
| 2026-06-06 | 新增留言板页面（表单 + 留言列表 + 反灌水） | 中 | 已完成 |
| 2026-06-06 | 新增 404 页面（友好错误提示 + 建议引导卡片） | 中 | 已完成 |
| 2026-06-06 | 新增自定义页面（轻量内容页，动态 slug 路由） | 中 | 已完成 |
| 2026-06-06 | 新增项目详情页（Hero + 截图 + 介绍） | 中 | 已完成 |
| 2026-06-06 | 归档/分类/标签/友链/项目列表页视觉升级 | 中 | 已完成 |
| 2026-06-08 | 文章分类改为多选（一篇文章对应多个分类） | 高 | 已完成 |
| 2026-06-08 | 标签选择显示全部标签（去掉+N 显示） | 高 | 已完成 |
| 2026-06-08 | Markdown 编辑器更换为功能更强的版本（支持视频上传） | 高 | 已完成 |
| 2026-06-08 | 分类管理支持三级子分类（ParentId 递归） | 高 | 已完成 |
| 2026-06-08 | 自定义页面编辑弹窗放大为全屏 | 中 | 已完成 |
| 2026-06-08 | 个人页面编辑器宽度放大到 100% | 中 | 已完成 |
| 2026-06-08 | 项目展示编辑弹窗加宽 | 中 | 已完成 |
| 2026-06-08 | 布局管理添加模块时默认类别选择修复 | 中 | 已完成 |
| 2026-06-08 | 修复后台刷新页面跳转登录页（auth store user 未持久化） | 高 | 已完成 |
| 2026-06-08 | 分类管理二级分类重复显示问题修复（重写页面改用 tree-props 树形渲染） | 高 | 已完成 |
| 2026-06-08 | ProjectEditor 添加视频上传/媒体库按钮到编辑器右上角 | 高 | 已完成 |
| 2026-06-08 | PageEditor 媒体库按钮移到编辑器右上角 | 高 | 已完成 |
| 2026-06-08 | 创建共享 MediaSelector 组件，媒体库对话框升级（分页 + 上传按钮 + 多文件上传 + 最新在前） | 高 | 已完成 |
| 2026-06-08 | MediaSelector 新增附件类型（file）支持，排除可执行文件 | 高 | 已完成 |
| 2026-06-08 | ArticleEditor/PageEditor/ProjectEditor 添加上传附件和媒体库附件按钮 | 高 | 已完成 |
| 2026-06-08 | 修复 Media.vue 查询条件多行显示问题 | 高 | 已完成 |
| 2026-06-08 | MediaSelector 每页改为 3 行（pageSize=9）+ 添加搜索功能 | 高 | 已完成 |
| 2026-06-08 | Media.vue 添加上传视频和上传附件按钮 | 高 | 已完成 |
| 2026-06-08 | 后台添加管理员修改密码功能（后端 API + 前端 ChangePassword.vue + 侧边栏/下拉菜单入口） | 高 | 已完成 |
| 2026-06-08 | MediaSelector 图片预览改为完整显示（scale-down）+ 16:9 比例 | 中 | 已完成 |
| 2026-06-08 | 媒体库插入内容改为插入到编辑器光标位置（insertText） | 高 | 已完成 |
| 2026-06-08 | 媒体库、TASK.md 等功能变更记录更新 | 中 | 已完成 |
| 2026-06-08 | 前端 Next.js 添加 /uploads 代理规则（图片无法访问修复） | 高 | 已完成 |
| 2026-06-08 | Server Component 事件处理器修复（onMouseOver/onMouseOut → CSS hover 类，涉及 articles/projects/archive/search 页） | 高 | 已完成 |
| 2026-06-08 | 项目封面图入库/编辑修复（ProjectEditor 媒体库选择封面图改为调用 openCoverMediaSelector） | 高 | 已完成 |
| 2026-06-08 | 前端标签颜色全部从 API 获取（color+bgColor），不再硬编码 | 高 | 已完成 |
| 2026-06-08 | 项目详情页封面图 object-contain 完整显示（修复裁剪） | 中 | 已完成 |
| 2026-06-08 | 评论提交后提示"正在等待审核" | 中 | 已完成 |
| 2026-06-08 | 评论记录真实 IP（X-Forwarded-For/X-Real-IP）+ 城市级地理定位 | 高 | 已完成 |
| 2026-06-08 | 文章详情评论数显示不准确修复 | 高 | 已完成 |
| 2026-06-08 | 后台标签配色方案扩展到 18 种 | 中 | 已完成 |
| 2026-06-08 | 项目详情页修复 Markdown 渲染（改用 react-markdown） | 高 | 已完成 |
| 2026-06-08 | 后台编辑项目时封面图和技术栈丢失修复 | 高 | 已完成 |
| 2026-06-08 | 首页精选文章卡片封面图高度 90px → 180px | 中 | 已完成 |
| 2026-06-08 | ScrollReveal 性能优化（threshold 0.01、rootMargin -40px、prefers-reduced-motion 提前检查） | 中 | 已完成 |
| 2026-06-08 | 首页最新文章卡片封面图去掉分类/标签徽章，无图时显示首字母图标 | 高 | 已完成 |
| 2026-06-08 | 文章详情页 TOC 面板加宽（140px → 220px）、字体放大、修复中文标题点击跳转、添加活跃指示器 | 高 | 已完成 |
| 2026-06-08 | headings.ts 中文标题 slug 生成修复（空 slug 时使用 charCode 兜底） | 高 | 已完成 |
| 2026-06-08 | 文章详情页类别标签从标题上方移至元信息行 | 高 | 已完成 |
| 2026-06-08 | 文章详情页右侧操作/分类导航组件字体放大（ShareTools 42×34px、分类导航 text-[13px]） | 中 | 已完成 |
| 2026-06-08 | 全站页面 Hero 标题统一放大（text-[20px] → text-[24px] font-semibold）+ 描述字体放大 | 中 | 已完成 |
| 2026-06-08 | 导航栏字体放大美化（text-sm → text-[15px]，px-3 py-1.5 → px-4 py-2，标题行 h-[36px] → h-[40px]，导航栏 h-[44px] → h-[52px]，图标 16px → 18px） | 中 | 已完成 |
| 2026-06-08 | 导航栏搜索+Dark Mode 图标移到顶部标题行，网站名称放大（text-[15px] → text-[18px] font-bold，Zhu 高亮为主色） | 中 | 已完成 |
| 2026-06-08 | 文章详情页 TOC 目录面板完全重构（玻璃拟态卡片、渐变色条 header、圆点活跃指示器、圆角按钮+主色发光 shadow、支持 Escape 关闭、fadeIn 动画） | 高 | 已完成 |
| 2026-06-08 | 修复导航栏搜索参数名不一致（q → keywords，搜索页预期 keywords） | 高 | 已完成 |
| 2026-06-08 | 修复 TOC 点击不能跳转到对应标题（改为 textContent 匹配，而非 id 匹配） | 高 | 已完成 |
| 2026-06-08 | 文章详情页 content-container 间距缩小（pt-[80px] → pt-[48px], pb-[32px] → pb-[16px]） | 中 | 已完成 |
| 2026-06-08 | 文章详情页支持多分类显示（引入 categories 数组类型，多个分类 badge 横向 wrap 排列在元信息行） | 高 | 已完成 |
| 2026-06-08 | 文章列表页右侧分类/标签字体放大（分类 12px → 13px，标签 10px → 12px，统一 px-3 py-1.5） | 中 | 已完成 |
| 2026-06-08 | 文章列表页卡片视图重构（16:9 封面、无图显示首字母、分类移至封面下方、显示摘要、减少留白） | 高 | 已完成 |
| 2026-06-08 | 文章列表页列表视图完全重做（56×56 方形头像/首字母、分类+标签单独区域、阅读数+日期右侧对齐） | 高 | 已完成 |
| 2026-06-08 | 文章列表页统计总数填充 + 搜索结果栏移除"篇文章"文字 | 高 | 已完成 |
| 2026-06-08 | 文章列表页卡片模式分类移至标题下方，支持全部分类显示 | 高 | 已完成 |
| 2026-06-08 | 全站查看数图标统一替换（📍 → 眼睛 SVG 图标）涉及 ArticlesPageContent、首页、详情页等 | 中 | 已完成 |
| 2026-06-08 | 文章详情页右侧 ShareTools 区域 sticky 固定（top: 80px），滚动时保持可见 | 高 | 已完成 |
| 2026-06-08 | 文章详情页评论系统补全（CommentList + CommentForm CSS 样式，支持写评论/回复/验证码/分页） | 高 | 已完成 |
| 2026-06-08 | 无图卡片首字母 fallback 过滤标点符号，只取字母或汉字（字母转大写） | 中 | 已完成 |
| 2026-06-08 | 文章详情页分类 badges 字体统一加大（text-[11px] → text-[12px]，px-2 → px-2.5） | 中 | 已完成 |
| 2026-06-08 | 修复 PagedData 类型字段不匹配（前端 total → totalCount 匹配后端 PagedResponse.TotalCount） | 高 | 已完成 |
| 2026-06-08 | 修复 articles/page.tsx、search/page.tsx、home page.tsx、ArticlesPageContent.tsx、CommentList.tsx 中所有错误引用 data.total 为 data.totalCount | 高 | 已完成 |
| 2026-06-08 | 文章列表页 Hero 间距缩小（mb-8 → mb-4）| 中 | 已完成 |
| 2026-06-08 | 文章列表页搜索结果统计行删除（搜索条下方的 mb-4 text-[11px] 行）| 中 | 已完成 |
| 2026-06-08 | 侧栏分类/标签标题字体统一放大（text-[13px] → text-[14px]），与详情页 badges 统一 | 高 | 已完成 |
| 2026-06-08 | 文章详情页分类 badges 字体再次加大（text-[12px] → text-[14px]）| 高 | 已完成 |
| 2026-06-08 | 文章详情页右侧操作栏 sticky 修复（移除内层重复 sticky，保留外层 aside sticky self-start）| 高 | 已完成 |
| 2026-06-08 | 全站多分类支持：首页精选/最新文章、搜索页、404 页、文章详情相关文章/分类导航活跃态，全部改用 categories 数组优先 | 高 | 已完成 |
| 2026-06-08 | 项目封面图入库/编辑修复（ProjectEditor 媒体库选择封面图改为调用 openCoverMediaSelector）；前端标签颜色全部从 API 获取（color+bgColor 4 个组件）；项目封面图 object-contain 完整显示；评论提交后提示"等待审核" | 高 | 已完成 |
| 2026-06-08 | 评论记录真实 IP（支持 X-Forwarded-For/X-Real-IP）+ 城市级地理定位（ip-api.com）；文章详情评论数现在取实际审核通过的评论数；项目页 onMouseOver/onMouseOut Server Component 错误修复；Docker 迁移自动修复逻辑 | 高 | 已完成 |
| 2026-06-08 | 后台标签配色方案从 8 种扩展至 18 种；项目详情页改用 MarkdownContent 渲染（修复显示原始 markdown）；ProjectEditor 编辑时封面图和 techStack 丢失修复（后端 DTO 添加 CoverImageId、JSON 解析 TechStack，前端 loadProject 正确读取 coverImageId） | 高 | 已完成 |
| 2026-06-08 | 导航栏站点名/Logo/Favicon 从后台系统设置同步：DynamicNavbar 新增 /api/settings 客户端获取；桌面/移动端品牌区支持 Logo 图片或首字符着色；useEffect 动态更新 Favicon | 高 | 已完成 |
| 2026-06-08 | 首页 Hero 品牌区站点名/Logo/描述从后台系统设置获取（替换硬编码"独立开发者..."）；Footer 完整集成后台配置（socialLinks/blocks/style 三种布局 + 联系信息 + 页脚样式控制）；Navbar 导航菜单从后台导航菜单管理获取（含 children 子菜单）；PublicFooterConfig 类型补齐 contactEmail/WeChat/Phone | 高 | 已完成 |
| 2026-06-09 | 全站封面图 16:9 比例规范化：前端所有封面卡片统一使用 aspect-video；后台上传封面新增 /api/admin/upload/cover 端点自动居中裁剪为 16:9；ArticleEditor/ProjectEditor 封面上传改为裁剪接口；提示文本统一为"建议尺寸 1920×1080（16:9）" | 高 | 已完成 |
| 2026-06-09 | 导航子菜单图标修复：前端子菜单展开图标从十字(+)改回朝下箭头(>)，展开时 rotate-180 | 高 | 已完成 |
| 2026-06-09 | 后台导航管理树形展开/折叠：有子项的菜单旁显示 > 按钮与名称同行，默认只显示顶级，点击展开子项 | 高 | 已完成 |
| 2026-06-09 | 后台页脚社交链接布局调整：IconPicker 宽度 120px → 80px，URL 输入框 220px → 280px | 中 | 已完成 |
| 2026-06-09 | 项目详情页展示 4 个字段（项目地址/GitHub/演示地址/技术栈），无值不渲染；用 2 列网格卡片展示，每个卡片独立图标+标签+URL，后改为圆形图标按钮(hover 放大) + 内联技术栈标签 | 高 | 已完成 |
| 2026-06-09 | 项目列表页卡片底部增加彩色 badge 标签：项目地址(蓝#dbeafe)、GitHub(灰#e5e7eb)、演示(绿#d1fae5)，带小 SVG 图标，flex-wrap 排列 | 高 | 已完成 |
| 2026-06-09 | 项目详情页返回链接美化：纯文字 → 带边框卡片按钮样式 + 左侧箭头 SVG + hover 左移 | 中 | 已完成 |
| 2026-06-09 | 后端 TechStack API 映射修复：ProjectService.GetPublicBySlugAsync 和 GetPublicListAsync 未反序列化 TechStack JSON 字段，导致前台不显示技术栈 | 高 | 已完成 |
| 2026-06-09 | 封面裁剪改为用户交互式：后端移除自动裁剪逻辑（CropToAspectRatio 方法），UploadCover 直接保存原图；前端创建 CoverCropper.vue 组件（cropperjs，16:9 固定比例，用户拖动选择裁剪区域，已 16:9 的图片跳过裁剪弹窗），集成到 ArticleEditor/ProjectEditor | 高 | 已完成 |
| 2026-06-09 | 封面媒体库选择增加裁剪：从媒体库选封面时检测比例，非 16:9 打开 CoverCropper 裁剪；CoverCropper 新增 fromMedia 模式 | 高 | 已完成 |
| 2026-06-09 | 布局管理全面重构：后台 LayoutManage.vue 从4页扩展至15页（分组显示：首页/文章/项目/分类标签/其他），每页模块一一对应前台实际页面；后端 GetDefaultModules 覆盖全部15页的默认模块定义；前台首页集成布局配置，通过 isModuleEnabled 动态控制各模块（hero/hot_tags/featured_posts/recent_posts/projects/subscription）显隐 | 高 | 已完成 |
| 2026-06-09 | 前台剩余13个页面全部集成布局配置（articles/article_detail/archive/search/projects/project_detail/categories/category_detail/tags/tag_detail/about/friends/guestbook），使用 fetchPageLayout + isEnabled 控制各模块显隐；后端 GetByPageKeyAsync 新增自动衬底机制（无配置时自动创建默认布局并持久化）；修复9个 Turbopack JSX 嵌套语法错误；三端构建验证全部通过 | 高 | 已完成 |
| 2026-06-09 | 首页布局后台完全控制：getHomeData 从布局 config 动态读取 count/categoryIds 控制各模块数量；精选文章支持分类筛选（featured_posts.categoryIds）；新增 custom 模块渲染支持（自定义 HTML）；数据获取 pageSize 提升至 50 | 高 | 已完成 |
| 2026-06-09 | 后台 LayoutManage.vue 全面升级：config JSON 按模块类型拆分表单控件（count/autoPlay/interval/columns/categoryIds/displayStyle 等）；可视化配置表单替代原始 JSON 文本编辑框；排序/标题与前端联动；UI 美化（卡片分组、颜色标识、字段标签） | 高 | 已完成 |
| 2026-06-09 | 新增后台模块定义：image_carousel(图片轮播)、video_player(视频播放)、statistics(统计面板)、call_to_action(号召按钮)、divider(分割线)、spacer(间距占位) ，含完整默认配置 | 高 | 已完成 |
| 2026-06-09 | 前台新增模块渲染组件：ImageCarousel / VideoPlayer / CallToAction / Statistics 等，注册到 ModuleRenderer 引擎；排序和标题严格遵循后台配置 | 高 | 已完成 |
| 2026-06-09 | 留言板全面升级：后台新增 MessageReply API（回复/审核/查看）；前台新增 GuestbookMessageList + GuestbookMessageForm 独立组件（非依赖 CommentList）；新增留言审核流程（提交 → 待审核 → 审核通过/拒绝）；前台回复支持（带验证码、须审核）；留言记录 IP 地址和地理位置；用户提交后提示"留言已提交，等待审核" | 高 | 已完成 |
| 2026-06-09 | 修复后台留言管理页面缺失：新增 Messages.vue（列表+筛选+审核+回复+删除）；API 封装 message.ts；侧边栏 AdminLayout.vue 菜单添加"留言管理"；路由注册 | 高 | 已完成 |
| 2026-06-09 | 修复 messages 建表缺失问题：Program.cs EnsureCreated 中添加 CREATE TABLE IF NOT EXISTS messages | 高 | 已完成 |
| 2026-06-09 | 前台留言表单布局修复：提交按钮移至验证码右侧（与验证码/换一张同行）；表单昵称输入框与留言内容区增加间距（mt-4）；回复按钮移至时间戳右侧（header 行右对齐）；修复留言回复未提交问题（parentId 传递和回调机制） | 高 | 已完成 |
| 2026-06-09 | 后台留言/评论管理增强：Messages.vue 内容列可点击弹出详情层（完整留言信息+回复内容）；Comments.vue 内容列可点击弹出详情层（完整评论+引用文章）；Comments.vue 新增"回复"按钮 + 管理员回复对话框；后端新增 CommentController ReplyAsync API | 高 | 已完成 |
| 2026-06-09 | 后台回复子留言前台不可见修复：ChildReply 组件新增管理员回复渲染（绿色气泡卡片）；后台回复对话框预填已有回复内容（editAdminReply）；操作列宽度 280→340 确保按钮完整显示 | 高 | 已完成 |
| 2026-06-09 | 后台留言嵌套回复支持：MessageService 改为递归层级加载（LoadDescendantsAsync）；API 返回扁平列表（MessageFlatResponse）含父留言者信息；详情弹窗树形展示多级回复含审核按钮 | 高 | 已完成 |
| 2026-06-09 | 前端用户评论表单布局优化：验证码/换一张/验证码图片/提交评论/取消回复全部合并为一行显示 | 高 | 已完成 |
| 2026-06-10 | 前台留言表单提交后立即隐藏（hidden 状态而非仅清空数据），成功提示保留 2 秒后自动消失 | 高 | 已完成 |
| 2026-06-10 | 新增自动填充昵称邮箱：评论和留言表单从 localStorage('blog_user_info') 读取并自动填入，提交时自动更新缓存 | 高 | 已完成 |
| 2026-06-10 | 文章详情操作添加收藏功能（浏览器收藏夹 Ctrl+D/Cmd+D）和二维码 hover 弹窗（qrserver API 200x200） | 高 | 已完成 |
| 2026-06-10 | 文章列表页侧栏 sticky 固定（sticky top-24 self-start）；侧栏分类/标签点击后正确筛选文章列表（useEffect 监听 URL 参数）；首页"查看全部"链接改为圆角药丸按钮样式 | 高 | 已完成 |
| 2026-06-10 | 批量创建 51 篇文章脚本（Node.js 实现 UTF-8 编码），自动分配分类/标签/封面图/推荐标记/发布时间 | 高 | 已完成 |
| 2026-06-10 | 修复文章列表搜索无效（handleSearch 直接调用 fetchArticles）；全部分类数量改用 initialTotalAll（独立 API 获取）；侧栏点击清除搜索框；搜索框/下拉框放大至 h-[38px]、统一圆角/字号 | 高 | 已完成 |
| 2026-06-10 | 每周订阅发送任务（每周一6点定时发送 + 周报内容：本周新文章/热门文章/推荐文章 + 邮件模板 + 发送日志） | 高 | 已完成 |
| 2026-06-10 | 后台补发周报功能（全部已确认订阅者 + 单用户补发） | 高 | 已完成 |
| 2026-06-10 | 文章详情 QR 码添加加载占位符（灰色脉冲动画 + 二维码图标 + "加载中..."文字）；Hero 文本始终显示全量文章总数，筛选时追加"在 xx 分类/标签中找到了 xx 篇文章" | 高 | 已完成 |
| 2026-06-10 | 邮件订阅系统：前端订阅表单、确认邮件、退订流程（验证码+补全邮箱+重订阅）、后台管理（列表/筛选/导出/补发周报）、发送日志管理 | 高 | 已完成 |
| 2026-06-10 | 每周订阅发送任务 + 后台补发周报功能（全部和单用户补发） | 高 | 已完成 |
| 2026-06-10 | 退订页面修复：路由改为 /subscribe/unsubscribe?email=xxx，补全邮箱前缀验证，重新订阅表单 | 高 | 已完成 |
| 2026-06-10 | 所有邮件模板 LOGO 控制：有则显示图片，无则完全不显示（confirm_subscription/subscription_detail/unsubscribed） | 高 | 已完成 |
| 2026-06-10 | 后台布局管理：默认首页模块同步前端实际布局 + 全线15页支持可拖拽排序 + 顺序配置直接驱动前台渲染 | 高 | 已完成 |
| 2026-06-10 | 后台主题色配置打通前台：ThemeProvider 转 --color-* CSS 变量（--color-primary/--color-bg/--color-text/--color-border/--color-primary-light/--color-surface），前台所有页面自动应用 | 高 | 已完成 |
| 2026-06-10 | 主题配置全面升级：多主题管理（新建/复制/删除/启用）+ 修复只激活一个主题的问题（新建主题默认不激活）+ 修复导航栏背景色不生效问题（DynamicNavbar 改用 --navbar-bg）+ 新增 8 个可配置样式属性（字体族/成功色/危险色/警告色/页脚背景/页脚文字/Hero 背景/代码块背景）+ 前台 ThemeProvider 完整输出 50+ CSS 变量 | 高 | 已完成 |
| 2026-06-10 | 布局管理大重构：通用模块（分割线/间距占位/自定义/轮播/视频/统计/CTA/公告横幅/引用块/图标列表/卡片横幅）全部支持重复添加 + 所有页面可用 + 全新通用模块渲染引擎 UniversalModuleComponents.tsx；LayoutUniversalModules 客户端组件实现所有页面按后台配置渲染通用模块 | 高 | 已完成 |
| 2026-06-10 | 页面标题自定义：page_hero 模块新增 customTitle/customSubtitle 配置字段，所有页面（文章列表/归档/搜索/项目/分类/标签/关于/友链/留言板）均支持后台自定义大标题和子标题 | 高 | 已完成 |
| 2026-06-10 | 多套配色预设：6 套（极光紫/翡翠绿/落日橙/海洋蓝/玫瑰红/暗夜黑）写入 DbInitializer 种子数据 | 中 | 已完成 |
| 2026-06-10 | 页面添加 LayoutUniversalModules 通用模块渲染组件：archive/search/projects/categories/tags/about/friends/guestbook 页面标题均支持自定义 | 高 | 已完成 |
| 2026-06-10 | 修复首页模块重复渲染（HomeContent 移除末尾 LayoutUniversalModules，避免已由主循环渲染的 image_carousel/statistics 等再次渲染）；修复置顶文章字段名不匹配（后端 IsTop → 前端 isTop，原误用 isPinned 导致永不显示）；修复统计面板数值（HomeContent 新增 statistics renderer 使用真实 totalArticles/tagsCount/projectsCount/totalViews） | 高 | 已完成 |
| 2026-06-10 | 修复后台文章状态切换失效（后端 [HttpPut] 改为 [HttpPatch]，匹配前端 axios 实际发送的 PATCH 请求）；新增彻底删除功能：后端 HardDeleteAsync（连带清理 ArticleTags/ArticleCategories/Comments 关联数据）+ [HttpDelete("{id}/force")] API；后台文章列表新增"回收站"模式（showDeleted 切换，筛选已软删除文章）+ 彻底删除按钮；后台文章查询参数新增 ShowDeleted 字段 | 高 | 已完成 |
| 2026-06-10 | 置顶文章卡片统一使用 ArticleCard 组件，与文章列表卡片样式一致（ArticlesPageContent / categories/[slug] 的 pinned_posts 模块改用 ArticleCard）；首页 pinned_posts 卡片增加分类徽章覆盖层 + 标签区，与 featured_posts 样式一致；首页 featured_posts 封面强制 aspect-video（移除 style 覆写） | 高 | 已完成 |
| 2026-06-10 | 修复所有卡片封面 16:9（ArticleCard、HomeContent featured/pinned、ArticlesPageContent card、projects 卡片封面统一使用 `absolute inset-0 w-full h-full object-cover` + `aspectRatio: 16/9`，移除 `flex items-center justify-center` 父容器避免 flex 收缩图片） | 高 | 已完成 |
| 2026-06-10 | 文章列表置顶卡片增加分类显示（showCategories=true）；前台文章详情页增加状态检查，草稿/已下架文章显示友好提示页（"该文章暂未发布" / "该文章已下架" + 返回按钮）；修复回收站无数据问题（toggleTrash 进入回收站时自动重置筛选条件，避免残留的筛选条件导致无搜索结果） | 高 | 已完成 |
| 2026-06-10 | 首页置顶文章列表模式添加阅读时间（"5 分钟"）和右侧标签显示，与最新文章列表样式一致；分类详情页移除置顶文章卡片模块（pinned_posts），因为分类文章列表已通过后端排序将置顶文章优先显示，不再需要独立模块重复展示 | 高 | 已完成 |
| 2026-06-11 | AI 一键写文增加文章内配图比例选择（16:9/9:16/4:3/3:4/1:1），后端根据比例自动选择对应生图尺寸；后端 ReplaceImageMarkers 支持传入比例参数 | 高 | 已完成 |
| 2026-06-11 | 正文编辑器新增"插入 AI 图"按钮（AI 润色左侧），点击弹窗输入提示词 + 选择图片比例，调用 /api/admin/articles/ai/generate-article-image 生成图片并插入编辑器光标位置 | 高 | 已完成 |
| 2026-06-11 | 后端新增 AiGenerateArticleImageRequest + GenerateArticleImageAsync + CompressImage；AI 生图超过 2MB 时用 SkiaSharp 压缩至 80% 质量，宽度超过 1920px 等比缩放 | 高 | 已完成 |
| 2026-06-11 | 分类详情页 + 标签详情页视图切换栏左侧显示"共找到 xx 篇文章"（ArticleListView 新增 totalCount prop）；标签详情页移除 hero 区域旧的文章数文本 | 中 | 已完成 |
| 2026-06-11 | 文章列表页置顶文章去重：置顶模块启用时正常列表过滤置顶文章；置顶模块禁用时置顶文章混入列表并在封面右上角显示橙色"置顶"角标 | 高 | 已完成 |
| 2026-06-11 | 修复 AI 写文/润色超时（API timeout 300s）+ catch 静默吞错误（改为 ElMessage.error + console.error） | 高 | 已完成 |
| 2026-06-12 | RSS/Atom Feed 实现（/feed 端点，RSS 2.0 规范）+ 文章 enclosure 图片链接补全为完整 URL | 高 | 已完成 |
| 2026-06-12 | Open Graph 图片自动生成（首页 + 文章详情页，Next.js ImageResponse API 动态生成） | 高 | 已完成 |
| 2026-06-12 | 全文搜索升级：PostgreSQL tsvector/tsquery + GIN 索引，SQLite 降级为 LIKE 搜索 | 高 | 已完成 |
| 2026-06-12 | 评论回复邮件通知：CommentService.SendReplyNotificationAsync，审核通过时自动发送回复通知邮件 | 高 | 已完成 |
| 2026-06-12 | 图片懒加载 & WebP 优化：next.config.ts 配置 WebP/AVIF 格式，所有 `<img>` 替换为 `next/image`，创建 OptimizedImage 通用组件 | 高 | 已完成 |
| 2026-06-12 | Sitemap 搜索引擎收录提交脚本（deploy/submit-sitemap.ps1，支持 Google/Bing/百度/IndexNow） | 中 | 已完成 |
| 2026-06-12 | PWA 支持：manifest.ts 生成 manifest.json，sw.js 离线缓存 Service Worker，PwaRegistration 自动注册，offline 离线页面 | 中 | 已完成 |
| 2026-06-12 | 数据库备份/导出功能：BackupController 全量 30 张表 JSON 导出（支持 ZIP 压缩），ImportService + ImportController 文件上传导入（JSON/ZIP 解析 → 清空 → 批量插入 → 详细报告） | 高 | 已完成 |
| 2026-06-12 | 后台数据备份导入界面（Backup.vue）：数据概览置顶 + 导出/导入双栏 + 导入报告明细表格 | 中 | 已完成 |
| 2026-06-12 | SiteSetting 新增 ReplyNotificationEnabled（评论回复邮件通知开关，后台系统设置页可切换） | 高 | 已完成 |
| 2026-06-12 | 修复 Robots.txt Sitemap URL：改用环境变量 PUBLIC_SITE_URL/SITE_URL 替代硬编码 example.com | 高 | 已完成 |
| 2026-06-12 | 回滚 SiteUrl 数据库字段：站点 URL 统一复用环境变量 PUBLIC_SITE_URL（SeoService/CommentService/SubscriptionService/EmailService 全部统一），后台系统设置页移除"站点地址"输入框 | 高 | 已完成 |
| 2026-06-12 | 备份导出导入全量覆盖：BackupController.Export() 导出全部 30 张表，ImportService 导入全部 30 张表；stats 端点从 15 项扩展到 30 项；前端 Backup.vue 数据概览展示全部 30 张表统计 | 高 | 已完成 |
| 2026-06-12 | 备份导出支持 ZIP 压缩选项：BackupController.Export([FromQuery] bool zip)，前端弹窗选择"需要压缩(.json.zip)"或"不压缩(.json)" | 高 | 已完成 |
| 2026-06-12 | 数据导入功能：ImportController POST /api/admin/import/upload 支持上传 .json 和 .zip 文件，先清空所有表再批量导入，事务完整性保证，返回详细导入报告（每张表成功/失败数量及错误原因） | 高 | 已完成 |
| 2026-06-12 | 修复数据导入 ArticleTag/ArticleCategory 重复追踪冲突：两阶段插入（先清除导航属性再插主表，关联表用 Entry.State=Added 逐条插入） | 高 | 已完成 |
| 2026-06-12 | 备份导入界面优化：数据概览置顶（30 项统计网格），导出/导入双栏并排布局，导入选择文件后显示文件名+开始导入+清除按钮，导入报告明细表格 | 中 | 已完成 |
| 2026-06-12 | 修复导入文件选择第二次不更新：el-upload 替换为原生 input[type=file]，handleFileChange 直接读 e.target.files[0] | 中 | 已完成 |
| 2026-06-12 | 修复备份导出服务器错误：ReplyNotificationEnabled 字段缺少数据库迁移，执行 ef migrations add + database update | 高 | 已完成 |
| 2026-06-12 | 修复登录失败页面刷新：request.ts 401 拦截器排除 /auth/login 和 /auth/refresh 接口，Login.vue 401 显示"用户名或密码错误"而非页面刷新 | 高 | 已完成 |