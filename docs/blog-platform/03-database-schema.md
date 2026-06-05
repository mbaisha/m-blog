# 03. PostgreSQL 数据库设计

## 1. 设计原则

本项目的数据库使用 PostgreSQL，目标是稳定、可扩展、适合生产环境。

### 核心原则

- 主键统一使用 UUID。
- 时间字段统一使用 `timestamptz`。
- 对外 URL 使用 `slug`。
- 高频查询字段建立索引。
- 删除优先使用软删除。
- 可扩展配置使用 `jsonb`。
- 所有表必须有 `created_at` 和 `updated_at`。
- 所有业务表必须有唯一约束或业务约束。
- 所有外键必须有明确删除策略。
- 所有状态字段使用枚举或约束。

---

## 2. 命名规范

### 2.1 表名

使用小写复数形式：

```text
users
articles
categories
tags
comments
media
projects
friends
seo_settings
profile_sections
visits
likes
audit_logs
captcha_sessions
refresh_tokens
```

---

### 2.2 字段名

使用 snake_case：

```text
created_at
updated_at
deleted_at
is_visible
```

---

### 2.3 索引名

格式：

```text
idx_{table}_{column}
```

示例：

```text
idx_articles_slug
idx_articles_published_at
idx_comments_article_id_status
```

---

### 2.4 唯一约束名

格式：

```text
uq_{table}_{column}
```

示例：

```text
uq_users_username
uq_articles_slug
uq_categories_slug
```

---

## 3. 公共字段

建议所有业务表包含：

```text
id UUID PRIMARY KEY
created_at TIMESTAMPTZ NOT NULL DEFAULT now()
updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
```

软删除表额外包含：

```text
deleted_at TIMESTAMPTZ NULL
```

---

## 4. 表结构设计

## 4.1 users

后台管理员表，后续可扩展注册用户。

```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(64) NOT NULL,
    email VARCHAR(255),
    password_hash TEXT NOT NULL,
    role VARCHAR(32) NOT NULL DEFAULT 'admin',
    avatar_media_id UUID,
    status VARCHAR(32) NOT NULL DEFAULT 'active',
    last_login_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at TIMESTAMPTZ NULL,

    CONSTRAINT uq_users_username UNIQUE (username),
    CONSTRAINT uq_users_email UNIQUE (email),
    CONSTRAINT ck_users_role CHECK (role IN ('super_admin', 'admin', 'content_admin', 'comment_moderator')),
    CONSTRAINT ck_users_status CHECK (status IN ('active', 'disabled', 'deleted'))
);

CREATE INDEX idx_users_status ON users(status);
```

字段说明：

| 字段 | 类型 | 说明 |
|---|---|---|
| id | uuid | 主键 |
| username | varchar | 登录名 |
| email | varchar | 邮箱 |
| password_hash | text | 密码哈希 |
| role | varchar | 角色 |
| avatar_media_id | uuid | 头像媒体 ID |
| status | varchar | 账号状态 |
| last_login_at | timestamptz | 最后登录时间 |

---

## 4.2 refresh_tokens

Refresh Token 表。

```sql
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash TEXT NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    revoked_at TIMESTAMPTZ NULL,
    created_ip VARCHAR(45),
    created_user_agent TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_refresh_tokens_token_hash UNIQUE (token_hash)
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_expires_at ON refresh_tokens(expires_at);
```

---

## 4.3 media

媒体资源表。

```sql
CREATE TABLE media (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    type VARCHAR(32) NOT NULL,
    url TEXT NOT NULL,
    thumbnail_url TEXT,
    filename TEXT NOT NULL,
    original_filename TEXT,
    storage_path TEXT NOT NULL,
    size_bytes BIGINT NOT NULL DEFAULT 0,
    mime_type VARCHAR(128) NOT NULL,
    width INT,
    height INT,
    duration_seconds NUMERIC(10,2),
    created_by UUID REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at TIMESTAMPTZ NULL,

    CONSTRAINT ck_media_type CHECK (type IN ('image', 'video', 'file'))
);

CREATE INDEX idx_media_type ON media(type);
CREATE INDEX idx_media_created_at ON media(created_at DESC);
```

字段说明：

