using System;
using System.Reflection;
using Azure.Storage.Blobs;
using AzureDockerVolumeBackup.Backups;
using AzureDockerVolumeBackup.Blobs;
using AzureDockerVolumeBackup.Calendar;
using AzureDockerVolumeBackup.Files;
using AzureDockerVolumeBackup.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz.Impl;

namespace AzureDockerVolumeBackup
{
    static class Program
    {
        static void Main(string[] args)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            Console.WriteLine($"Azure Docker Volume Backup - v{version}");
            
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => builder
                    .AddJsonFile("appsettings.json", false)
                    .AddJsonFile("appsettings.debug.json", true)
                    .AddEnvironmentVariables())
                .ConfigureLogging(builder => builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole(x => x.TimestampFormat = "[yyyy-MM-dd HH:mm:ss]"))
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<Options>(hostContext.Configuration);

                    var options = hostContext.Configuration.Get<Options>();
                    
                    var blobServiceClient = new BlobServiceClient(
                        $"DefaultEndpointsProtocol=https;AccountName={options.Account};AccountKey={options.Key};EndpointSuffix=core.windows.net");

                    var containerClient = blobServiceClient.GetBlobContainerClient(options.Container);
                    services.AddSingleton(containerClient);
                    
                    var factory = new StdSchedulerFactory();
                    var scheduler = factory.GetScheduler().Result;
                    services.AddSingleton(scheduler);
                    
                    services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
                    services.AddSingleton<IFileService, FileService>();
                    services.AddSingleton<IBlobService, BlobService>();
                    services.AddSingleton<IBackupService, BackupService>();
                    services.AddHostedService<Worker>();
                });
    }
}