using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mblog.API.Services;

/// <summary>
/// 定时清理过期访问记录（每天凌晨 3 点执行，默认保留 30 天）
/// </summary>
public class VisitCleanupJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VisitCleanupJob> _logger;
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);
    private const int RetentionDays = 30;

    public VisitCleanupJob(IServiceScopeFactory scopeFactory, ILogger<VisitCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("访问记录清理任务已启动");

        // 首次运行延迟至下一个凌晨 3 点
        var now = DateTimeOffset.UtcNow;
        var nextRun = now.Date.AddDays(1).AddHours(3);
        var initialDelay = nextRun - now;
        if (initialDelay > TimeSpan.Zero)
        {
            try { await Task.Delay(initialDelay, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var visitService = scope.ServiceProvider.GetRequiredService<IVisitService>();
                var result = await visitService.CleanupAsync(RetentionDays);
                _logger.LogInformation("访问记录清理完成：删除 {Count} 条 {BeforeDate} 之前的记录",
                    result.DeletedCount, result.BeforeDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "访问记录清理任务执行出错");
            }

            try { await Task.Delay(RunInterval, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }
}