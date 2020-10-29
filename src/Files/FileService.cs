using System.IO;
using Microsoft.Extensions.Options;
using Options = AzureDockerVolumeBackup.Settings.Options;

namespace AzureDockerVolumeBackup.Files
{
    public class FileService : IFileService
    {
        private readonly Options _options;
        
        public FileService(IOptions<Options> options)
        {
            _options = options.Value;
        }
        
        public string[] GetAllVolumes()
        {
            return Directory.GetDirectories(_options.VolumesFolder);
        }

        public string[] GetAllFiles(string volume)
        {
            return Directory.GetFiles(volume, "*", SearchOption.AllDirectories);
        }

        public Stream OpenFile(string file)
        {
            return File.OpenRead(file);
        }

        public Stream WriteFile(string file)
        {
            return File.Create(file);
        }

        public void DeleteIfExists(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        public bool IsFileLocked(string file)
        {
            FileStream stream = null;

            try
            {
                stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                stream.ReadByte();
                return false;
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                stream?.Close();
                stream?.Dispose();
            }
        }
    }
}