| 字段 | 类型 | 说明 |
|---|---|---|
| type | varchar | image / video / file |
| url | text | 对外访问 URL |
| thumbnail_url | text | 缩略图 URL |
| filename | text | 安全文件名 |
| original_filename | text | 原始文件名 |
| storage_path | text | 本地路径或对象存储 key |
| size_bytes | bigint | 文件大小 |
| mime_type | varchar | MIME 类型 |
| width | int | 图片宽度 |
| height | int | 图片高度 |
| duration_seconds | numeric | 视频时长 |

---

## 4.4 categories

文章分类表。

```sql
CREATE TABLE categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(128) NOT NULL,
    slug VARCHAR(128) NOT NULL,
    description TEXT,
    cover_image_id UUID REFERENCES media(id) ON DELETE SET NULL,
    sort_order INT NOT NULL DEFAULT 0,
    seo_title VARCHAR(255),
    seo_description TEXT,
    seo_keywords TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at TIMESTAMPTZ NULL,

    CONSTRAINT uq_categories_slug UNIQUE (slug)
);

CREATE INDEX idx_categories_sort_order ON categories(sort_order, created_at DESC);
```

---

## 4.5 tags

文章标签表。

```sql
CREATE TABLE tags (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(128) NOT NULL,
    slug VARCHAR(128) NOT NULL,
    color VARCHAR(32),
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at TIMESTAMPTZ NULL,

    CONSTRAINT uq_tags_slug UNIQUE (slug)
);

CREATE INDEX idx_tags_name ON tags(name);
```

---

## 4.6 articles

文章表。

```sql
CREATE TABLE articles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL,
    summary TEXT,
    content TEXT NOT NULL,
    cover_image_id UUID REFERENCES media(id) ON DELETE SET NULL,
    author_id UUID NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    category_id UUID REFERENCES categories(id) ON DELETE SET NULL,
    status VARCHAR(32) NOT NULL DEFAULT 'draft',
    is_top BOOLEAN NOT NULL DEFAULT false,
    is_recommend BOOLEAN NOT NULL DEFAULT false,
    view_count BIGINT NOT NULL DEFAULT 0,
    like_count BIGINT NOT NULL DEFAULT 0,
    comment_count BIGINT NOT NULL DEFAULT 0,
    seo_title VARCHAR(255),
    seo_description TEXT,
    seo_keywords TEXT,
    published_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at TIMESTAMPTZ NULL,

    CONSTRAINT uq_articles_slug UNIQUE (slug),
    CONSTRAINT ck_articles_status CHECK (status IN ('draft', 'published', 'archived')),
    CONSTRAINT ck_articles_view_count CHECK (view_count >= 0),
    CONSTRAINT ck_articles_like_count CHECK (like_count >= 0),
    CONSTRAINT ck_articles_comment_count CHECK (comment_count >= 0)
);

CREATE INDEX idx_articles_slug ON articles(slug);
CREATE INDEX idx_articles_status_published_at ON articles(status, published_at DESC);
CREATE INDEX idx_articles_category_id ON articles(category_id);
CREATE INDEX idx_articles_author_id ON articles(author_id);
CREATE INDEX idx_articles_is_top ON articles(is_top DESC, published_at DESC);
CREATE INDEX idx_articles_is_recommend ON articles(is_recommend DESC, published_at DESC);
```

字段说明：

| 字段 | 类型 | 说明 |
|---|---|---|
| title | varchar | 标题 |
| slug | varchar | URL 标识 |
| summary | text | 摘要 |
| content | text | Markdown 正文 |
| cover_image_id | uuid | 封面图 |
| author_id | uuid | 作者 |
| category_id | uuid | 分类 |
| status | varchar | draft / published / archived |
| is_top | boolean | 是否置顶 |
| is_recommend | boolean | 是否推荐 |
| view_count | bigint | 阅读量 |
| like_count | bigint | 点赞数 |
| comment_count | bigint | 评论数 |

---

## 4.7 article_tags

文章标签关联表。

```sql
CREATE TABLE article_tags (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    article_id UUID NOT NULL REFERENCES articles(id) ON DELETE CASCADE,
    tag_id UUID NOT NULL REFERENCES tags(id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_article_tags_article_tag UNIQUE (article_id, tag_id)
);

CREATE INDEX idx_article_tags_tag_id ON article_tags(tag_id);
```

