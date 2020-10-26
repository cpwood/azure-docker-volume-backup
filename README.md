# azure-docker-volume-backup
Performs backups of Docker volumes to Azure Blob Storage.

## Background
Key points:

1. Mounted volumes are the recommended approach of storing files outside of a running container.
2. Multiple Docker containers can share the same named volume.
3. By mounting volumes under `/volumes` on a `azure-docker-volume-backup` container, the application will create a `.tar.gz` for each volume and upload it to Azure Blob Storage. The filename will start with a date and time stamp and end with the volume name, e.g. `2020-10-22 23-12-23 MyVolume.tar.gz`

## Configuration
The following environment variables allow you to configure the software:

* `ACCOUNT` (required) - the name of the Azure Blob Storage account (without the `core.windows.net` suffix);
* `KEY` (required) - the access key;
* `CONTAINER` - the name of the **blob** container (defaults to `docker`);
* `SCHEDULE` - a [Cron-expression](https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/crontriggers.html#cron-expressions) that controls the timing and frequency of backup runs (defaults to 2am every morning - `0 0 2 * * ?`);
* `DAYSTOKEEP` - an integer value (defaults to keeping the last `7` days of backups).

You must also mount the volumes you want to back up under `/volumes`.

```
docker run --name azure-docker-volume-backup \
    --mount source=my-vol,destination=/volumes/my-vol \
    -e ACCOUNT=foo \
    -e KEY=sfh9sh0shsf0h0sfh \
    -e CONTAINER=docker \
    cpwood/azure-docker-volume-backup:latest
```

## Restoring a Backup

Restoring the data requires no special software and takes only a couple of minutes.

1. Find the backup file you want to restore using [Azure Storage Explorer](https://azure.microsoft.com/en-gb/features/storage-explorer/).
2. Right-click on the file and choose **Get Shared Access Signature**. Then click **Create**.
3. Click the **Copy** button next to the **URI** field.
4. Start up an Alpine shell and mount the volume you want to restore to: `docker run -it --name data-restore --mount source=my-vol,destination=/my-vol alpine /bin/sh` .
5. From the shell, download the backup: `wget -O backup.tar.gz "<url-you-copied>"` (the quotes around the URL are important).
6. Untar the backup to the volume you mounted: `tar -xf backup.tar.gz -C /my-vol`.
7. Use `Ctrl + D` to exit the shell.
8. Delete the container: `docker rm -f data-restore` .