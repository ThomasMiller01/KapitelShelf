// <copyright file="RemoveStorageData.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Logic.Interfaces.CloudStorages;
using Quartz;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Tasks.CloudStorage;

/// <summary>
/// Deletes all the local data of a cloud storage.
/// </summary>
public class RemoveStorageData(ITaskRuntimeDataStore dataStore, ILogger<TaskBase> logger, ICloudStoragesLogic logic) : TaskBase(dataStore, logger)
{
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1401 // Fields should be private
    internal IJobExecutionContext? executionContext = null;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter

    private readonly ICloudStoragesLogic logic = logic;

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
    // this.fileStorage.DeleteDirectory(this.mapper.CloudStorageModelToCloudStorageDto(storage));
    public override async Task ExecuteTask(IJobExecutionContext context)
    {
        if (!Enum.TryParse(this.StorageType, out CloudTypeDTO type))
        {
            return;
        }

        // set context for onFileDelete callback
        this.executionContext = context;

        var partialStorage = new CloudStorageDTO
        {
            CloudOwnerEmail = StorageOwnerEmail,
            Type = type,
        };
        this.logic.DeleteStorageData(partialStorage, this.RemoveOnlyCloudData, onFileDelete: this.OnFileDelete);

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

    internal void OnFileDelete(string filePath, int totalFiles, int fileIndex)
    {
        ArgumentNullException.ThrowIfNull(this.executionContext);

        this.CheckForInterrupt(this.executionContext);
        this.DataStore.SetProgress(JobKey(this.executionContext), fileIndex, totalFiles);
    }
}