---

## 4.8 comments

评论表。

```sql
CREATE TABLE comments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    article_id UUID NOT NULL REFERENCES articles(id) ON DELETE CASCADE,
    parent_id UUID REFERENCES comments(id) ON DELETE CASCADE,
    nickname VARCHAR(64) NOT NULL,
    email VARCHAR(255),
    website VARCHAR(512),
    content TEXT NOT NULL,
    avatar TEXT,
    ip_hash VARCHAR(128) NOT NULL,
    status VARCHAR(32) NOT NULL DEFAULT 'pending',
    is_spam BOOLEAN NOT NULL DEFAULT false,
    reviewed_by UUID REFERENCES users(id) ON DELETE SET NULL,
    reviewed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at TIMESTAMPTZ NULL,

    CONSTRAINT ck_comments_status CHECK (status IN ('pending', 'approved', 'rejected', 'hidden', 'spam')),
    CONSTRAINT ck_comments_parent CHECK (parent_id IS NULL OR parent_id <> id)
);

CREATE INDEX idx_comments_article_id_status ON comments(article_id, status, created_at DESC);
CREATE INDEX idx_comments_parent_id ON comments(parent_id);
CREATE INDEX idx_comments_ip_hash ON comments(ip_hash);
CREATE INDEX idx_comments_created_at ON comments(created_at DESC);
CREATE INDEX idx_comments_status ON comments(status);
```

---

## 4.9 captcha_sessions

验证码会话表。

```sql
CREATE TABLE captcha_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id VARCHAR(128) NOT NULL,
    answer_hash TEXT NOT NULL,
    ip_hash VARCHAR(128) NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    used_at TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_captcha_sessions_session_id UNIQUE (session_id)
);

CREATE INDEX idx_captcha_sessions_expires_at ON captcha_sessions(expires_at);
CREATE INDEX idx_captcha_sessions_session_id ON captcha_sessions(session_id);
```

---

## 4.10 profile_sections

个人页面模块表。

```sql
CREATE TABLE profile_sections (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    section_type VARCHAR(64) NOT NULL,
    title VARCHAR(128),
    content TEXT,
    sort_order INT NOT NULL DEFAULT 0,
    metadata JSONB NOT NULL DEFAULT '{}'::jsonb,
    is_enabled BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT ck_profile_sections_type CHECK (section_type IN ('hero', 'intro', 'skills', 'experience', 'education', 'contact', 'social', 'custom'))
);

CREATE INDEX idx_profile_sections_type_order ON profile_sections(section_type, sort_order);
```

---

## 4.11 projects

项目展示表。

```sql
CREATE TABLE projects (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL,
    summary TEXT,
    content TEXT,
    cover_image_id UUID REFERENCES media(id) ON DELETE SET NULL,
    tech_stack JSONB NOT NULL DEFAULT '[]'::jsonb,
    project_url VARCHAR(512),
    github_url VARCHAR(512),
    demo_url VARCHAR(512),
    sort_order INT NOT NULL DEFAULT 0,
    is_visible BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at TIMESTAMPTZ NULL,

    CONSTRAINT uq_projects_slug UNIQUE (slug)
);

CREATE INDEX idx_projects_sort_order ON projects(is_visible DESC, sort_order, created_at DESC);
CREATE INDEX idx_projects_slug ON projects(slug);
```

---

## 4.12 friends

友情链接表。

```sql
CREATE TABLE friends (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(128) NOT NULL,
    url TEXT NOT NULL,
    description TEXT,
    logo_image_id UUID REFERENCES media(id) ON DELETE SET NULL,
    sort_order INT NOT NULL DEFAULT 0,
    is_visible BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at TIMESTAMPTZ NULL
);

CREATE INDEX idx_friends_sort_order ON friends(is_visible DESC, sort_order);
```

---

## 4.13 seo_settings

SEO 配置表。

