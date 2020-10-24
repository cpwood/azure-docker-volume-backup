using System.Threading;
using System.Threading.Tasks;
using AzureDockerVolumeBackup.Backups;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Lambda;
using Options = AzureDockerVolumeBackup.Settings.Options;

namespace AzureDockerVolumeBackup
{
    public class Worker : BackgroundService
    {
        private readonly IScheduler _scheduler;
        private readonly IBackupService _backupService;
        private readonly Options _options;

        public Worker(
            IScheduler scheduler,
            IBackupService backupService,
            IOptions<Options> options)
        {
            _scheduler = scheduler;
            _backupService = backupService;
            _options = options.Value;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _scheduler.ScheduleJob(async () =>
                {
                    await _backupService.RemoveElderlyBackupsAsync();
                    await _backupService.DoBackupsAsync();
                }, 
                builder => builder.StartNow()
                    .WithCronSchedule(_options.Schedule));

            await _scheduler.Start(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(int.MaxValue, stoppingToken);
        }

    }
}