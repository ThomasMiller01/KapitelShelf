// <copyright file="CleanupDatabase.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Logic;
using Quartz;

namespace KapitelShelf.Api.Tasks.Maintenance;

/// <summary>
/// Removes all orphanes database entires as well as associated files.
/// </summary>
public class CleanupDatabase(ITaskRuntimeDataStore dataStore, ILogger<TaskBase> logger, IBooksLogic logic) : TaskBase(dataStore, logger)
{
    private readonly IBooksLogic logic = logic;

    /// <inheritdoc/>
    public override async Task ExecuteTask(IJobExecutionContext context) => await this.logic.CleanupDatabase();

    /// <inheritdoc/>
    public override async Task Kill() => await Task.CompletedTask;

    /// <summary>
    /// Schedule this task.
    /// </summary>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="options">The task schedule options.</param>
    /// <returns>The job key.</returns>
    public static async Task<string> Schedule(IScheduler scheduler, TaskScheduleOptionsDTO? options = null)
    {
        ArgumentNullException.ThrowIfNull(scheduler);

        var internalTask = new InternalTask<CleanupFinishedTasks>
        {
            Title = "Cleanup Database",
            Category = "Maintenance",
            Description = "Removes all orphanes database entires as well as associated files.",
            ShouldRecover = true,
            StartNow = true,
            Cronjob = "0 */10 * ? * *", // every 10 minutes
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
}