```sql
CREATE TABLE seo_settings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_key VARCHAR(64) NOT NULL,
    title VARCHAR(255),
    description TEXT,
    keywords TEXT,
    og_image_id UUID REFERENCES media(id) ON DELETE SET NULL,
    canonical_url TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_seo_settings_page_key UNIQUE (page_key),
    CONSTRAINT ck_seo_settings_page_key CHECK (page_key IN ('global', 'home', 'articles', 'article', 'category', 'tag', 'archive', 'search', 'about', 'project', 'friends', 'message'))
);
```

---

## 4.14 visits

访问统计表。

```sql
CREATE TABLE visits (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    article_id UUID REFERENCES articles(id) ON DELETE CASCADE,
    page_path TEXT,
    ip_hash VARCHAR(128),
    user_agent TEXT,
    referer TEXT,
    visited_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_visits_visited_at ON visits(visited_at DESC);
CREATE INDEX idx_visits_article_id_visited_at ON visits(article_id, visited_at DESC);
CREATE INDEX idx_visits_ip_hash_visited_at ON visits(ip_hash, visited_at DESC);
```

---

## 4.15 likes

点赞表，可选。

```sql
CREATE TABLE likes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    article_id UUID NOT NULL REFERENCES articles(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    ip_hash VARCHAR(128),
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_ip VARCHAR(45),

    CONSTRAINT uq_likes_article_user UNIQUE (article_id, user_id),
    CONSTRAINT uq_likes_article_ip UNIQUE (article_id, ip_hash)
);

CREATE INDEX idx_likes_article_id ON likes(article_id);
CREATE INDEX idx_likes_created_at ON likes(created_at DESC);
```

注意：

- 如果用户未登录，`user_id` 为空，`ip_hash` 必须有值。
- 需要在应用层保证 `user_id` 和 `ip_hash` 至少一个有值。

---

## 4.16 audit_logs

审计日志表。

```sql
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    action VARCHAR(128) NOT NULL,
    resource_type VARCHAR(64),
    resource_id UUID,
    ip VARCHAR(45),
    user_agent TEXT,
    details JSONB NOT NULL DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_audit_logs_created_at ON audit_logs(created_at DESC);
CREATE INDEX idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_action ON audit_logs(action);
```

---

## 4.17 pages

自定义页面表，支持后台创建和管理独立页面。

```sql
CREATE TABLE pages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL,
    content TEXT,
    summary TEXT,
    template VARCHAR(64) NOT NULL DEFAULT 'default',
    cover_image_id UUID REFERENCES media(id) ON DELETE SET NULL,
    author_id UUID NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    status VARCHAR(32) NOT NULL DEFAULT 'draft',
    is_published BOOLEAN NOT NULL DEFAULT false,
    view_count BIGINT NOT NULL DEFAULT 0,
    seo_title VARCHAR(255),
    seo_description TEXT,
    seo_keywords TEXT,
    published_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at TIMESTAMPTZ NULL,

    CONSTRAINT uq_pages_slug UNIQUE (slug),
    CONSTRAINT ck_pages_status CHECK (status IN ('draft', 'published', 'archived')),
    CONSTRAINT ck_pages_view_count CHECK (view_count >= 0)
);

CREATE INDEX idx_pages_slug ON pages(slug);
CREATE INDEX idx_pages_status_published_at ON pages(status, published_at DESC);
CREATE INDEX idx_pages_author_id ON pages(author_id);
```

---

## 4.18 navigation_menus

导航菜单项表，支持后台管理菜单树。

```sql
CREATE TABLE navigation_menus (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    label VARCHAR(128) NOT NULL,
    url TEXT NOT NULL,
    target VARCHAR(16) NOT NULL DEFAULT '_self',
    icon VARCHAR(64),
    parent_id UUID REFERENCES navigation_menus(id) ON DELETE CASCADE,
    sort_order INT NOT NULL DEFAULT 0,
    is_visible BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT ck_navigation_menus_target CHECK (target IN ('_self', '_blank'))
);

CREATE INDEX idx_navigation_menus_parent_id ON navigation_menus(parent_id);
CREATE INDEX idx_navigation_menus_sort_order ON navigation_menus(is_visible, sort_order);
```

---

## 4.19 footer_sections

页脚区块表。

