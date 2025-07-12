// <copyright file="DummyTask.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Quartz;

namespace KapitelShelf.Api.Tasks;

/// <summary>
/// A dummy task for developemnt.
/// </summary>
/// <remarks>REMOVE BEFORE NEXT RELEASE!.</remarks>
[Obsolete("REMOVE BEFORE NEXT RELEASE!")]
public class DummyTask(TaskRuntimeDataStore dataStore, ILogger<TaskBase> logger) : TaskBase(dataStore, logger)
{
    /// <inheritdoc/>
    public override async Task ExecuteTask(IJobExecutionContext context)
    {
        // example: simulate work in steps, reporting progress
        for (int i = 1; i <= 5; i++)
        {
            this.Logger.LogInformation("Task Progress: {Progress}", i * 20);

            // simulate work
            await Task.Delay(5000);

            // set progress (as percent)
            this.DataStore.SetProgress(this.JobKey(context), i * 20);
        }
    }
}
