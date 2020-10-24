namespace AzureDockerVolumeBackup.Settings
{
    public class Options
    {
        public string Account { get; set; }
        public string Key { get; set; }
        public string Container { get; set; }
        public string Schedule { get; set; }
        public int DaysToKeep { get; set; } = 7;
        public string VolumesFolder { get; set; }
    }
}