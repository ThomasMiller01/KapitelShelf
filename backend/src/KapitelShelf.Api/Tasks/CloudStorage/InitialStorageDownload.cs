// <copyright file="InitialStorageDownload.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Logic.Interfaces.CloudStorages;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Utils;
using Quartz;

namespace KapitelShelf.Api.Tasks.CloudStorage;

/// <summary>
/// Initially download data from a cloud storage.
/// </summary>
public partial class InitialStorageDownload(
    ITaskRuntimeDataStore dataStore,
    ILogger<TaskBase> logger,
    INotificationsLogic notifications,
    ICloudStoragesLogic logic,
    Mapper mapper) : TaskBase(dataStore, logger, notifications)
{
    private readonly ICloudStoragesLogic logic = logic;

    private readonly Mapper mapper = mapper;

    private IJobExecutionContext? executionContext = null;

    private IProcess? rcloneProcess = null;

    /// <summary>
    /// Sets the storage id.
    /// </summary>
    public Guid StorageId { private get; set; }

    /// <inheritdoc/>
    public override async Task ExecuteTask(IJobExecutionContext context)
    {
        var storageModel = await this.logic.GetStorageModel(StorageId);
        if (storageModel is null)
        {
            // storage already deleted
            return;
        }

        var storage = this.mapper.CloudStorageModelToCloudStorageDto(storageModel);

        // clean directory
        await this.logic.CleanStorageDirectory(storage);
        this.DataStore.SetProgress(JobKey(context), 20);

        // set context for progress handler
        this.executionContext = context;

        // download new data
        await this.logic.DownloadStorageInitially(
            storage,
            this.DownloadProgressHandler,
            p => this.rcloneProcess = new ProcessWrapper(p),
            context.CancellationToken);
    }

    /// <inheritdoc/>
    public override async Task Kill()
    {
        if (this.rcloneProcess is null)
        {
            return;
        }

        try
        {
            if (!this.rcloneProcess.HasExited)
            {
                this.rcloneProcess.Kill(entireProcessTree: true);
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Could not kill rclone process");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Schedule this task.
    /// </summary>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="storage">The storage.</param>
    /// <param name="options">The task schedule options.</param>
    /// <returns>The job key.</returns>
    public static async Task<string> Schedule(IScheduler scheduler, CloudStorageDTO storage, TaskScheduleOptionsDTO? options = null)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(storage);

        var internalTask = new InternalTask<InitialStorageDownload>
        {
            Title = $"Downloading cloud data from {storage.Type}",
            Category = "Cloud Storage",
            Description = $"Initial download for '{storage.CloudDirectory}'",
            ShouldRecover = true,
            StartNow = true,
        };

        var job = internalTask.JobDetail
            .UsingJobData("StorageId", storage.Id.ToString())
            .Build();

        var trigger = internalTask.Trigger
            .Build();

        await PreScheduleSteps(scheduler, job, options);

        await scheduler.ScheduleJob(job, trigger);

        await PostScheduleSteps(scheduler, job, options);

        return job.Key.ToString();
    }

    /// <summary>
    /// Set the progress based on the rclone output during download.
    /// </summary>
    /// <param name="data">The stdout data.</param>
    private void DownloadProgressHandler(string data)
    {
        ArgumentNullException.ThrowIfNull(this.executionContext);

        if (!RCloneUtils.TryParseStats(data, out var stats))
        {
            // ignore non-stats output
            return;
        }

        if (stats.Percent is not null)
        {
            // map percentage to remaining 20-100% progress
            // map 0..100 to 20..100
            int progress = (int)Math.Round(20 + (stats.Percent.Value / 100 * 80));
            this.DataStore.SetProgress(JobKey(this.executionContext), progress);
        }

        if (stats.Speed is not null || stats.Eta is not null)
        {
            var speedText = RCloneUtils.FormatSpeed(stats.Speed);
            var etaText = RCloneUtils.FormatEta(stats.Eta);

            var message = $"{speedText}, ETA {etaText}";
            this.DataStore.SetMessage(JobKey(this.executionContext), message);
        }
    }
}
