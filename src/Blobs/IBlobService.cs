using System.Threading.Tasks;

namespace AzureDockerVolumeBackup.Blobs
{
    public interface IBlobService
    {
        Task UploadBlobAsync(string file);
        Task<string[]> ListBlobsAsync(string earlierThan);
        Task DeleteBlobAsync(string blob);
    }
}