// <copyright file="ScanForBooks.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.CloudStorages;
using KapitelShelf.Api.Logic.Storage;
using KapitelShelf.Api.Settings;
using Quartz;

namespace KapitelShelf.Api.Tasks.CloudStorage;

/// <summary>
/// Scan the cloud storages for books.
/// </summary>
[DisallowConcurrentExecution]
public class ScanForBooks(ITaskRuntimeDataStore dataStore, ILogger<TaskBase> logger, ICloudStorage fileStorage, CloudStoragesLogic logic, IMapper mapper, IBooksLogic booksLogic, IBookParserManager bookParserManager) : TaskBase(dataStore, logger)
{
    private readonly ICloudStorage fileStorage = fileStorage;

    private readonly CloudStoragesLogic logic = logic;

    private readonly IMapper mapper = mapper;

    private readonly IBooksLogic booksLogic = booksLogic;

    private readonly IBookParserManager bookParserManager = bookParserManager;

    /// <inheritdoc/>
    public override async Task ExecuteTask(IJobExecutionContext context)
    {
        var storages = await this.logic.GetDownloadedStorageModels();
        if (storages.Count == 0)
        {
            // no storages to sync
            return;
        }

        var progressIncrement = 100 / storages.Count;
        var progressBase = 0;

        foreach (var storageModel in storages)
        {
            this.CheckForInterrupt(context);

            this.DataStore.SetMessage(JobKey(context), $"Scanning '{storageModel.CloudDirectory}' [{storageModel.Type}]");

            // download new data
            var storage = this.mapper.Map<CloudStorageDTO>(storageModel);
            var localPath = this.fileStorage.FullPath(storage, StaticConstants.CloudStorageCloudDataSubPath);

            var files = new List<string>();
            foreach (var extension in this.bookParserManager.SupportedFileEndings())
            {
                files.AddRange(Directory.EnumerateFiles(localPath, $"*.{extension}", SearchOption.AllDirectories));
            }

            int totalFiles = files.Count;

            int i = 0;
            foreach (var filePath in files)
            {
                this.CheckForInterrupt(context);

                // increment counter for task progress
                i++;

                var file = filePath.ToFile();

                var fileImported = await this.booksLogic.BookFileExists(file);
                var fileImportFailedBefore = await this.logic.CloudFileImportFailed(storage, file);
                if (fileImported || fileImportFailedBefore)
                {
                    // ignore file
                    continue;
                }

                try
                {
                    // import book
                    await this.booksLogic.ImportBookAsync(file);
                }
                catch (Exception ex)
                {
                    await this.logic.AddCloudFileImportFail(storage, file.ToFileInfo(filePath), ex.Message);
                }

                // Calculate global progress:
                // - Each storage is mapped to 'progressIncrement' percent of total bar.
                // - For the current storage, percentage is from 0 to 100%.
                // - So progress for this storage is: progressBase + percentage * (progressIncrement / 100.0)
                // - Example: progressBase = 40, progressIncrement = 20, percentage = 50
                //   => progress = 40 + (50 * 0.2) = 50
                int progress = (int)Math.Round(progressBase + ((double)i / totalFiles * progressIncrement));
                this.DataStore.SetProgress(JobKey(context), progress);
            }

            progressBase += progressIncrement;
        }
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

        var job = JobBuilder.Create<ScanForBooks>()
            .WithIdentity("Scan Cloud Storages for Books", "Cloud Storage")
            .WithDescription("Scan the cloud storages for new books to import")
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity("Scan Cloud Storages for Books", "Cloud Storage")
            .WithCronSchedule("0 3/5 * ? * *") // execution time shifted from sync storages to not interfere
            .Build();

        await PreScheduleSteps(scheduler, job, options);

        await scheduler.ScheduleJob(job, [trigger], replace: true);

        await PostScheduleSteps(scheduler, job, options);

        return job.Key.ToString();
    }

    /// <inheritdoc/>
    public override async Task Kill() => await Task.CompletedTask;
}
