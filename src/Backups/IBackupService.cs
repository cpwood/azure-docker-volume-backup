using System.Threading.Tasks;

namespace AzureDockerVolumeBackup.Backups
{
    public interface IBackupService
    {
        Task RemoveElderlyBackupsAsync();
        Task DoBackupsAsync();
    }
}