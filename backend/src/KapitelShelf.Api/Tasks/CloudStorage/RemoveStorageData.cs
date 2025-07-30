// <copyright file="RemoveStorageData.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Logic.Storage;
using KapitelShelf.Api.Settings;
using Quartz;

namespace KapitelShelf.Api.Tasks.CloudStorage;

/// <summary>
/// Deletes all the local data of a cloud storage.
/// </summary>
public class RemoveStorageData(ITaskRuntimeDataStore dataStore, ILogger<TaskBase> logger, ICloudStorage fileStorage) : TaskBase(dataStore, logger)
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
        if (!Directory.Exists(storagePath))
        {
            // directory was already deleted
            return;
        }

        var files = Directory.EnumerateFiles(storagePath, "*", SearchOption.AllDirectories);

        int totalFiles = files.Count();
        int i = 0;
        foreach (var file in files)
        {
            this.CheckForInterrupt(context);

            // increment counter for task progress
            i++;

            try
            {
                // try to delete the file
                File.Delete(file);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Could not delete file '{File}' in cloud storage", file);
            }

            this.DataStore.SetProgress(JobKey(context), i, totalFiles);
        }

        // try to delete the directory itself
        if (!Directory.Exists(storagePath))
        {
            try
            {
                Directory.Delete(storagePath);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Could not delete directory '{Directory}' in cloud storage", storagePath);
            }
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task Kill() => await Task.CompletedTask;

    /// <summary>
    /// Schedule this task.
    /// </summary>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="storage">The storage.</param>
    /// <param name="removeOnlyCloudData">Remove only the cloud data and leave config.</param>
    /// <param name="options">The task schedule options.</param>
    /// <returns>The job key.</returns>
    public static async Task<string> Schedule(IScheduler scheduler, CloudStorageDTO storage, bool removeOnlyCloudData = false, TaskScheduleOptionsDTO? options = null)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(storage);

        var internalTask = new InternalTask<RemoveStorageData>
        {
            Title = $"Removing local cloud data of {storage.Type}",
            Category = "Cloud Storage",
            Description = $"Delete local data of '{storage.CloudOwnerName}'",
            ShouldRecover = true,
            StartNow = true,
        };

        var job = internalTask.JobDetail
            .UsingJobData("StorageOwnerEmail", storage.CloudOwnerEmail)
            .UsingJobData("StorageType", storage.Type.ToString())
            .UsingJobData("RemoveOnlyCloudData", removeOnlyCloudData)
            .Build();

        var trigger = internalTask.Trigger
            .Build();

        await PreScheduleSteps(scheduler, job, options);

        await scheduler.ScheduleJob(job, trigger);

        await PostScheduleSteps(scheduler, job, options);

        return job.Key.ToString();
    }
}
