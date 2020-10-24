using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using AzureDockerVolumeBackup.Files;

namespace AzureDockerVolumeBackup.Blobs
{
    public class BlobService : IBlobService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly IFileService _fileService;

        public BlobService(
            BlobContainerClient containerClient,
            IFileService fileService)
        {
            _containerClient = containerClient;
            _fileService = fileService;
        }
        
        public async Task UploadBlobAsync(string file)
        {
            var blobClient = _containerClient.GetBlobClient(file);
            await using var stream = _fileService.OpenFile(file);
            await blobClient.UploadAsync(stream, true);
            stream.Close();
        }

        public async Task<string[]> ListBlobsAsync(string earlierThan)
        {
            var blobs = new List<string>();
            await foreach (var blobItem in _containerClient.GetBlobsAsync())
            {
                if (string.CompareOrdinal(blobItem.Name, earlierThan) <= 0)
                    blobs.Add(blobItem.Name);
            }

            return blobs.ToArray();
        }

        public async Task DeleteBlobAsync(string blob)
        {
            await _containerClient.DeleteBlobAsync(blob);
        }
    }
}