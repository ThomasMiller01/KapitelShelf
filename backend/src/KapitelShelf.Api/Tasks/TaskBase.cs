// <copyright file="TaskBase.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Quartz;

namespace KapitelShelf.Api.Tasks;

/// <summary>
/// The base class for all tasks to inherit from.
/// </summary>
public abstract class TaskBase(TaskRuntimeDataStore dataStore, ILogger<TaskBase> logger) : IJob
{
#pragma warning disable SA1401 // Fields should be private
    internal readonly ILogger<TaskBase> Logger = logger;

    internal readonly TaskRuntimeDataStore DataStore = dataStore;
#pragma warning restore SA1401 // Fields should be private

    /// <inheritdoc/>
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(context);

            this.DataStore.SetProgress(this.JobKey(context), 0);

            await this.ExecuteTask(context);

            this.DataStore.ClearProgress(this.JobKey(context));
        }
        catch (JobExecutionException)
        {
            // rethrow job execution exception
            throw;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error during task execution");
        }
    }

    /// <summary>
    /// The execute method for the task.
    /// </summary>
    /// <param name="context">The task context.</param>
    /// <returns>A task.</returns>
    public abstract Task ExecuteTask(IJobExecutionContext context);

    /// <summary>
    /// Get the job key for a task.
    /// </summary>
    /// <param name="context">The job context.</param>
    /// <returns>The job key.</returns>
    public string JobKey(IJobExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.JobDetail.Key.ToString();
    }
}