```sql
CREATE TABLE footer_sections (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    section_key VARCHAR(64) NOT NULL,
    title VARCHAR(128),
    content TEXT,
    sort_order INT NOT NULL DEFAULT 0,
    is_enabled BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_footer_sections_key UNIQUE (section_key),
    CONSTRAINT ck_footer_sections_key CHECK (section_key IN ('copyright', 'icp', 'public_security', 'friends', 'contact', 'social', 'rss', 'sitemap', 'privacy', 'disclaimer', 'custom'))
);

CREATE INDEX idx_footer_sections_order ON footer_sections(is_enabled, sort_order);
```

---

## 4.20 site_settings

站点全局配置表，键值对存储。

```sql
CREATE TABLE site_settings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    setting_key VARCHAR(128) NOT NULL,
    setting_value JSONB NOT NULL DEFAULT '{}'::jsonb,
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_site_settings_key UNIQUE (setting_key)
);

CREATE INDEX idx_site_settings_key ON site_settings(setting_key);
```

站点配置覆盖以下内容：

- `primary_color` - 主色调十六进制值。
- `home_layout` - 首页模块布局配置。
- `article_layout` - 文章列表页布局配置。
- `page_layout` - 自定义页面默认布局。
- `visitor_retention_days` - 访问记录保留天数，默认 30。
- `site_name` - 站点名称。
- `site_description` - 站点描述。
- `site_logo` - 站点 Logo。
- `scheduled_cleanup_enabled` - 是否启用自动清理。

---

## 4.21 page_views

页面阅读数聚合表（可选，如果不需要实时统计，可以直接使用各业务表自带的 view_count 字段）。

```sql
CREATE TABLE page_views (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_type VARCHAR(32) NOT NULL,
    page_id UUID,
    page_path TEXT NOT NULL,
    view_count BIGINT NOT NULL DEFAULT 1,
    view_date DATE NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT ck_page_views_type CHECK (page_type IN ('article', 'page', 'category', 'tag', 'project', 'home', 'about', 'friends', 'message'))
);

CREATE INDEX idx_page_views_date ON page_views(view_date DESC);
CREATE INDEX idx_page_views_path ON page_views(page_type, page_id);
CREATE INDEX idx_page_views_type_date ON page_views(page_type, view_date DESC);
```

---

## 4.22 home_layout_modules

首页与内页模块布局配置表，支持按页面类型配置模块列表。

```sql
CREATE TABLE home_layout_modules (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_type VARCHAR(32) NOT NULL,
    module_key VARCHAR(64) NOT NULL,
    title VARCHAR(128),
    data_source JSONB NOT NULL DEFAULT '{}'::jsonb,
    display_options JSONB NOT NULL DEFAULT '{}'::jsonb,
    sort_order INT NOT NULL DEFAULT 0,
    is_enabled BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT ck_layout_page_type CHECK (page_type IN ('home', 'articles', 'article_detail', 'category', 'tag', 'projects', 'friends', 'about', 'message', 'custom_page'))
);

CREATE INDEX idx_layout_page_type_order ON home_layout_modules(page_type, sort_order);
```

每个 `module_key` 对应前台的一个模块组件，例如：

| module_key | 说明 | 典型 page_type |
|---|---|---|
| hero | 首屏大图区域 | home |
| featured_articles | 精选文章 | home |
| latest_articles | 最新文章 | home, articles |
| category_grid | 分类网格 | home |
| tag_cloud | 标签云 | home |
| project_showcase | 项目展示 | home |
| about_brief | 个人简介 | home |
| friend_links | 友情链接 | home, friends |
| article_list | 文章列表 | articles, category, tag |
| article_detail | 文章正文 | article_detail |
| sidebar | 侧边栏 | articles, article_detail |
| related_posts | 相关文章 | article_detail |
| comment_area | 评论区 | article_detail, custom_page |
| share_bar | 分享栏 | article_detail, custom_page |
| custom_content | 自定义 HTML/内容 | 任意 |

### 5.1 文章查询

常用查询：

```sql
WHERE status = 'published'
ORDER BY published_at DESC
LIMIT 20 OFFSET 0;
```

已有索引：

```text
idx_articles_status_published_at
```

---

### 5.2 分类文章

```sql
WHERE category_id = ?
  AND status = 'published'
ORDER BY published_at DESC;
```

已有索引：

