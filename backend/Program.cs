using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Services;
using Serilog;
using Mblog.API;

// ========== Serilog 初始化 ==========
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/mblog-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 使用 Serilog 作为日志提供程序
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/mblog-.log", rollingInterval: RollingInterval.Day));

    // ========== 数据库连接：优先从环境变量读取 ==========
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
    var dbName = Environment.GetEnvironmentVariable("DB_NAME");
    var dbUser = Environment.GetEnvironmentVariable("DB_USER");
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

    string connectionString;
    if (!string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbName)
        && !string.IsNullOrEmpty(dbUser) && !string.IsNullOrEmpty(dbPassword))
    {
        var port = string.IsNullOrEmpty(dbPort) ? "5432" : dbPort;
        connectionString = $"Host={dbHost};Port={port};Database={dbName};Username={dbUser};Password={dbPassword}";
    }
    else
    {
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("数据库连接字符串未配置。请设置环境变量 DB_HOST/DB_PORT/DB_NAME/DB_USER/DB_PASSWORD，或配置 ConnectionStrings:DefaultConnection。");
    }

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        var usePg = !string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbName)
            && !string.IsNullOrEmpty(dbUser) && !string.IsNullOrEmpty(dbPassword);

        // 抑制"模型有挂起更改"警告，允许 Migrate() 在无迁移文件的情况下运行
        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

        if (usePg)
        {
            // PostgreSQL（通过环境变量配置）
            options.UseNpgsql(connectionString);
        }
        else if (connectionString.Contains("Host="))
        {
            // PostgreSQL（通过 appsettings.json 配置）
            options.UseNpgsql(connectionString);
        }
        else
        {
            // SQLite（本地开发，无需安装 PostgreSQL）
            var sqlitePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "mblog.db");
            if (!File.Exists(sqlitePath))
            {
                sqlitePath = Path.Combine(Directory.GetCurrentDirectory(), "mblog.db");
            }
            options.UseSqlite($"Data Source={sqlitePath}");
        }
    });

    // ========== 健康检查 ==========
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);

    // ========== CORS ==========
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // ========== JWT 认证 ==========
    var jwtSection = builder.Configuration.GetSection("Jwt");
    builder.Services.Configure<JwtSettings>(jwtSection);

    var jwtSettings = jwtSection.Get<JwtSettings>() ?? throw new InvalidOperationException("JWT 配置缺失");
    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

    // ========== 业务服务 ==========
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<ITagService, TagService>();
    builder.Services.AddScoped<IArticleService, ArticleService>();
    builder.Services.AddScoped<IMediaService, MediaService>();
    builder.Services.AddScoped<ICaptchaService, CaptchaService>();
    builder.Services.AddHttpClient<ICommentService, CommentService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(5);
    });
    builder.Services.AddScoped<IProfileService, ProfileService>();
    builder.Services.AddScoped<IProjectService, ProjectService>();
    builder.Services.AddScoped<IFriendService, FriendService>();
    builder.Services.AddScoped<IPageService, PageService>();
    builder.Services.AddScoped<INavigationService, NavigationService>();
    builder.Services.AddScoped<IFooterService, FooterService>();
    builder.Services.AddScoped<IThemeService, ThemeService>();
    builder.Services.AddScoped<ILayoutService, LayoutService>();
    builder.Services.AddScoped<ISiteSettingService, SiteSettingService>();
    builder.Services.AddHttpClient<IVisitService, VisitService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(5);
    });
    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
    builder.Services.AddScoped<ISeoService, SeoService>();
    builder.Services.AddScoped<IMessageService, MessageService>();
    builder.Services.AddScoped<IWeeklyDigestService, WeeklyDigestService>();
