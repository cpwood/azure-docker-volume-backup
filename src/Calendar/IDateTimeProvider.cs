using System;

namespace AzureDockerVolumeBackup.Calendar
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}