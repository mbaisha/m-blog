CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE captcha_sessions (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "SessionId" character varying(128) NOT NULL,
        "AnswerHash" text NOT NULL,
        "IpHash" character varying(128) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "UsedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_captcha_sessions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE pages (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "Title" character varying(255) NOT NULL,
        "Slug" character varying(255) NOT NULL,
        "Content" text,
        "Summary" text,
        "Status" character varying(32) NOT NULL DEFAULT 'draft',
        "IsVisible" boolean NOT NULL,
        "SortOrder" integer NOT NULL,
        "SeoTitle" character varying(255),
        "SeoDescription" text,
        "SeoKeywords" text,
        "PublishedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_pages" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE profile_sections (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "SectionType" character varying(64) NOT NULL,
        "Title" character varying(128),
        "Content" text,
        "SortOrder" integer NOT NULL,
        "Metadata" jsonb NOT NULL DEFAULT ('{}'::jsonb),
        "IsEnabled" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_profile_sections" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE tags (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "Name" character varying(128) NOT NULL,
        "Slug" character varying(128) NOT NULL,
        "Color" character varying(32),
        "DeletedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_tags" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE article_tags (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "ArticleId" uuid NOT NULL,
        "TagId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_article_tags" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_article_tags_tags_TagId" FOREIGN KEY ("TagId") REFERENCES tags ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE articles (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "Title" character varying(255) NOT NULL,
        "Slug" character varying(255) NOT NULL,
        "Summary" text,
        "Content" text NOT NULL,
        "CoverImageId" uuid,
        "AuthorId" uuid NOT NULL,
        "CategoryId" uuid,
        "Status" character varying(32) NOT NULL DEFAULT 'draft',
        "IsTop" boolean NOT NULL,
        "IsRecommend" boolean NOT NULL,
        "ViewCount" bigint NOT NULL,
        "LikeCount" bigint NOT NULL,
        "CommentCount" bigint NOT NULL,
        "SeoTitle" character varying(255),
        "SeoDescription" text,
        "SeoKeywords" text,
        "PublishedAt" timestamp with time zone,
        "DeletedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_articles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE visits (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "ArticleId" uuid,
        "PagePath" text,
        "IpHash" character varying(128),
        "UserAgent" text,
        "Referer" text,
        "VisitedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_visits" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_visits_articles_ArticleId" FOREIGN KEY ("ArticleId") REFERENCES articles ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE audit_logs (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "UserId" uuid,
        "Action" character varying(128) NOT NULL,
        "ResourceType" character varying(64),
        "ResourceId" uuid,
        "Ip" character varying(45),
        "UserAgent" text,
        "Details" jsonb NOT NULL DEFAULT ('{}'::jsonb),
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_audit_logs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE categories (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "Name" character varying(128) NOT NULL,
        "Slug" character varying(128) NOT NULL,
        "Description" text,
        "CoverImageId" uuid,
        "SortOrder" integer NOT NULL,
        "SeoTitle" character varying(255),
        "SeoDescription" text,
        "SeoKeywords" text,
        "DeletedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_categories" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE comments (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "ArticleId" uuid NOT NULL,
        "ParentId" uuid,
        "Nickname" character varying(64) NOT NULL,
        "Email" character varying(255),
        "Website" character varying(512),
        "Content" text NOT NULL,
        "Avatar" text,
        "IpHash" character varying(128) NOT NULL,
        "Status" character varying(32) NOT NULL DEFAULT 'pending',
        "IsSpam" boolean NOT NULL,
        "ReviewedBy" uuid,
        "ReviewedAt" timestamp with time zone,
        "DeletedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_comments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_comments_articles_ArticleId" FOREIGN KEY ("ArticleId") REFERENCES articles ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_comments_comments_ParentId" FOREIGN KEY ("ParentId") REFERENCES comments ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE friends (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "Name" character varying(128) NOT NULL,
        "Url" text NOT NULL,
        "Description" text,
        "LogoImageId" uuid,
        "SortOrder" integer NOT NULL,
        "IsVisible" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_friends" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE likes (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "ArticleId" uuid NOT NULL,
        "UserId" uuid,
        "IpHash" text,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "CreatedIp" character varying(45),
        CONSTRAINT "PK_likes" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_likes_articles_ArticleId" FOREIGN KEY ("ArticleId") REFERENCES articles ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE media (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "Type" character varying(32) NOT NULL,
        "Url" text NOT NULL,
        "ThumbnailUrl" text,
        "Filename" text NOT NULL,
        "OriginalFilename" text,
        "StoragePath" text NOT NULL,
        "SizeBytes" bigint NOT NULL,
        "MimeType" character varying(128) NOT NULL,
        "Width" integer,
        "Height" integer,
        "DurationSeconds" numeric(10,2),
        "CreatedBy" uuid,
        "DeletedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_media" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE projects (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "Title" character varying(255) NOT NULL,
        "Slug" character varying(255) NOT NULL,
        "Summary" text,
        "Content" text,
        "CoverImageId" uuid,
        "TechStack" jsonb NOT NULL DEFAULT ('[]'::jsonb),
        "ProjectUrl" character varying(512),
        "GithubUrl" character varying(512),
        "DemoUrl" character varying(512),
        "SortOrder" integer NOT NULL,
        "IsVisible" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_projects" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_projects_media_CoverImageId" FOREIGN KEY ("CoverImageId") REFERENCES media ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE seo_settings (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "PageKey" character varying(64) NOT NULL,
        "Title" character varying(255),
        "Description" text,
        "Keywords" text,
        "OgImageId" uuid,
        "CanonicalUrl" text,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_seo_settings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_seo_settings_media_OgImageId" FOREIGN KEY ("OgImageId") REFERENCES media ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE users (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "Username" character varying(64) NOT NULL,
        "Email" character varying(255),
        "PasswordHash" text NOT NULL,
        "Role" character varying(32) NOT NULL DEFAULT 'admin',
        "AvatarMediaId" uuid,
        "Status" character varying(32) NOT NULL DEFAULT 'active',
        "LastLoginAt" timestamp with time zone,
        "DeletedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_users" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_users_media_AvatarMediaId" FOREIGN KEY ("AvatarMediaId") REFERENCES media ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE TABLE refresh_tokens (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "UserId" uuid NOT NULL,
        "TokenHash" text NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "RevokedAt" timestamp with time zone,
        "CreatedIp" text,
        "CreatedUserAgent" text,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_refresh_tokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_refresh_tokens_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_article_tags_ArticleId_TagId" ON article_tags ("ArticleId", "TagId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_article_tags_TagId" ON article_tags ("TagId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_articles_AuthorId" ON articles ("AuthorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_articles_CategoryId" ON articles ("CategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_articles_CoverImageId" ON articles ("CoverImageId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_articles_IsRecommend_PublishedAt" ON articles ("IsRecommend" DESC, "PublishedAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_articles_IsTop_PublishedAt" ON articles ("IsTop" DESC, "PublishedAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_articles_Slug" ON articles ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_articles_Status_PublishedAt" ON articles ("Status", "PublishedAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_audit_logs_Action" ON audit_logs ("Action");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_audit_logs_CreatedAt" ON audit_logs ("CreatedAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_audit_logs_UserId" ON audit_logs ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_captcha_sessions_ExpiresAt" ON captcha_sessions ("ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_captcha_sessions_SessionId" ON captcha_sessions ("SessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_categories_CoverImageId" ON categories ("CoverImageId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_categories_Slug" ON categories ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_categories_SortOrder_CreatedAt" ON categories ("SortOrder", "CreatedAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_comments_ArticleId_Status_CreatedAt" ON comments ("ArticleId", "Status", "CreatedAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_comments_CreatedAt" ON comments ("CreatedAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_comments_IpHash" ON comments ("IpHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_comments_ParentId" ON comments ("ParentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_comments_ReviewedBy" ON comments ("ReviewedBy");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_comments_Status" ON comments ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_friends_IsVisible_SortOrder" ON friends ("IsVisible", "SortOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_friends_LogoImageId" ON friends ("LogoImageId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_likes_ArticleId" ON likes ("ArticleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_likes_ArticleId_IpHash" ON likes ("ArticleId", "IpHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_likes_ArticleId_UserId" ON likes ("ArticleId", "UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_likes_CreatedAt" ON likes ("CreatedAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_likes_UserId" ON likes ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_media_CreatedAt" ON media ("CreatedAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_media_CreatedBy" ON media ("CreatedBy");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_media_Type" ON media ("Type");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_pages_Slug" ON pages ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_profile_sections_SectionType_SortOrder" ON profile_sections ("SectionType", "SortOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_projects_CoverImageId" ON projects ("CoverImageId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_projects_IsVisible_SortOrder_CreatedAt" ON projects ("IsVisible" DESC, "SortOrder", "CreatedAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_projects_Slug" ON projects ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_refresh_tokens_ExpiresAt" ON refresh_tokens ("ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_refresh_tokens_TokenHash" ON refresh_tokens ("TokenHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_refresh_tokens_UserId" ON refresh_tokens ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_seo_settings_OgImageId" ON seo_settings ("OgImageId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_seo_settings_PageKey" ON seo_settings ("PageKey");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_tags_Name" ON tags ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_tags_Slug" ON tags ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_users_AvatarMediaId" ON users ("AvatarMediaId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_users_Email" ON users ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_users_Status" ON users ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_users_Username" ON users ("Username");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_visits_ArticleId_VisitedAt" ON visits ("ArticleId", "VisitedAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_visits_IpHash_VisitedAt" ON visits ("IpHash", "VisitedAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    CREATE INDEX "IX_visits_VisitedAt" ON visits ("VisitedAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    ALTER TABLE article_tags ADD CONSTRAINT "FK_article_tags_articles_ArticleId" FOREIGN KEY ("ArticleId") REFERENCES articles ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    ALTER TABLE articles ADD CONSTRAINT "FK_articles_categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES categories ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    ALTER TABLE articles ADD CONSTRAINT "FK_articles_media_CoverImageId" FOREIGN KEY ("CoverImageId") REFERENCES media ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    ALTER TABLE articles ADD CONSTRAINT "FK_articles_users_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES users ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    ALTER TABLE audit_logs ADD CONSTRAINT "FK_audit_logs_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    ALTER TABLE categories ADD CONSTRAINT "FK_categories_media_CoverImageId" FOREIGN KEY ("CoverImageId") REFERENCES media ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    ALTER TABLE comments ADD CONSTRAINT "FK_comments_users_ReviewedBy" FOREIGN KEY ("ReviewedBy") REFERENCES users ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    ALTER TABLE friends ADD CONSTRAINT "FK_friends_media_LogoImageId" FOREIGN KEY ("LogoImageId") REFERENCES media ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    ALTER TABLE likes ADD CONSTRAINT "FK_likes_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    ALTER TABLE media ADD CONSTRAINT "FK_media_users_CreatedBy" FOREIGN KEY ("CreatedBy") REFERENCES users ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605155241_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260605155241_InitialCreate', '10.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607151307_AddPageEnableComments') THEN
    ALTER TABLE pages ADD "EnableComments" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607151307_AddPageEnableComments') THEN
    CREATE TABLE "FooterConfigs" (
        "Id" uuid NOT NULL,
        "Copyright" text,
        "IcpNumber" text,
        "IcpUrl" text,
        "PoliceNumber" text,
        "PoliceUrl" text,
        "SocialLinks" text NOT NULL,
        "Blocks" text NOT NULL,
        "Style" text NOT NULL,
        "IsVisible" boolean NOT NULL,
        "ContactEmail" text,
        "ContactWeChat" text,
        "ContactPhone" text,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_FooterConfigs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607151307_AddPageEnableComments') THEN
    CREATE TABLE "ModuleLayouts" (
        "Id" uuid NOT NULL,
        "PageKey" text NOT NULL,
        "ModuleKey" text NOT NULL,
        "Title" text,
        "SortOrder" integer NOT NULL,
        "IsEnabled" boolean NOT NULL,
        "Config" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_ModuleLayouts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607151307_AddPageEnableComments') THEN
    CREATE TABLE "NavigationItems" (
        "Id" uuid NOT NULL,
        "Title" text NOT NULL,
        "Url" text NOT NULL,
        "ParentId" uuid,
        "Icon" text,
        "OpenInNewTab" boolean NOT NULL,
        "SortOrder" integer NOT NULL,
        "IsVisible" boolean NOT NULL,
        "Location" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_NavigationItems" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607151307_AddPageEnableComments') THEN
    CREATE TABLE site_settings (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "SiteName" character varying(128) NOT NULL DEFAULT 'My Blog',
        "SiteDescription" character varying(512),
        "LogoImageId" uuid,
        "FaviconImageId" uuid,
        "CommentModerationEnabled" boolean NOT NULL,
        "VisitRetentionDays" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_site_settings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_site_settings_media_FaviconImageId" FOREIGN KEY ("FaviconImageId") REFERENCES media ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_site_settings_media_LogoImageId" FOREIGN KEY ("LogoImageId") REFERENCES media ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607151307_AddPageEnableComments') THEN
    CREATE TABLE "ThemeSettings" (
        "Id" uuid NOT NULL,
        "ThemeName" text NOT NULL,
        "PrimaryColor" text NOT NULL,
        "AccentColor" text NOT NULL,
        "BackgroundColor" text NOT NULL,
        "TextColor" text NOT NULL,
        "LinkColor" text NOT NULL,
        "NavbarBackground" text,
        "NavbarTextColor" text,
        "BorderRadius" text NOT NULL,
        "CustomCss" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_ThemeSettings" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607151307_AddPageEnableComments') THEN
    CREATE INDEX "IX_site_settings_FaviconImageId" ON site_settings ("FaviconImageId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607151307_AddPageEnableComments') THEN
    CREATE INDEX "IX_site_settings_LogoImageId" ON site_settings ("LogoImageId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607151307_AddPageEnableComments') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260607151307_AddPageEnableComments', '10.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607230809_AddSubscriptionAndSubscriber') THEN
    ALTER TABLE site_settings ADD "SubscriptionEnabled" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607230809_AddSubscriptionAndSubscriber') THEN
    CREATE TABLE "Subscribers" (
        "Id" uuid NOT NULL,
        "Email" text NOT NULL,
        "SubscribedAt" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "UnsubscribedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_Subscribers" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607230809_AddSubscriptionAndSubscriber') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260607230809_AddSubscriptionAndSubscriber', '10.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260608023718_AddMultiCategoryAndSubCategory') THEN
    ALTER TABLE categories ADD "Level" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260608023718_AddMultiCategoryAndSubCategory') THEN
    ALTER TABLE categories ADD "ParentId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260608023718_AddMultiCategoryAndSubCategory') THEN
    CREATE TABLE article_categories (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "ArticleId" uuid NOT NULL,
        "CategoryId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_article_categories" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_article_categories_articles_ArticleId" FOREIGN KEY ("ArticleId") REFERENCES articles ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_article_categories_categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES categories ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260608023718_AddMultiCategoryAndSubCategory') THEN
    CREATE INDEX "IX_categories_ParentId" ON categories ("ParentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260608023718_AddMultiCategoryAndSubCategory') THEN
    CREATE UNIQUE INDEX "IX_article_categories_ArticleId_CategoryId" ON article_categories ("ArticleId", "CategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260608023718_AddMultiCategoryAndSubCategory') THEN
    CREATE INDEX "IX_article_categories_CategoryId" ON article_categories ("CategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260608023718_AddMultiCategoryAndSubCategory') THEN
    ALTER TABLE categories ADD CONSTRAINT "FK_categories_categories_ParentId" FOREIGN KEY ("ParentId") REFERENCES categories ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260608023718_AddMultiCategoryAndSubCategory') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260608023718_AddMultiCategoryAndSubCategory', '10.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260608130302_AddIpAndLocationToVisit') THEN
    ALTER TABLE visits ADD "IpAddress" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260608130302_AddIpAndLocationToVisit') THEN
    ALTER TABLE visits ADD "Location" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260608130302_AddIpAndLocationToVisit') THEN
    ALTER TABLE tags ADD "BgColor" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260608130302_AddIpAndLocationToVisit') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260608130302_AddIpAndLocationToVisit', '10.0.8');
    END IF;
END $EF$;
COMMIT;

