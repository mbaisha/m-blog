using Microsoft.EntityFrameworkCore;
using Mblog.API.Models.Entities;

namespace Mblog.API.Data;

/// <summary>
/// 应用数据库上下文，管理所有实体与数据库的映射
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ========== 数据集 ==========
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleTag> ArticleTags => Set<ArticleTag>();
    public DbSet<ArticleCategory> ArticleCategories => Set<ArticleCategory>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CaptchaSession> CaptchaSessions => Set<CaptchaSession>();
    public DbSet<ProfileSection> ProfileSections => Set<ProfileSection>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Friend> Friends => Set<Friend>();
    public DbSet<SeoSetting> SeoSettings => Set<SeoSetting>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<NavigationItem> NavigationItems => Set<NavigationItem>();
    public DbSet<FooterConfig> FooterConfigs => Set<FooterConfig>();
    public DbSet<ThemeSetting> ThemeSettings => Set<ThemeSetting>();
    public DbSet<ModuleLayout> ModuleLayouts => Set<ModuleLayout>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<Subscriber> Subscribers => Set<Subscriber>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<EmailSetting> EmailSettings => Set<EmailSetting>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<LlmConfig> LlmConfigs => Set<LlmConfig>();
    public DbSet<ImageGenConfig> ImageGenConfigs => Set<ImageGenConfig>();

    /// <summary>
    /// 配置实体与数据库表的映射关系、约束、索引和过滤器
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var isSqlite = Database.ProviderName?.Contains("Sqlite") == true;

        // === 所有实体的公共字段默认值 ===
        if (!isSqlite)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // CreatedAt 默认使用当前时间
                if (entity.FindProperty("CreatedAt") is var createdAt && createdAt != null)
                {
                    createdAt.SetDefaultValueSql("now()");
                }
                // UpdatedAt 默认使用当前时间
                if (entity.FindProperty("UpdatedAt") is var updatedAt && updatedAt != null)
                {
                    updatedAt.SetDefaultValueSql("now()");
                }
            }
        }

        // ==================== User 用户表 ====================
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Username).HasMaxLength(64).IsRequired();
            e.Property(x => x.Email).HasMaxLength(255);
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Role).HasMaxLength(32).IsRequired().HasDefaultValue("admin");
            e.Property(x => x.Status).HasMaxLength(32).IsRequired().HasDefaultValue("active");
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.Username).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasOne(x => x.Avatar).WithMany().HasForeignKey(x => x.AvatarMediaId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.DeletedAt == null); // 默认过滤软删除
        });

        // ==================== RefreshToken 刷新令牌表 ====================
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.TokenHash).IsRequired();
            e.HasIndex(x => x.TokenHash).IsUnique(); // Token哈希唯一
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.ExpiresAt);
            e.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== Media 媒体资源表 ====================
        modelBuilder.Entity<Media>(e =>
        {
            e.ToTable("media");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Type).HasMaxLength(32).IsRequired();
            e.Property(x => x.Url).IsRequired();
            e.Property(x => x.Filename).IsRequired();
            e.Property(x => x.StoragePath).IsRequired();
            e.Property(x => x.MimeType).HasMaxLength(128).IsRequired();
            if (!isSqlite) e.Property(x => x.DurationSeconds).HasColumnType("numeric(10,2)");
            e.HasIndex(x => x.Type);
            e.HasIndex(x => x.CreatedAt).IsDescending();
            e.HasOne(x => x.Creator).WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        // ==================== Category 分类表 ====================
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(128).IsRequired();
            e.Property(x => x.SeoTitle).HasMaxLength(255);
            e.HasIndex(x => x.Slug).IsUnique(); // Slug 唯一
            e.HasIndex(x => new { x.SortOrder, x.CreatedAt }).IsDescending(false, true);
            e.HasOne(x => x.CoverImage).WithMany().HasForeignKey(x => x.CoverImageId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        // ==================== Tag 标签表 ====================
        modelBuilder.Entity<Tag>(e =>
        {
            e.ToTable("tags");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(128).IsRequired();
            e.Property(x => x.Color).HasMaxLength(32);
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasIndex(x => x.Name);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        // ==================== Article 文章表 ====================
        modelBuilder.Entity<Article>(e =>
        {
            e.ToTable("articles");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Title).HasMaxLength(255).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(255).IsRequired();
            e.Property(x => x.Content).IsRequired();
            e.Property(x => x.Status).HasMaxLength(32).IsRequired().HasDefaultValue("draft");
            e.Property(x => x.SeoTitle).HasMaxLength(255);
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasIndex(x => new { x.Status, x.PublishedAt }).IsDescending(false, true); // 按状态和发布时间查询
            e.HasIndex(x => x.CategoryId);
            e.HasIndex(x => x.AuthorId);
            e.HasIndex(x => new { x.IsTop, x.PublishedAt }).IsDescending(true, true);  // 置顶文章排序
            e.HasIndex(x => new { x.IsRecommend, x.PublishedAt }).IsDescending(true, true); // 推荐文章排序
            e.HasOne(x => x.CoverImage).WithMany().HasForeignKey(x => x.CoverImageId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Author).WithMany(x => x.Articles).HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict); // 禁止级联删除作者
            e.HasOne(x => x.Category).WithMany(x => x.Articles).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        // ==================== ArticleTag 文章-标签关联表 ====================
        modelBuilder.Entity<ArticleTag>(e =>
        {
            e.ToTable("article_tags");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasIndex(x => new { x.ArticleId, x.TagId }).IsUnique(); // 同一文章不能重复添加同一标签
            e.HasIndex(x => x.TagId);
            e.HasOne(x => x.Article).WithMany(x => x.ArticleTags).HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Tag).WithMany(x => x.ArticleTags).HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== ArticleCategory 文章-分类关联表 ====================
        modelBuilder.Entity<ArticleCategory>(e =>
        {
            e.ToTable("article_categories");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasIndex(x => new { x.ArticleId, x.CategoryId }).IsUnique();
            e.HasIndex(x => x.CategoryId);
            e.HasOne(x => x.Article).WithMany(x => x.ArticleCategories).HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Category).WithMany(x => x.ArticleCategories).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== Comment 评论表 ====================
        modelBuilder.Entity<Comment>(e =>
        {
            e.ToTable("comments");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Nickname).HasMaxLength(64).IsRequired();
            e.Property(x => x.Email).HasMaxLength(255);
            e.Property(x => x.Website).HasMaxLength(512);
            e.Property(x => x.Content).IsRequired();
            e.Property(x => x.IpHash).HasMaxLength(128).IsRequired();
            e.Property(x => x.Status).HasMaxLength(32).IsRequired().HasDefaultValue("pending");
            e.HasIndex(x => new { x.ArticleId, x.Status, x.CreatedAt }).IsDescending(false, false, true); // 按文章+状态查询评论
            e.HasIndex(x => x.ParentId);
            e.HasIndex(x => x.IpHash);
            e.HasIndex(x => x.CreatedAt).IsDescending();
            e.HasIndex(x => x.Status);
            e.HasOne(x => x.Article).WithMany(x => x.Comments).HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Cascade); // 级联删除子评论
            e.HasOne(x => x.Reviewer).WithMany().HasForeignKey(x => x.ReviewedBy).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        // ==================== CaptchaSession 验证码会话表 ====================
        modelBuilder.Entity<CaptchaSession>(e =>
        {
            e.ToTable("captcha_sessions");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.SessionId).HasMaxLength(128).IsRequired();
            e.Property(x => x.AnswerHash).IsRequired();
            e.Property(x => x.IpHash).HasMaxLength(128).IsRequired();
            e.HasIndex(x => x.SessionId).IsUnique();
            e.HasIndex(x => x.ExpiresAt);
        });

        // ==================== ProfileSection 个人页面模块表 ====================
        modelBuilder.Entity<ProfileSection>(e =>
        {
            e.ToTable("profile_sections");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.SectionType).HasMaxLength(64).IsRequired();
            e.Property(x => x.Title).HasMaxLength(128);
            e.Property(x => x.Metadata).HasColumnType(isSqlite ? "TEXT" : "jsonb").HasDefaultValueSql(isSqlite ? "'{}'" : "'{}'::jsonb");
            e.HasIndex(x => new { x.SectionType, x.SortOrder });
        });

        // ==================== Project 项目展示表 ====================
        modelBuilder.Entity<Project>(e =>
        {
            e.ToTable("projects");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Title).HasMaxLength(255).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(255).IsRequired();
            if (!isSqlite) e.Property(x => x.TechStack).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
            else e.Property(x => x.TechStack).HasColumnType("TEXT").HasDefaultValueSql("'[]'");
            e.Property(x => x.ProjectUrl).HasMaxLength(512);
            e.Property(x => x.GithubUrl).HasMaxLength(512);
            e.Property(x => x.DemoUrl).HasMaxLength(512);
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasIndex(x => new { x.IsVisible, x.SortOrder, x.CreatedAt }).IsDescending(true, false, true);
            e.HasOne(x => x.CoverImage).WithMany().HasForeignKey(x => x.CoverImageId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        // ==================== Friend 友情链接表 ====================
        modelBuilder.Entity<Friend>(e =>
        {
            e.ToTable("friends");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.Url).IsRequired();
            e.HasIndex(x => new { x.IsVisible, x.SortOrder });
            e.HasOne(x => x.LogoImage).WithMany().HasForeignKey(x => x.LogoImageId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        // ==================== SeoSetting SEO 配置表 ====================
        modelBuilder.Entity<SeoSetting>(e =>
        {
            e.ToTable("seo_settings");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.PageKey).HasMaxLength(64).IsRequired();
            e.Property(x => x.Title).HasMaxLength(255);
            e.HasIndex(x => x.PageKey).IsUnique(); // 每个页面只有一个 SEO 配置
            e.HasOne(x => x.OgImage).WithMany().HasForeignKey(x => x.OgImageId).OnDelete(DeleteBehavior.SetNull);
        });

        // ==================== Visit 访问记录表 ====================
        modelBuilder.Entity<Visit>(e =>
        {
            e.ToTable("visits");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.IpHash).HasMaxLength(128);
            e.HasIndex(x => x.VisitedAt).IsDescending();
            e.HasIndex(x => new { x.ArticleId, x.VisitedAt }).IsDescending(false, true);
            e.HasIndex(x => new { x.IpHash, x.VisitedAt }).IsDescending(false, true);
            e.HasOne(x => x.Article).WithMany().HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== Like 点赞记录表 ====================
        modelBuilder.Entity<Like>(e =>
        {
            e.ToTable("likes");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CreatedIp).HasMaxLength(45);
            e.HasIndex(x => new { x.ArticleId, x.UserId }).IsUnique(); // 同一用户一篇文章只能点一个赞
            e.HasIndex(x => new { x.ArticleId, x.IpHash }).IsUnique(); // 同一 IP 一篇文章只记录一次
            e.HasIndex(x => x.ArticleId);
            e.HasIndex(x => x.CreatedAt).IsDescending();
            e.HasOne(x => x.Article).WithMany().HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        // ==================== AuditLog 审计日志表 ====================
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("audit_logs");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Action).HasMaxLength(128).IsRequired();
            e.Property(x => x.ResourceType).HasMaxLength(64);
            e.Property(x => x.Ip).HasMaxLength(45);
            e.Property(x => x.Details).HasColumnType(isSqlite ? "TEXT" : "jsonb").HasDefaultValueSql(isSqlite ? "'{}'" : "'{}'::jsonb");
            e.HasIndex(x => x.CreatedAt).IsDescending();
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.Action);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        // ==================== SiteSetting 站点设置表 ====================
        modelBuilder.Entity<SiteSetting>(e =>
        {
            e.ToTable("site_settings");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.SiteName).HasMaxLength(128).IsRequired().HasDefaultValue("My Blog");
            e.Property(x => x.SiteDescription).HasMaxLength(512);
            e.HasOne(x => x.LogoImage).WithMany().HasForeignKey(x => x.LogoImageId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.FaviconImage).WithMany().HasForeignKey(x => x.FaviconImageId).OnDelete(DeleteBehavior.SetNull);
        });

        // ==================== Page 自定义页面表 ====================
        modelBuilder.Entity<Page>(e =>
        {
            e.ToTable("pages");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Title).HasMaxLength(255).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(255).IsRequired();
            e.Property(x => x.Status).HasMaxLength(32).IsRequired().HasDefaultValue("draft");
            e.Property(x => x.SeoTitle).HasMaxLength(255);
            e.HasIndex(x => x.Slug).IsUnique();
        });

        // ==================== Message 留言板消息表 ====================
        modelBuilder.Entity<Message>(e =>
        {
            e.ToTable("messages");
            e.HasKey(x => x.Id);
            if (!isSqlite) e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Nickname).HasMaxLength(64).IsRequired();
            e.Property(x => x.Email).HasMaxLength(255);
            e.Property(x => x.Content).IsRequired();
            e.Property(x => x.IpAddress).HasMaxLength(64);
            e.Property(x => x.IpCity).HasMaxLength(128);
            e.Property(x => x.Status).HasMaxLength(32).IsRequired().HasDefaultValue("pending");
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.CreatedAt).IsDescending();
            e.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}