<div align="center">

**English** | [中文](README.md)

</div>

---

<div align="center">

# MBlog 📝

**A modern, SEO-friendly, full-featured personal blog and portfolio platform with a headless CMS backend**

[![Docker](https://img.shields.io/badge/deploy-Docker%20Compose-2496ED?logo=docker&logoColor=white)](https://docs.docker.com/compose/)
[![Next.js](https://img.shields.io/badge/frontend-Next.js%2016-000000?logo=next.js)](https://nextjs.org/)
[![Vue 3](https://img.shields.io/badge/admin-Vue%203%20%2F%20Element%20Plus-4FC08D?logo=vue.js)](https://vuejs.org/)
[![ASP.NET](https://img.shields.io/badge/backend-ASP.NET%20Core-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/database-PostgreSQL-4169E1?logo=postgresql)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

</div>

---

## 📖 Overview

MBlog is a **production-ready open-source personal blog platform** built with a three-tier architecture. It combines a **Next.js** SSR/SSG frontend for visitors, a **Vue 3 + Element Plus** rich admin panel for content management, and a **C# ASP.NET Core Web API** backend. It is designed for developers and content creators who want a self-hosted, scalable, and beautiful personal website with full control over their content and data.

---

## ✨ Features

### 🌐 Frontend (Visitor-Facing)

| Feature | Description |
|---|---|
| **Homepage Module Engine** | Modular homepage with configurable blocks (hero, latest articles, projects, categories, tags, subscription, statistics) — order and visibility managed from admin |
| **Article List & Detail** | Paginated article list, rich detail page with Markdown rendering, syntax highlighting, TOC, reading progress bar, prev/next navigation, related articles |
| **Category & Tag Pages** | Browse articles by category tree or tag cloud |
| **Date Archive** | Timeline archive grouped by year/month |
| **Full-Text Search** | Real-time article search with result stats |
| **Project Portfolio** | Showcase projects with covers, tech stacks, and Markdown descriptions |
| **About / Profile Page** | Modular personal page (intro, skills, experience, education, projects) |
| **Guestbook / Messages** | Visitor message board with CAPTCHA and admin moderation |
| **Friend Links** | Friendly link exchange display |
| **Custom Pages** | Publish standalone pages (Privacy, Terms, etc.) from admin |
| **SEO Optimized** | Independent title, description, keywords, Open Graph, Twitter Card, JSON-LD, canonical URL per page. Built-in sitemap.xml & robots.txt |
| **Responsive Design** | Adaptive layouts for desktop, tablet, and mobile. Dark mode support |
| **Dynamic Navigation** | Multi-level menu from admin, supports open-in-new-tab |
| **Dynamic Footer** | Configurable footer blocks (copyright, ICP/PSB备案, social links, etc.) |
| **Comment System** | CAPTCHA, rate limiting, spam filter, admin review required |
| **Visit Tracking** | Privacy-friendly view counting per page |

### 🛠 Admin Panel (CMS)

| Feature | Description |
|---|---|
| **Dashboard** | Overview statistics: article/project/comment counts, visit trends |
| **Article Management** | Full CRUD with Markdown editor, draft/publish/schedule, cover image, categories, tags, custom slug, SEO fields |
| **Markdown Editor** | Real-time preview, image/video/attachment upload, media library integration, 7 AI tools built into toolbar |
| **AI Writing Assistant** | One-click article generation, content polishing with optional image insertion, SEO extraction, cover generation via LLM |
| **AI Batch Fill** | Batch process selected articles for SEO, summaries, and covers with progress dialog and auto-retry (3 attempts) |
| **AI Chat** | Conversational AI assistant in admin panel |
| **AI Image Generation** | Text-to-image generation with configurable models |
| **Category & Tag Management** | CRUD with multi-level category support (parent/child) |
| **Media Library** | Image/video/attachment upload, preview, search, categorization |
| **Comment Moderation** | Approve, reject, mark spam, batch operations, IP/location info |
| **Guestbook Management** | Message list, reply, delete, moderation |
| **Custom Pages** | Create, edit, publish/draft standalone pages with SEO fields |
| **Profile Page Config** | Manage about page sections (intro, skills, experience, etc.) |
| **Project Management** | CRUD for portfolio projects with cover, tech stack, Markdown content |
| **Friend Links** | Manage friendly links with logo, description, sort order |
| **Navigation Management** | Tree editor with drag-and-drop sorting (SortableJS), multi-level, open-in-new-tab |
| **Footer Management** | Enable/disable, edit, reorder footer blocks |
| **SEO Configuration** | Global and per-page SEO settings, sitemap generation |
| **Theme Settings** | Customize primary/surface/text colors, dark mode colors, site name, logo, favicon, social links |
| **Layout Management** | Drag-and-drop module ordering per page (home, articles, detail, archive, etc.) |
| **Site Settings** | Site name, description, ICP/PSB备案号, comment auto-approve, subscription toggle, visit retention days |
| **Visitor Logs** | Paged visit records with IP, location, browser, page, time; cleanup policy |
| **Statistics** | Charts and trend data by day/week/month |
| **Email System** | SMTP settings, HTML email template editor, send logs, subscriber management, weekly digest |
| **Subscription Management** | Email subscriber list with status, unsubscribe |
| **LLM Settings** | Configurable LLM provider (Tencent, OpenAI-compatible), model, temperature, max tokens, timeouts |
| **ImageGen Settings** | Configurable text-to-image provider |
| **Change Password** | Authenticated password update |
| **Role-Based Access** | JWT + Refresh Token authentication on all admin routes |

### ⚙️ Backend API (30+ Controllers)

| Module | Controllers |
|---|---|
| **Authentication** | `AuthController` — Login, refresh token, logout, current user |
| **Articles** | `ArticleController`, `PublicArticleController` — Full CRUD + public listing, detail, prev/next, related, view tracking |
| **Categories** | `CategoryController`, `PublicCategoryController` — CRUD, tree, public list |
| **Tags** | `TagController`, `PublicTagController` — CRUD, public list |
| **Comments** | `CommentController`, `AdminCommentController` — Submit, list, approve, reject, spam, batch |
| **Messages** | `MessageController`, `AdminMessageController` — Submit, list, reply, delete |
| **Media / Uploads** | `UploadController` — Image/video/attachment upload with validation |
| **Custom Pages** | `PageController`, `PublicPageController` — CRUD, publish, public access |
| **Profile** | `ProfileController`, `PublicProfileController` — About page sections |
| **Projects** | `ProjectController`, `PublicProjectController` — CRUD, public listing + detail |
| **Navigation** | `NavigationController`, `PublicNavigationController` — Tree CRUD, public by location |
| **Footer** | `FooterController`, `PublicFooterController` — Config, public access |
| **SEO** | `SeoController`, `AdminSeoController`, `SitemapController` — Settings, sitemap.xml, robots.txt |
| **Site Settings** | `SiteSettingController` — System config CRUD |
| **Theme** | `ThemeController` — Theme settings CRUD |
| **Layout** | `LayoutController`, `PublicLayoutController` — Module layout CRUD |
| **Dashboard** | `DashboardController` — Aggregated stats |
| **Visits & Stats** | `VisitController`, `PublicVisitController` — Track, count, paged records, stats, cleanup |
| **Archive** | `PublicArchiveController` — Year/month timeline |
| **Friends** | `FriendController`, `PublicFriendController` — CRUD, public list |
| **Captcha** | `CaptchaController` — CAPTCHA generation & verification (image + slider) |
| **Login Attempt** | `LoginAttemptService` — IP-based failure counting & lockout |
| **IP Blacklist** | `IpBanMiddleware` — In-memory frequency tracking, threshold-based ban |
| **LLM / AI** | `LlmController` — Model config, AI article/SEO/cover/polish |
| **Image Generation** | `ImageGenController` — Text-to-image config & generation |
| **Email** | `EmailSettingController`, `EmailLogController` — SMTP config, templates, send logs |
| **Subscription** | `SubscriptionController` — Subscribe, unsubscribe, list, weekly digest trigger |

### 🤖 AI Integration

| Feature | Description |
|---|---|
| **AI Write** | Generate full article from a title/topic via LLM |
| **AI Polish** | Rewrite and enhance existing content with optional image insertion (preserves original images) |
| **AI SEO** | Automatically extract keywords and description from article content |
| **AI Cover** | Generate cover image from article content via text-to-image API |
| **AI Batch Fill** | Batch process selected articles: SEO extraction → cover generation → update, with progress dialog & auto-retry (3 attempts) |
| **AI Chat** | Conversational AI assistant in admin panel |
| **AI Image Gen** | Standalone text-to-image page in admin panel |
| **Provider Support** | Configurable Tencent LLM, OpenAI-compatible APIs; separate config for LLM and ImageGen |

### 🛡️ Security

| Feature | Description |
|---|---|
| **Password Hashing** | Strong hash algorithms for user credentials |
| **JWT Authentication** | Short-lived access tokens + revocable refresh tokens stored in database |
| **Role Authorization** | Admin routes require authentication; super_admin/admin roles |
| **Input Validation** | FluentValidation for all external inputs |
| **SQL Injection Prevention** | Parameterized queries via EF Core |
| **Comment Anti-Spam** | CAPTCHA, IP rate limiting, content filtering, link restrictions, duplicate detection |
| **Hotlinking Protection** | Nginx valid_referers + warning SVG for uploaded media |
| **Upload Security** | MIME validation, script execution disabled on upload directory |
| **Audit Logging** | Serilog audit trail for sensitive operations |
| **IP Privacy** | IP hashing for visitor storage |
| **CORS** | Configurable cross-origin policies |
| **Rate Limiting** | Built-in rate limiter for API abuse prevention |
| **Login CAPTCHA** | Slider CAPTCHA after first failed login attempt, 5 failures locks IP for 15 minutes, configurable toggle |
| **IP Blacklist** | In-memory IP frequency tracking, auto-ban on threshold exceed, API-only interception, skips static assets and high-frequency endpoints, configurable threshold/window/duration/toggle, full-screen overlay with countdown on 429 |

### 🔧 Performance & SEO

| Feature | Description |
|---|---|
| **SSR / SSG** | Next.js server-side rendering & static generation |
| **Per-Page SEO** | Independent title, description, keywords for every page |
| **Open Graph** | og:title, og:description, og:image, og:type for social sharing |
| **Twitter Card** | Twitter card meta tags |
| **JSON-LD** | Structured data for articles and pages |
| **Sitemap** | Auto-generated sitemap.xml with static + dynamic routes |
| **Robots.txt** | Configurable robots.txt |
| **Image Lazy Loading** | Native lazy loading for images |
| **Video On-Demand** | Video loads only when visible |
| **Cache Headers** | Immutable asset caching, response caching |
| **Old URL Redirect** | `/index.php/archives/:id/` → `/articles/show-:id` redirect support |

### 📦 Deployment & DevOps

| Feature | Description |
|---|---|
| **Docker Compose** | Multi-service orchestration (Nginx, frontend, admin, API, PostgreSQL) |
| **Nginx Reverse Proxy** | SSL termination, load balancing, anti-hotlinking, client size limits (210MB) |
| **Health Checks** | Readiness/liveness probes for all services |
| **Database Backups** | Automated backup script with rotation (daily 7d, weekly 4w, monthly 12m) |
| **Environment Config** | `.env.example` templates with all configurable variables |
| **Containerized Build** | Multi-stage Dockerfiles for optimized image sizes |

---

## 🚀 Quick Start

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) & [Docker Compose](https://docs.docker.com/compose/install/)

### One-Click Deployment

```bash
# Clone the repository
git clone https://github.com/mbaisha/m-blog.git
cd m-blog

# Copy and configure environment
cp .env.example .env
# Edit .env with your settings (database passwords, JWT secrets, AI API keys, etc.)

# Start all services
docker compose up -d

# Services are now available at:
# Frontend:  http://localhost:3000      (Visitor blog)
# Admin:     http://localhost:3001/admin (Admin panel)
# API:       http://localhost:5000/api   (Backend API)
```

See [deploy/.env.example](deploy/.env.example) for all configurable environment variables.

> **Default admin credentials**: Check the database initialization script in `backend/DbInitializer.cs`.

---

## 🛠 Tech Stack

| Layer | Technology | Purpose |
|---|---|---|
| **Frontend** | [Next.js 16](https://nextjs.org/) + TypeScript + [Tailwind CSS 4](https://tailwindcss.com/) | SSR/SSG blog with SEO optimization |
| **Admin Panel** | [Vue 3](https://vuejs.org/) + TypeScript + [Element Plus](https://element-plus.org/) + [Vite](https://vitejs.dev/) | Full-featured CMS interface |
| **Backend API** | [C#](https://dotnet.microsoft.com/) + [ASP.NET Core Web API](https://dotnet.microsoft.com/en-us/apps/aspnet/apis) | RESTful API layer |
| **ORM** | [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) | Data access & migrations (15+ migrations) |
| **Database** | [PostgreSQL](https://www.postgresql.org/) | Primary data store (UUID, timestamptz, jsonb) |
| **Authentication** | JWT + Refresh Token | Stateless API auth |
| **Validation** | [FluentValidation](https://docs.fluentvalidation.net/) | Request validation |
| **Logging** | [Serilog](https://serilog.net/) | Structured logging & audit trail |
| **Charts** | [ECharts](https://echarts.apache.org/) | Dashboard statistics visualization |
| **Deployment** | [Docker Compose](https://docs.docker.com/compose/) | Multi-service orchestration |
| **Reverse Proxy** | [Nginx](https://nginx.org/) | Load balancing, SSL, anti-hotlinking |
| **Editor** | [md-editor-v3](https://imzbf.github.io/md-editor-v3/) | Markdown editing with custom AI toolbar |
| **Drag & Drop** | [SortableJS](https://sortablejs.org/) | Navigation & layout reordering |

---

## 📁 Project Structure

```
m-blog/
├── frontend/                      # Next.js blog (SSR/SSG)
│   ├── src/
│   │   ├── app/                   # App Router pages
│   │   │   ├── page.tsx           # Home
│   │   │   ├── articles/          # Article list + detail
│   │   │   ├── categories/        # Category pages
│   │   │   ├── tags/              # Tag pages
│   │   │   ├── archive/           # Date archive
│   │   │   ├── search/            # Full-text search
│   │   │   ├── projects/          # Project portfolio
│   │   │   ├── about/             # About/profile page
│   │   │   ├── friends/           # Friend links
│   │   │   ├── guestbook/         # Message board
│   │   │   ├── [slug]/            # Custom pages
│   │   │   ├── sitemap.ts         # Dynamic sitemap
│   │   │   └── layout.tsx         # Root layout with navbar/footer
│   │   ├── components/            # Reusable UI components (IpBanOverlay, etc.)
│   │   └── lib/                   # API client & utilities
│   ├── scripts/                   # Utility scripts (generate-icons.js)
│   └── Dockerfile
│
├── admin/                         # Vue 3 admin panel
│   ├── src/
│   │   ├── views/                 # 33 page views
│   │   ├── components/            # Reusable components (MarkdownEditor, etc.)
│   │   ├── api/                   # API client modules
│   │   ├── stores/                # Pinia stores (auth, etc.)
│   │   └── router/                # Route definitions
│   └── Dockerfile
│
├── backend/                       # ASP.NET Core Web API
│   ├── Mblog.API/
│   │   ├── Controllers/           # 30+ API controllers
│   │   ├── Common/                # ApiResponse, ExceptionMiddleware, etc.
│   │   ├── Models/
│   │   │   ├── Entities/          # 30+ entity classes
│   │   │   └── DTOs/              # Request/Response DTOs
│   │   ├── Services/              # 27+ application services
│   │   ├── Data/                  # DbContext, EF Core migrations
│   │   ├── Validators/            # FluentValidation validators
│   │   └── Program.cs             # Startup & DI configuration
│   └── Dockerfile
│
├── deploy/                        # Deployment configurations
│   ├── nginx.conf                 # Nginx reverse proxy config
│   ├── .env.example               # Production environment template
│   ├── hotlink-warning.svg        # Anti-hotlinking warning image
│   └── backup.sh                  # Database backup script
│
├── docs/                          # Documentation
│   └── blog-platform/             # 14 detailed design documents
│
├── docker-compose.yml             # Production orchestration
├── .env.example                   # Root environment template
└── README.md
```

---

## 🗄️ Database Entities (30+)

```
User, RefreshToken, AuditLog
Article, ArticleCategory, ArticleTag
Category (self-referencing parent/child), Tag
Comment, Message, CaptchaSession
Media (images/videos/attachments)
Page, ProfileSection
Project, Friend, NavigationItem
FooterConfig, SiteSetting, SeoSetting
ThemeSetting, ModuleLayout
Visit, Like
LlmConfig, ImageGenConfig
Subscriber, EmailSetting, EmailTemplate, EmailLog
```

---

## 🧩 Key Module Details

### 🎨 Theme System
- Customizable primary color, surface color, text colors
- Dark mode color configuration (independent from light mode)
- Site name, logo, favicon, social links (GitHub, Twitter, WeChat, etc.)
- ICP record number, PSB security record number

### 📧 Email & Subscription
- SMTP settings with SSL/TLS support
- HTML email template editor with variables
- Email send logs with status tracking
- Newsletter subscription form on homepage
- Weekly digest scheduled job

### 📐 Layout Engine
- Per-page module configuration (home, articles list, article detail, categories, tags, archive, search, projects, project detail)
- Drag-and-drop module ordering
- Module-specific config fields (count, columns, display style, etc.)
- Configurable visibility per module

### 🧹 Scheduled Jobs
- **Visit Cleanup Job**: Auto-cleanup visit records older than configured retention days
- **Weekly Digest Job**: Generate and send weekly email digests to subscribers

---

## 📊 Architecture Overview

```
                    ┌──────────────┐
                    │   Browser    │
                    └──────┬───────┘
                           │
                    ┌──────▼───────┐
                    │    Nginx     │  Reverse proxy, SSL, anti-hotlinking
                    └──┬───────┬───┘
                       │       │
              ┌────────▼─┐  ┌──▼──────────┐
              │  Next.js  │  │  Vue Admin  │
              │  Frontend │  │  Panel      │
              │  :3000    │  │  :3001      │
              └────────┬──┘  └──┬──────────┘
                       │        │
              ┌────────▼────────▼──┐
              │  ASP.NET Core API  │
              │  :5000             │
              └────────┬───────────┘
                       │
              ┌────────▼───────────┐
              │    PostgreSQL      │
              │    :5432           │
              └────────────────────┘
```

---

## 🔧 Configuration

### Environment Variables (Docker Compose)

| Variable | Description |
|---|---|
| `DB_HOST` | PostgreSQL host address |
| `DB_PORT` | PostgreSQL port (default 5432) |
| `DB_NAME` | PostgreSQL database name |
| `DB_USER` | PostgreSQL user |
| `DB_PASSWORD` | PostgreSQL password |
| `JWT_KEY` | JWT signing key (min 32 characters) |
| `SITE_URL` | Site access URL (e.g. `http://blog.example.com`) |
| `SITE_NAME` | Site name |
| `API_BASE_URL` | API base URL (e.g. `http://localhost:5000/api`) |

### Environment Variables (Backend Direct Config, use `__` instead of `:`)

| Variable | Config Path | Description |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | `ConnectionStrings:DefaultConnection` | PostgreSQL connection string (overrides DB_HOST etc.) |
| `Jwt__Key` | `Jwt:Key` | JWT signing key |
| `Jwt__Issuer` | `Jwt:Issuer` | JWT issuer |
| `Jwt__Audience` | `Jwt:Audience` | JWT audience |
| `Jwt__AccessTokenExpirationMinutes` | `Jwt:AccessTokenExpirationMinutes` | Access token expiration (minutes, default 120) |
| `Jwt__RefreshTokenExpirationDays` | `Jwt:RefreshTokenExpirationDays` | Refresh token expiration (days, default 7) |
| `Storage__RootPath` | `Storage:RootPath` | Upload file storage path (default `./uploads`) |
| `Cors__AllowedOrigins` | `Cors:AllowedOrigins` | Allowed CORS origins (comma-separated) |

### Database Entity Config (Managed via Admin Panel)

The following configurations are **not environment variables** — they are stored in the database and managed through the admin panel:

- **LLM Settings** — Configure provider, model, API key in "LLM Settings" page
- **Image Generation Settings** — Configure in "ImageGen Settings" page
- **Email (SMTP) Settings** — Configure server, port, credentials in "Email Settings" page
- **Site Settings** — Configure site name, ICP/PSB备案, comment policy in "Site Settings" page
- **Theme Settings** — Customize colors, logo, etc. in "Theme Settings" page

See [backend/.env.example](backend/.env.example), [deploy/.env.example](deploy/.env.example) and [.env.example](.env.example) for full configuration reference.

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create a feature branch: `git checkout -b feat/your-feature`
3. Commit your changes: `git commit -m 'feat: add some feature'`
4. Push to the branch: `git push origin feat/your-feature`
5. Open a Pull Request

Please follow the existing code style and structure.

---

## 📄 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## 🌟 Support

If you find this project useful, please consider giving it a ⭐ on GitHub!

For issues and feature requests, use the [GitHub Issues](https://github.com/mbaisha/m-blog/issues) page.
