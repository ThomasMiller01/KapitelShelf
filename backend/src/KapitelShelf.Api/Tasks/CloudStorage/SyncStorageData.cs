// <copyright file="SyncStorageData.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Runtime.CompilerServices;
using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Logic.CloudStorages;
using KapitelShelf.Api.Utils;
using Quartz;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Tasks.CloudStorage;

/// <summary>
/// Initially download data from a cloud storage.
/// </summary>
[DisallowConcurrentExecution]
public partial class SyncStorageData(
    ITaskRuntimeDataStore dataStore,
    ILogger<TaskBase> logger,
    ICloudStoragesLogic logic,
    IMapper mapper) : TaskBase(dataStore, logger)
{
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1401 // Fields should be private
    internal IProcess? rcloneProcess = null;

    internal IJobExecutionContext? executionContext = null;

    internal int currentStorageIndex = 0;
    internal int totalStorageIndex = 0;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter

    private readonly ICloudStoragesLogic logic = logic;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Sets a value to only sync a single storage.
    /// </summary>
    public Guid? ForSingleStorageId { private get; set; }

    /// <inheritdoc/>
    public override async Task ExecuteTask(IJobExecutionContext context)
    {
        this.executionContext = context;

        if (this.ForSingleStorageId.HasValue)
        {
            // sync a single storage
            await this.SyncSingleStorage(context);
            return;
        }

        // sync all storages
        await this.SyncAllStorages(context);
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
    /// <param name="options">The task schedule options.</param>
    /// <param name="forSingleStorage">Sync only for a single storage.</param>
    /// <returns>The job key.</returns>
    public static async Task<string> Schedule(IScheduler scheduler, TaskScheduleOptionsDTO? options = null, CloudStorageDTO? forSingleStorage = null)
    {
        ArgumentNullException.ThrowIfNull(scheduler);

        var internalTask = new InternalTask<SyncStorageData>
        {
            Title = "Sync Cloud Storages",
            Category = "Cloud Storage",
            Description = "Sync cloud data for configured storages",
            Cronjob = "0 */5 * ? * *",
        };

        var jobDetail = internalTask.JobDetail;

        // sync a single storage
        if (forSingleStorage is not null)
        {
            // update internal task
            internalTask.Title = $"Sync Cloud Storage for {forSingleStorage.Type}";
            internalTask.Description = $"Sync cloud data for '{forSingleStorage.CloudDirectory}'";

            // start immediately, no cronjob
            internalTask.StartNow = true;
            internalTask.Cronjob = null;

            jobDetail = internalTask.JobDetail
                .UsingJobData("ForSingleStorageId", forSingleStorage.Id.ToString());
        }

        var job = jobDetail.Build();

        var trigger = internalTask.Trigger
            .Build();

        await PreScheduleSteps(scheduler, job, options);

        await scheduler.ScheduleJob(job, [trigger], replace: true);

        await PostScheduleSteps(scheduler, job, options);

        return job.Key.ToString();
    }

    /// <summary>
    /// Sync a single storage.
    /// </summary>
    /// <param name="context">The job context.</param>
    /// <returns>A task.</returns>
    /// <exception cref="ArgumentNullException">The single storage id is null.</exception>
    internal async Task SyncSingleStorage(IJobExecutionContext context)
    {
        if (!ForSingleStorageId.HasValue)
        {
            throw new ArgumentNullException(nameof(this.ForSingleStorageId));
        }

        var storageModel = await this.logic.GetStorageModel(this.ForSingleStorageId.Value);
        if (storageModel is null)
        {
            // storage already deleted
            return;
        }

        this.currentStorageIndex = 0;
        this.totalStorageIndex = 1;

        this.DataStore.SetMessage(JobKey(context), $"Synching '{storageModel.CloudDirectory}' [{storageModel.Type}]");

        // download new data
        var storage = this.mapper.Map<CloudStorageDTO>(storageModel);
        await this.logic.SyncStorage(storage, this.DownloadProgressHandler, this.OnProcessStarted);
    }

    /// <summary>
    /// Sync all storages.
    /// </summary>
    /// <param name="context">The job context.</param>
    /// <returns>A task.</returns>
    internal async Task SyncAllStorages(IJobExecutionContext context)
    {
        var storages = await this.logic.GetDownloadedStorageModels();
        if (storages.Count == 0)
        {
            // no storages to sync
            return;
        }

        this.totalStorageIndex = storages.Count;

        for (int i = 0; i < storages.Count; i++)
        {
            var storageModel = storages[i];

            this.CheckForInterrupt(context);
            this.DataStore.SetMessage(JobKey(context), $"Synching '{storageModel.CloudDirectory}' [{storageModel.Type}]");

            // download new data
            var storage = this.mapper.Map<CloudStorageDTO>(storageModel);
            await this.logic.SyncStorage(storage, this.DownloadProgressHandler, this.OnProcessStarted);

            this.currentStorageIndex = i;
        }
    }

    /// <summary>
    /// Set the progress based on the rclone output during download.
    /// </summary>
    /// <param name="data">The stdout data.</param>
    internal void DownloadProgressHandler(string data)
    {
        ArgumentNullException.ThrowIfNull(this.executionContext);

        if (!RCloneUtils.TryParseStats(data, out var stats))
        {
            // ignore non-stats output
            return;
        }

        if (stats.Percent is not null)
        {
            this.DataStore.SetProgress(JobKey(this.executionContext), this.currentStorageIndex, this.totalStorageIndex, (int)stats.Percent);
        }

        if (stats.Speed is not null || stats.Eta is not null)
        {
            var speedText = RCloneUtils.FormatSpeed(stats.Speed);
            var etaText = RCloneUtils.FormatEta(stats.Eta);

            var message = $"{speedText}, ETA {etaText}";
            this.DataStore.SetMessage(JobKey(this.executionContext), message);
        }
    }

    private void OnProcessStarted(Process process) => this.rcloneProcess = new ProcessWrapper(process);
}