```text
idx_articles_category_id
idx_articles_status_published_at
```

后续可增强：

```sql
CREATE INDEX idx_articles_category_status_published
ON articles(category_id, status, published_at DESC);
```

---

### 5.3 标签文章

```sql
SELECT a.*
FROM articles a
JOIN article_tags at ON a.id = at.article_id
WHERE at.tag_id = ?
  AND a.status = 'published'
ORDER BY a.published_at DESC;
```

已有索引：

```text
idx_article_tags_tag_id
idx_articles_status_published_at
```

---

### 5.4 评论查询

```sql
WHERE article_id = ?
  AND status = 'approved'
ORDER BY created_at DESC;
```

已有索引：

```text
idx_comments_article_id_status
```

---

### 5.5 评论审核

```sql
WHERE status = 'pending'
ORDER BY created_at DESC;
```

已有索引：

```text
idx_comments_status
idx_comments_created_at
```

---

## 6. PostgreSQL 扩展建议

### 6.1 pgcrypto

用于生成 UUID：

```sql
CREATE EXTENSION IF NOT EXISTS pgcrypto;
```

---

### 6.2 pg_trgm

用于模糊搜索：

```sql
CREATE EXTENSION IF NOT EXISTS pg_trgm;
```

可为标题建立索引：

```sql
CREATE INDEX idx_articles_title_trgm ON articles USING gin (title gin_trgm_ops);
```

---

### 6.3 全文搜索

后续可使用：

```sql
ALTER TABLE articles ADD COLUMN search_vector tsvector;
```

---

## 7. 软删除策略

以下表建议支持软删除：

- users
- media
- categories
- tags
- articles
- comments
- projects
- friends

软删除后：

- 前台查询必须排除 `deleted_at IS NULL`。
- 后台可恢复或彻底删除。
- 彻底删除需要管理员权限。

---

## 8. 数据一致性规则

### 8.1 文章状态

- `draft`：草稿，前台不可见。
- `published`：已发布，前台可见。
- `archived`：归档，前台不可见或进入归档页。

---

### 8.2 评论状态

- `pending`：待审核。
- `approved`：已通过。
- `rejected`：已拒绝。
- `hidden`：已隐藏。
- `spam`：垃圾评论。

---

### 8.3 用户角色

- `super_admin`：超级管理员。
- `admin`：管理员。
- `content_admin`：内容管理员。
- `comment_moderator`：评论审核员。

---

## 9. 初始化数据

### 9.1 初始化超级管理员

注意：密码必须由后端使用安全哈希生成，不建议在 SQL 中写明文密码。

```sql
INSERT INTO users (username, email, password_hash, role, status)
VALUES (
    'admin',
    'admin@example.com',
    'REPLACE_WITH_SECURE_PASSWORD_HASH',
    'super_admin',
    'active'
);
```

---

### 9.2 初始化全局 SEO

```sql
INSERT INTO seo_settings (page_key, title, description, keywords)
VALUES (
    'global',
    '个人博客',
    '一个现代化个人博客与个人品牌展示平台。',
    '博客,个人品牌,技术文章'
)
ON CONFLICT (page_key) DO NOTHING;
```

---

## 10. EF Core 映射建议

### 10.1 全局配置

```csharp
modelBuilder.Entity<BaseEntity>().Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
modelBuilder.Entity<BaseEntity>().Property(e => e.CreatedAt).HasDefaultValueSql("now()");
modelBuilder.Entity<BaseEntity>().Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
```

---

### 10.2 JSONB 字段

使用 `Jsonb` 类型：

```csharp
builder.Property(x => x.Metadata)
    .HasColumnType("jsonb")
    .HasDefaultValueSql("'{}'::jsonb");
```

---

### 10.3 UUID 类型

```csharp
builder.Property(x => x.Id).HasColumnType("uuid");
```

---

## 11. 数据库验收标准

- 所有表可成功创建。
- 所有外键关系正确。
- 所有唯一约束生效。
- 所有状态字段有 CHECK 约束。
- 所有高频查询有索引。
- 文章发布后前台可查询。
- 评论审核状态可正确筛选。
- 媒体资源可被文章引用。
- 软删除查询不会返回已删除数据。
- EF Core Migration 可成功生成和应用。
