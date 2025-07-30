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
public class ScanForBooks(ITaskRuntimeDataStore dataStore, ILogger<TaskBase> logger, ICloudStorage fileStorage, ICloudStoragesLogic logic, IMapper mapper, IBooksLogic booksLogic, IBookParserManager bookParserManager) : TaskBase(dataStore, logger)
{
    private readonly ICloudStorage fileStorage = fileStorage;

    private readonly ICloudStoragesLogic logic = logic;

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

        var i = 0;
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

            int j = 0;
            foreach (var filePath in files)
            {
                this.CheckForInterrupt(context);

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

                var itemPercentage = (int)Math.Floor((double)j / files.Count * 100);
                this.DataStore.SetProgress(JobKey(context), i, storages.Count, itemPercentage);

                j++;
            }

            i++;
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

        var internalTask = new InternalTask<ScanForBooks>
        {
            Title = "Scan Cloud Storages for Books",
            Category = "Cloud Storage",
            Description = "Scan the cloud storages for new books to import",
            Cronjob = "0 3/5 * ? * *", // execution time shifted from sync storages to not interfere
        };

        var job = internalTask.JobDetail
            .Build();

        var trigger = internalTask.Trigger
            .Build();

        await PreScheduleSteps(scheduler, job, options);

        await scheduler.ScheduleJob(job, [trigger], replace: true);

        await PostScheduleSteps(scheduler, job, options);

        return job.Key.ToString();
    }

    /// <inheritdoc/>
    public override async Task Kill() => await Task.CompletedTask;
}
