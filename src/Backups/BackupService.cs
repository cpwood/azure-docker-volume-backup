using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDockerVolumeBackup.Blobs;
using AzureDockerVolumeBackup.Calendar;
using AzureDockerVolumeBackup.Files;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options = AzureDockerVolumeBackup.Settings.Options;

namespace AzureDockerVolumeBackup.Backups
{
    public class BackupService : IBackupService
    {
        private readonly IFileService _fileService;
        private readonly IBlobService _blobService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<BackupService> _logger;
        private readonly Options _options;
        
        public BackupService(
            IFileService fileService,
            IBlobService blobService,
            IDateTimeProvider dateTimeProvider,
            IOptions<Options> options,
            ILogger<BackupService> logger)
        {
            _fileService = fileService;
            _blobService = blobService;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _options = options.Value;
        }
        
        public async Task RemoveElderlyBackupsAsync()
        {
            try
            {
                var earlierThan = _dateTimeProvider.Now.AddDays(_options.DaysToKeep * -1).ToString("yyyy-MM-dd HH:mm:ss");
                var blobs = await _blobService.ListBlobsAsync(earlierThan);

                foreach (var blob in blobs)
                {
                    _logger.LogTrace($"Deleting '{blob}' ..");
                    await _blobService.DeleteBlobAsync(blob);
                }

                _logger.LogInformation($"Deleted {blobs.Length} backup files.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while removing elderly backups.");
            }

        }

        public async Task DoBackupsAsync()
        {
            foreach (var volume in _fileService.GetAllVolumes())
            {
                _logger.LogInformation($"Processing '{volume}' ..");
                
                var volumeName = volume.Split('/', '\\').Last();
                var filename =
                    $"{_dateTimeProvider.Now:yyyy-MM-dd HH-mm-ss} {volumeName}.tar.gz";
                
                try
                {
                    var files = _fileService.GetAllFiles(volume);

                    if (!files.Any())
                    {
                        _logger.LogInformation($"Skipping volume '{volume}' as it has no content");
                        continue;
                    }
                    
                    await using var outStream = _fileService.WriteFile(filename);
                    await using var gzoStream = new GZipOutputStream(outStream);
                    using var tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);
                    
                    tarArchive.RootPath = volume;

                    foreach (var file in files)
                    {
                        _logger.LogTrace($"Archiving '{file}' ..");

                        if (_fileService.IsFileLocked(file))
                        {
                            _logger.LogWarning($"File '{file}' is locked and cannot be read. It isn't included in the backup.");
                            continue;
                        }
                        
                        var tarEntry = TarEntry.CreateEntryFromFile(file);
                        tarEntry.Name = file;
                        tarArchive.WriteEntry(tarEntry, true);
                    }
                    
                    tarArchive.Close();
                    gzoStream.Close();
                    outStream.Close();
                    
                    _logger.LogTrace($"Uploading '{filename}' ..");

                    await _blobService.UploadBlobAsync(filename);
                    
                    _logger.LogInformation($"Uploaded '{filename}'.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred while processing '{volume}'.");
                }
                finally
                {
                    _fileService.DeleteIfExists(filename);
                }

            }
        }
    }
}