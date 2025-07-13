// <copyright file="ErrorTask.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Quartz;

namespace KapitelShelf.Api.Tasks;

/// <summary>
/// A dummy task for developemnt.
/// </summary>
/// <remarks>REMOVE BEFORE NEXT RELEASE!.</remarks>
[Obsolete("REMOVE BEFORE NEXT RELEASE!")]
public class ErrorTask(TaskRuntimeDataStore dataStore, ILogger<TaskBase> logger) : TaskBase(dataStore, logger)
{
    /// <inheritdoc/>
    public override async Task ExecuteTask(IJobExecutionContext context)
    {
        await Task.CompletedTask;

        throw new NotImplementedException();
    }
}
