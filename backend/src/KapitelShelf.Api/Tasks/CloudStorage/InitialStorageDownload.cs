// <copyright file="InitialStorageDownload.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Quartz;

namespace KapitelShelf.Api.Tasks.CloudStorage;

/// <summary>
/// Initially download data from a cloud storage.
/// </summary>
[DisallowConcurrentExecution]
public class InitialStorageDownload(TaskRuntimeDataStore dataStore, ILogger<TaskBase> logger) : TaskBase(dataStore, logger)
{
    /// <inheritdoc/>
    public override async Task ExecuteTask(IJobExecutionContext context) => await Task.CompletedTask;
}
