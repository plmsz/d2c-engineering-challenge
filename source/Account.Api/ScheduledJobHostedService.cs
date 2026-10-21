using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Account.Tasks.Jobs;

namespace Account.Api
{
    public class ScheduledJobHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ScheduledJobHostedService> _logger;

        public ScheduledJobHostedService(IServiceScopeFactory scopeFactory, ILogger<ScheduledJobHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string schedule;
            using (var scope = _scopeFactory.CreateScope())
                schedule = scope.ServiceProvider.GetRequiredService<SyncBiometricVerificationsJob>().Schedule;

            var expression = CronExpression.Parse(schedule, CronFormat.IncludeSeconds);
            var cursor = DateTimeOffset.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                var next = expression.GetNextOccurrence(cursor, TimeZoneInfo.Utc);
                if (next is null) break;

                try
                {
                    var wait = next.Value - DateTimeOffset.UtcNow;
                    if (wait > TimeSpan.Zero)
                        await Task.Delay(wait, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                cursor = next.Value;

                using var scope = _scopeFactory.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<SyncBiometricVerificationsJob>();

                _logger.LogInformation("Executando job SyncBiometricVerificationsJob às {Time}", DateTimeOffset.UtcNow);

                try
                {
                    await job.DoWorkAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao executar o job");
                }
            }
        }
    }
}