// <copyright file="SyncStorageData.cs" company="KapitelShelf">
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
public partial class SyncStorageData(TaskRuntimeDataStore dataStore, ILogger<TaskBase> logger, ICloudStorage fileStorage, CloudStoragesLogic logic, IMapper mapper, KapitelShelfSettings settings) : TaskBase(dataStore, logger)
{
    private readonly ILogger<TaskBase> logger = logger;

    private readonly ICloudStorage fileStorage = fileStorage;

    private readonly CloudStoragesLogic logic = logic;

    private readonly IMapper mapper = mapper;

    private readonly KapitelShelfSettings settings = settings;

    private IJobExecutionContext? executionContext = null;

    private Process? rcloneProcess = null;

    private double progressBase = 0;
    private double progressIncrement = 0;

    /// <inheritdoc/>
    public override async Task ExecuteTask(IJobExecutionContext context)
    {
        var storages = await this.logic.GetDownloadedStorageModels();

        this.progressIncrement = 100 / storages.Count;
        this.executionContext = context;

        var i = 0;
        foreach (var storageModel in storages)
        {
            this.CheckForInterrupt(context);

            // increment counter for task progress
            i++;

            this.DataStore.SetMessage(JobKey(context), $"Synching '{storageModel.CloudDirectory}' [{storageModel.Type}]");

            // download new data
            var storage = this.mapper.Map<CloudStorageDTO>(storageModel);
            var localPath = this.fileStorage.FullPath(storage, StaticConstants.CloudStorageCloudDataSubPath);

            if (StaticConstants.StoragesSupportRCloneBisync.Contains(storage.Type))
            {
                // use rclone bisync
                await storageModel.ExecuteRCloneCommand(
                    this.settings.CloudStorage.RClone,
                    [
                        "bisync",
                    $"\"{StaticConstants.CloudStorageRCloneConfigName}:{storage.CloudDirectory}\"",
                    $"\"{localPath}\"",
                    "--resilient",
                    "--recover",
                    "--conflict-resolve=newer",
                    "--max-lock=2m",
                    "--progress",
                    "--stats=1s",
                    "--stats-one-line"
                    ],
                    onStdout: this.DownloadProgressHandler,
                    stdoutSeperator: "xfr", // rclone transfer number
                    onProcessStarted: p => this.rcloneProcess = p,
                    cancellationToken: context.CancellationToken);
            }
            else
            {
                // use rclone clone
                await storageModel.ExecuteRCloneCommand(
                    this.settings.CloudStorage.RClone,
                    [
                        "sync",
                        $"\"{StaticConstants.CloudStorageRCloneConfigName}:{storage.CloudDirectory}\"",
                        $"\"{localPath}\"",
                        "--progress",
                        "--stats=1s",
                        "--stats-one-line"
                    ],
                    onStdout: this.DownloadProgressHandler,
                    stdoutSeperator: "xfr", // rclone transfer number
                    onProcessStarted: p => this.rcloneProcess = p,
                    cancellationToken: context.CancellationToken);
            }

            this.progressBase += this.progressIncrement;
        }
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
    /// <param name="options">The task schedule options.</param>
    /// <returns>The job key.</returns>
    public static async Task<string> Schedule(IScheduler scheduler, TaskScheduleOptionsDTO? options = null)
    {
        ArgumentNullException.ThrowIfNull(scheduler);

        var job = JobBuilder.Create<SyncStorageData>()
            .WithIdentity($"Sync Cloud Storages", "Cloud Storage")
            .WithDescription($"Sync cloud data for configured storages")
            .RequestRecovery() // re-execute after possible hard-shutdown
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"Sync Cloud Storages", "Cloud Storage")
            .StartNow()
            .WithCronSchedule("0 */5 * ? * *")
            .Build();

        await PreScheduleSteps(scheduler, job, options);

        await scheduler.ScheduleJob(job, [trigger], replace: true);

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

        // filter for correct line
        // e.g. "86.786 MiB / 1.187 GiB, 7%, 7.005 MiB/s, ETA 2m41s"
        if (!data.Contains("ETA"))
        {
            return;
        }

        // extract progress
        var percentMatch = MatchProgressPercentage().Match(data);
        if (percentMatch.Success)
        {
            if (int.TryParse(percentMatch.Groups[1].Value, out int percentage))
            {
                // Calculate global progress:
                // - Each storage is mapped to 'progressIncrement' percent of total bar.
                // - For the current storage, percentage is from 0 to 100%.
                // - So progress for this storage is: progressBase + percentage * (progressIncrement / 100.0)
                // - Example: progressBase = 40, progressIncrement = 20, percentage = 50
                //   => progress = 40 + (50 * 0.2) = 50
                int progress = (int)Math.Round(this.progressBase + (percentage / 100.0 * (this.progressIncrement / 100.0)));
                this.DataStore.SetProgress(JobKey(this.executionContext), progress);
            }
        }

        // transfer speed
        string? speed = null;
        var speedMatch = MatchSpeed().Match(data);
        if (speedMatch.Success)
        {
            speed = speedMatch.Groups[1].Value;
        }

        // estimated-time-of-arrival
        string? eta = null;
        var etaMatch = MatchETA().Match(data);
        if (etaMatch.Success)
        {
            eta = etaMatch.Groups[1].Value;
        }

        if (speed is not null || eta is not null)
        {
            var message = $"{speed}, ETA {eta}";
            this.DataStore.SetMessage(JobKey(executionContext), message);
        }
    }

    [GeneratedRegex(@"(\d+)%")]
    private static partial Regex MatchProgressPercentage();

    [GeneratedRegex(@"([\d.]+ [KMG]{0,1}i{0,1}B/s)")]
    private static partial Regex MatchSpeed();

    [GeneratedRegex(@"ETA ([\w\d:]+)")]
    private static partial Regex MatchETA();
}
