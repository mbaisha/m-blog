using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mblog.API.Services;

/// <summary>
/// 每周邮件摘要发送任务（每周一 6:00 执行）
/// </summary>
public class WeeklyDigestJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeeklyDigestJob> _logger;

    public WeeklyDigestJob(IServiceScopeFactory scopeFactory, ILogger<WeeklyDigestJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("每周邮件摘要任务已启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var nextMonday = GetNextMonday0600(now);
                var delay = nextMonday - now;

                if (delay > TimeSpan.Zero)
                {
                    _logger.LogInformation("距下次周报发送还有 {Hours} 小时", delay.TotalHours.ToString("F1"));
                    await Task.Delay(delay, stoppingToken);
                }

                if (stoppingToken.IsCancellationRequested) break;

                using var scope = _scopeFactory.CreateScope();
                var digestService = scope.ServiceProvider.GetRequiredService<IWeeklyDigestService>();
                var result = await digestService.ExecuteDigestAsync(stoppingToken);
                _logger.LogInformation("周报发送结果: {Message}", result.Message);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "每周邮件摘要任务执行出错，24 小时后重试");
                try { await Task.Delay(TimeSpan.FromHours(24), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private static DateTimeOffset GetNextMonday0600(DateTimeOffset now)
    {
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0 && now.TimeOfDay >= TimeSpan.FromHours(6))
            daysUntilMonday = 7;

        var next = now.Date.AddDays(daysUntilMonday).AddHours(6);
        return new DateTimeOffset(next, now.Offset);
    }
}