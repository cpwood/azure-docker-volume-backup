using System.IO;

namespace AzureDockerVolumeBackup.Files
{
    public interface IFileService
    {
        string[] GetAllVolumes();
        string[] GetAllFiles(string volume);
        Stream OpenFile(string file);
        Stream WriteFile(string file);
        void DeleteIfExists(string file);
    }
}