// <copyright file="RemoveStorageData.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Logic.Storage;
using Quartz;

namespace KapitelShelf.Api.Tasks.CloudStorage;

/// <summary>
/// Deletes all the local data of a cloud storage.
/// </summary>
[DisallowConcurrentExecution]
public class RemoveStorageData(TaskRuntimeDataStore dataStore, ILogger<TaskBase> logger, ICloudStorage fileStorage) : TaskBase(dataStore, logger)
{
    private readonly ICloudStorage fileStorage = fileStorage;

    /// <inheritdoc/>
    // this.fileStorage.DeleteDirectory(this.mapper.Map<CloudStorageDTO>(storage));
    public override async Task ExecuteTask(IJobExecutionContext context)
    {
        CloudStorageDTO storage = null!;
        var storagePath = this.fileStorage.FullPath(storage);

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

        await Task.CompletedTask;
    }
}