builder.Services.AddScoped<ILlmService, LlmService>();
builder.Services.AddScoped<IImageGenService, ImageGenService>();
builder.Services.AddScoped<IArticleAiService, ArticleAiService>();
    builder.Services.AddHostedService<VisitCleanupJob>();
    builder.Services.AddHostedService<WeeklyDigestJob>();
    builder.Services.Configure<StorageSettings>(builder.Configuration.GetSection(StorageSettings.SectionName));

    // ========== Rate Limiting（评论接口限流） ==========
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = 429;

        // 全局限流：每 IP 每秒最多 10 个请求
        options.AddPolicy("Global", context =>
            System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromSeconds(1),
                }));

        // 评论提交限流：每 IP 每分钟最多 5 次
        options.AddPolicy("CommentPerMinute", context =>
            System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                }));

        // 验证码获取限流：每 IP 每分钟最多 10 次
        options.AddPolicy("CaptchaPerMinute", context =>
            System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                }));
    });

    // ========== 控制器 + FluentValidation ==========
    builder.Services.AddControllers()
        .ConfigureInvalidModelStateResponse(); // 统一校验失败响应

    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

    // ========== Swagger（开发环境） ==========
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // ========== 中间件管道 ==========
    // 全局异常处理（须在最外层）
    app.UseMiddleware<ExceptionMiddleware>();

    // Swagger（仅开发环境）
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // 自动初始化数据库：应用迁移（不会删除数据）
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 自动应用迁移（会按顺序执行所有未应用的迁移，新数据库自动创建所有表）
        try
        {
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            // 迁移失败通常是迁移记录与真实表结构不同步（如表已存在但 __EFMigrationsHistory 为空）
            Log.Warning(ex, "数据库迁移失败，触发自动修复...");
        }

        // 确保 messages 表存在（无论 Migrate 成功还是失败都执行，幂等安全）
        try
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();

            // 检查表是否存在
            cmd.CommandText = """SELECT to_regclass('"tags"') IS NOT NULL""";
            var hasTables = (await cmd.ExecuteScalarAsync()) as bool? == true;

            if (!hasTables)
            {
                // 新数据库：EnsureCreated 创建所有表
                db.Database.EnsureCreated();
            }
            else
            {
                // 已有数据库：补充可能的缺失列
                cmd.CommandText = """
                    ALTER TABLE "tags" ADD COLUMN IF NOT EXISTS "BgColor" text NULL;
                    ALTER TABLE "visits" ADD COLUMN IF NOT EXISTS "IpAddress" text NULL;
                    ALTER TABLE "visits" ADD COLUMN IF NOT EXISTS "Location" text NULL;
                    ALTER TABLE "comments" ADD COLUMN IF NOT EXISTS "IpAddress" text NULL;
                    ALTER TABLE "comments" ADD COLUMN IF NOT EXISTS "Location" text NULL;
                """;
                await cmd.ExecuteNonQueryAsync();
            }

            // 创建新表（messages 等新增实体，幂等安全）
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS "messages" (
                    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
                    "ParentId" uuid NULL,
                    "Nickname" text NOT NULL,
                    "Email" text NULL,
                    "Content" text NOT NULL,
                    "IpAddress" text NULL,
                    "IpCity" text NULL,
                    "UserAgent" text NULL,
                    "Status" text NOT NULL DEFAULT 'pending',
                    "AdminReply" text NULL,
                    "AdminRepliedAt" timestamp with time zone NULL,
                    "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
                    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT now(),
                    CONSTRAINT "PK_messages" PRIMARY KEY ("Id")
                );
                CREATE INDEX IF NOT EXISTS "IX_messages_Status" ON "messages" ("Status");
                CREATE INDEX IF NOT EXISTS "IX_messages_CreatedAt" ON "messages" ("CreatedAt" DESC);

                -- Email settings table
                CREATE TABLE IF NOT EXISTS "EmailSettings" (
                    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
                    "SmtpServer" text NOT NULL DEFAULT '',
                    "SmtpPort" integer NOT NULL DEFAULT 587,
                    "SmtpUsername" text NOT NULL DEFAULT '',
                    "SmtpPassword" text NOT NULL DEFAULT '',
                    "SenderEmail" text NOT NULL DEFAULT '',
                    "SenderName" text NOT NULL DEFAULT '',
                    "UseSsl" boolean NOT NULL DEFAULT true,
                    "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
                    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT now(),
                    CONSTRAINT "PK_EmailSettings" PRIMARY KEY ("Id")
                );

                -- Email templates table
                CREATE TABLE IF NOT EXISTS "EmailTemplates" (
                    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
                    "TemplateKey" text NOT NULL,
                    "Subject" text NOT NULL DEFAULT '',
                    "HtmlContent" text NOT NULL DEFAULT '',
                    "Description" text NULL,
                    "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
                    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT now(),
                    CONSTRAINT "PK_EmailTemplates" PRIMARY KEY ("Id")
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_EmailTemplates_TemplateKey" ON "EmailTemplates" ("TemplateKey");

                -- Email logs table
                CREATE TABLE IF NOT EXISTS "EmailLogs" (
                    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
                    "SubscriberId" uuid NULL,
                    "TemplateKey" text NULL,
                    "Email" text NOT NULL DEFAULT '',
                    "Subject" text NOT NULL DEFAULT '',
                    "SentAt" timestamp with time zone NOT NULL DEFAULT now(),
                    "IsSuccess" boolean NOT NULL DEFAULT false,
                    "ErrorMessage" text NULL,
                    "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
                    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT now(),
                    CONSTRAINT "PK_EmailLogs" PRIMARY KEY ("Id")
                );
                CREATE INDEX IF NOT EXISTS "IX_EmailLogs_Email" ON "EmailLogs" ("Email");
                CREATE INDEX IF NOT EXISTS "IX_EmailLogs_SentAt" ON "EmailLogs" ("SentAt" DESC);

                -- Add new columns to Subscribers
                ALTER TABLE "Subscribers" ADD COLUMN IF NOT EXISTS "ConfirmationToken" text NULL;
                ALTER TABLE "Subscribers" ADD COLUMN IF NOT EXISTS "ConfirmedAt" timestamp with time zone NULL;
                ALTER TABLE "Subscribers" ADD COLUMN IF NOT EXISTS "TokenExpiresAt" timestamp with time zone NULL;
            """;
            await cmd.ExecuteNonQueryAsync();

            // 单独添加外键约束（DO 块确保幂等）
            cmd.CommandText = """
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_messages_messages_ParentId') THEN
                        ALTER TABLE "messages" ADD CONSTRAINT "FK_messages_messages_ParentId"
                            FOREIGN KEY ("ParentId") REFERENCES "messages" ("Id") ON DELETE CASCADE;
                    END IF;
                END
                $$;
            """;
            await cmd.ExecuteNonQueryAsync();

            // 插入缺失的迁移记录（幂等安全）
            var allMigrations = new[] {
                "20260605155241_InitialCreate",
                "20260607151307_AddPageEnableComments",
                "20260607230809_AddSubscriptionAndSubscriber",
                "20260608023718_AddMultiCategoryAndSubCategory",
                "20260608130302_AddIpAndLocationToVisit",
                "20260608223450_AddIpAndLocationToComment",
                "20260610000000_AddEmailSystem"
            };
            foreach (var migrationId in allMigrations)
            {
                cmd.CommandText = $"""
                    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                    VALUES ('{migrationId}', '10.0.8')
                    ON CONFLICT ("MigrationId") DO NOTHING
                """;
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "自动修复执行失败（非致命，可忽略）");
        }
        
        // 种子数据（已有用户则跳过）
        await DbInitializer.SeedAsync(db);
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    // 静态文件服务（wwwroot）
    app.UseStaticFiles();

    // 静态文件服务（上传目录 /uploads -> ./uploads）
    var uploadsRoot = Path.Combine(app.Environment.ContentRootPath, "uploads");
    if (!Directory.Exists(uploadsRoot))
        Directory.CreateDirectory(uploadsRoot);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsRoot),
        RequestPath = "/uploads"
    });

    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    // 健康检查端点
    app.MapHealthChecks("/api/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description
                })
            };
            await System.Text.Json.JsonSerializer.SerializeAsync(context.Response.Body, response);
        }
    });

    app.MapControllers();

    Log.Information("应用启动成功");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "应用启动失败");
}
finally
{
    Log.CloseAndFlush();
}