using System;

namespace AzureDockerVolumeBackup.Calendar
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}