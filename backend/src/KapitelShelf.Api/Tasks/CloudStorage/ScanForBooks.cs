// <copyright file="ScanForBooks.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Logic.Interfaces.CloudStorages;
using Quartz;

namespace KapitelShelf.Api.Tasks.CloudStorage;

/// <summary>
/// Scan the cloud storages for books.
/// </summary>
[DisallowConcurrentExecution]
public class ScanForBooks(ITaskRuntimeDataStore dataStore, ILogger<TaskBase> logger, ICloudStoragesLogic logic, IMapper mapper) : TaskBase(dataStore, logger)
{
    private readonly ICloudStoragesLogic logic = logic;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Sets a value to only sync a single storage.
    /// </summary>
    public Guid? ForSingleStorageId { private get; set; }

    /// <inheritdoc/>
    public override async Task ExecuteTask(IJobExecutionContext context)
    {
        if (this.ForSingleStorageId.HasValue)
        {
            // scan a single storage
            await this.ScanSingleStorage(context);
            return;
        }

        // scan all storages
        await this.ScanAllStorages(context);
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

        var internalTask = new InternalTask<ScanForBooks>
        {
            Title = "Scan Cloud Storages for Books",
            Category = "Cloud Storage",
            Description = "Scan the cloud storages for new books to import",
            Cronjob = "0 3/5 * ? * *", // execution time shifted from sync storages to not interfere
        };

        var jobDetail = internalTask.JobDetail;

        // sync a single storage
        if (forSingleStorage is not null)
        {
            // update internal task
            internalTask.Title = $"Scan Cloud Storage of {forSingleStorage.Type}";
            internalTask.Description = $"Scan cloud data of '{forSingleStorage.CloudDirectory}' for new books to import";

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

    /// <inheritdoc/>
    public override async Task Kill() => await Task.CompletedTask;

    /// <summary>
    /// Scan a single storage for books.
    /// </summary>
    /// <param name="context">The job context.</param>
    /// <returns>A task.</returns>
    /// <exception cref="ArgumentNullException">The single storage id is null.</exception>
    internal async Task ScanSingleStorage(IJobExecutionContext context)
    {
        if (!ForSingleStorageId.HasValue)
        {
            throw new ArgumentNullException(nameof(this.ForSingleStorageId));
        }

        var storageModel = await this.logic.GetStorageModel(this.ForSingleStorageId.Value);
        if (storageModel is null)
        {
            // storage deleted
            return;
        }

        this.DataStore.SetMessage(JobKey(context), $"Scanning '{storageModel.CloudDirectory}' [{storageModel.Type}]");

        // check for interrupts and update task progress
        void OnFileScanned(int totalFiles, int fileIndex)
        {
            this.CheckForInterrupt(context);
            this.DataStore.SetProgress(JobKey(context), fileIndex, totalFiles);
        }

        // download new data
        var storage = this.mapper.Map<CloudStorageDTO>(storageModel);
        await this.logic.ScanStorageForBooks(storage, onFileScanned: OnFileScanned);
    }

    /// <summary>
    /// Scan all storages for new books to import.
    /// </summary>
    /// <param name="context">The job context.</param>
    /// <returns>A task.</returns>
    internal async Task ScanAllStorages(IJobExecutionContext context)
    {
        var storages = await this.logic.GetDownloadedStorageModels();
        if (storages.Count == 0)
        {
            // no storages to scan
            return;
        }

        for (int i = 0; i < storages.Count; i++)
        {
            var storageModel = storages[i];

            this.DataStore.SetMessage(JobKey(context), $"Scanning '{storageModel.CloudDirectory}' [{storageModel.Type}]");

            void OnFileScanned(int totalFiles, int fileIndex)
            {
                this.CheckForInterrupt(context);

                var itemPercentage = (int)Math.Floor((double)fileIndex / totalFiles * 100);
                this.DataStore.SetProgress(JobKey(context), i, storages.Count, itemPercentage);
            }

            // download new data
            var storage = this.mapper.Map<CloudStorageDTO>(storageModel);
            await this.logic.ScanStorageForBooks(storage, onFileScanned: OnFileScanned);
        }
    }
}
