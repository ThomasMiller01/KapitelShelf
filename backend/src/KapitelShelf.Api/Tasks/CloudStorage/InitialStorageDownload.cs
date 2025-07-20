// <copyright file="InitialStorageDownload.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Text.RegularExpressions;
using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.CloudStorages;
using KapitelShelf.Api.Logic.Storage;
using KapitelShelf.Api.Settings;
using Quartz;

namespace KapitelShelf.Api.Tasks.CloudStorage;

/// <summary>
/// Initially download data from a cloud storage.
/// </summary>
[DisallowConcurrentExecution]
public partial class InitialStorageDownload(TaskRuntimeDataStore dataStore, ILogger<TaskBase> logger, ICloudStorage fileStorage, CloudStoragesLogic logic, ISchedulerFactory schedulerFactory, IMapper mapper, KapitelShelfSettings settings) : TaskBase(dataStore, logger)
{
    private readonly ILogger<TaskBase> logger = logger;

    private readonly ICloudStorage fileStorage = fileStorage;

    private readonly CloudStoragesLogic logic = logic;

    private readonly ISchedulerFactory schedulerFactory = schedulerFactory;

    private readonly IMapper mapper = mapper;

    private readonly KapitelShelfSettings settings = settings;

    private IJobExecutionContext? executionContext = null;

    private Process? rcloneProcess = null;

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

        var storage = this.mapper.Map<CloudStorageDTO>(storageModel);

        // clean directory
        await this.CleanDirectory(storage);
        this.DataStore.SetProgress(JobKey(context), 20);

        // set context for progress handler
        this.executionContext = context;

        // download new data
        var localPath = this.fileStorage.FullPath(storage, StaticConstants.CloudStorageCloudDataSubPath);
        if (!Directory.Exists(localPath))
        {
            Directory.CreateDirectory(localPath);
        }

        if (StaticConstants.StoragesSupportRCloneBisync.Contains(storage.Type))
        {
            // use rclone bisync
            await storageModel.ExecuteRCloneCommand(
                this.settings.CloudStorage.RClone,
                [
                    "bisync",
                    $"\"{StaticConstants.CloudStorageRCloneConfigName}:{storage.CloudDirectory}\"",
                    $"\"{localPath}\"",
                    "--resync",
                    "--progress",
                    "--stats=1s",
                    "--stats-one-line"
                ],
                onStdout: this.DownloadProgressHandler,
                stdoutSeperator: "ETA",
                onProcessStarted: p => this.rcloneProcess = p,
                cancellationToken: context.CancellationToken);
        }
        else
        {
            // use rclone clone
            await storageModel.ExecuteRCloneCommand(
                this.settings.CloudStorage.RClone,
                [
                    "copy",
                    $"\"{StaticConstants.CloudStorageRCloneConfigName}:{storage.CloudDirectory}\"",
                    $"\"{localPath}\"",
                    "--progress",
                    "--stats=1s",
                    "--stats-one-line"
                ],
                onStdout: this.DownloadProgressHandler,
                stdoutSeperator: "ETA",
                onProcessStarted: p => this.rcloneProcess = p,
                cancellationToken: context.CancellationToken);
        }

        await this.logic.MarkStorageAsDownloaded(storage.Id);
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
            this.logger.LogError(ex, "Could not kill rclone process");
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

        var job = JobBuilder.Create<InitialStorageDownload>()
            .WithIdentity($"Downloading '{storage.CloudDirectory}' from {storage.Type}", "Cloud Storage")
            .UsingJobData("StorageId", storage.Id)
            .RequestRecovery() // re-execute after possible hard-shutdown
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"Downloading '{storage.CloudDirectory}' from {storage.Type}", "Cloud Storage")
            .StartNow()
            .Build();

        await PreScheduleSteps(scheduler, job, options);

        await scheduler.ScheduleJob(job, trigger);

        await PostScheduleSteps(scheduler, job, options);

        return job.Key.ToString();
    }

    /// <summary>
    /// Clean the storage directory if it exists.
    /// </summary>
    /// <param name="storage">The storage.</param>
    private async Task CleanDirectory(CloudStorageDTO storage)
    {
        var storagePath = this.fileStorage.FullPath(storage);

        // check if the directory is empty
        var isEmpty = !Directory.EnumerateFileSystemEntries(storagePath).Any();
        if (isEmpty)
        {
            this.logger.LogInformation("Directory is empty, nothing to do.");
            return;
        }

        // schedule task for removing cloud data
        var scheduler = await this.schedulerFactory.GetScheduler();
        await RemoveStorageData.Schedule(scheduler, storage, removeOnlyCloudData: true, options: TaskScheduleOptionsDTO.WaitPreset);
    }

    /// <summary>
    /// Set the progress based on the rclone output during download.
    /// </summary>
    /// <param name="data">The stdout data.</param>
    private void DownloadProgressHandler(string data)
    {
        ArgumentNullException.ThrowIfNull(this.executionContext);

        // filter for correct line
        // e.g. "Transferred: 86.786 MiB / 1.187 GiB, 7%, 7.005 MiB/s, ETA 2m41s"
        if (!data.Contains("ETA"))
        {
            return;
        }

        var percentMatch = MatchProgressPercentage().Match(data);
        if (percentMatch.Success)
        {
            if (int.TryParse(percentMatch.Groups[1].Value, out int percentage))
            {
                // map percentage to remaining 20-100% progress
                // map 0..100 to 20..100
                int progress = (int)Math.Round(20 + (percentage / 100.0 * 80));
                this.DataStore.SetProgress(JobKey(this.executionContext), progress);
            }
        }
    }

    [GeneratedRegex(@"(\d+)%")]
    private static partial Regex MatchProgressPercentage();
}
