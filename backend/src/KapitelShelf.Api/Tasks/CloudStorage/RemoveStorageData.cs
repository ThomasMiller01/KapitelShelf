// <copyright file="RemoveStorageData.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Logic.Storage;
using KapitelShelf.Api.Settings;
using Quartz;

namespace KapitelShelf.Api.Tasks.CloudStorage;

/// <summary>
/// Deletes all the local data of a cloud storage.
/// </summary>
[DisallowConcurrentExecution]
public class RemoveStorageData(TaskRuntimeDataStore dataStore, ILogger<TaskBase> logger, ICloudStorage fileStorage) : TaskBase(dataStore, logger)
{
    private readonly ICloudStorage fileStorage = fileStorage;

    /// <summary>
    /// Sets the storage owner email.
    /// </summary>
    public string StorageOwnerEmail { private get; set; } = null!;

    /// <summary>
    /// Sets the storage type.
    /// </summary>
    public string StorageType { private get; set; } = null!;

    /// <summary>
    /// Sets a value indicating whether to only delete cloud data.
    /// </summary>
    public bool RemoveOnlyCloudData { private get; set; }

    /// <inheritdoc/>
    // this.fileStorage.DeleteDirectory(this.mapper.Map<CloudStorageDTO>(storage));
    public override async Task ExecuteTask(IJobExecutionContext context)
    {
        if (!Enum.TryParse(this.StorageType, out CloudTypeDTO type))
        {
            return;
        }

        var partialStorage = new CloudStorageDTO
        {
            CloudOwnerEmail = StorageOwnerEmail,
            Type = type,
        };
        var storagePath = this.fileStorage.FullPath(partialStorage, this.RemoveOnlyCloudData ? StaticConstants.CloudStorageCloudDataSubPath : string.Empty);

        var files = Directory.EnumerateFiles(storagePath, "*", SearchOption.AllDirectories);

        int totalFiles = files.Count();
        int i = 0;
        foreach (var file in files)
        {
            // increment counter for task progress
            i++;

            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Could not delete file '{File}' in cloud storage", file);
            }

            this.DataStore.SetProgress(JobKey(context), i, totalFiles);
        }

        Directory.Delete(storagePath, true);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Schedule this task.
    /// </summary>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="storage">The storage.</param>
    /// <param name="removeOnlyCloudData">Remove only the cloud data and leave config.</param>
    /// <param name="waitForFinish">Wait for the task to finish.</param>
    /// <returns>The job key.</returns>
    public static async Task<string> Schedule(IScheduler scheduler, CloudStorageDTO storage, bool removeOnlyCloudData = false, bool waitForFinish = false)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(storage);

        var job = JobBuilder.Create<RemoveStorageData>()
            .WithIdentity($"Removing local '{storage.CloudOwnerName}' of {storage.Type}", "Cloud Storage")
            .UsingJobData("StorageOwnerEmail", storage.CloudOwnerEmail)
            .UsingJobData("StorageType", storage.Type.ToString())
            .UsingJobData("RemoveOnlyCloudData", removeOnlyCloudData)
            .RequestRecovery() // re-execute after possible hard-shutdown
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"Removing local '{storage.CloudOwnerName}' of {storage.Type}", "Cloud Storage")
            .StartNow()
            .Build();

        // set the listener, if this method should wait for the task to finish
        WaitForTaskListener? listener = null;
        if (waitForFinish)
        {
            listener = new WaitForTaskListener(job.Key.ToString());
            scheduler.ListenerManager.AddJobListener(listener);
        }

        await scheduler.ScheduleJob(job, trigger);

        // wait for the task to finish, if the listener is set
        if (listener is not null)
        {
            await listener.WaitAsync();
        }

        return job.Key.ToString();
    }
}
