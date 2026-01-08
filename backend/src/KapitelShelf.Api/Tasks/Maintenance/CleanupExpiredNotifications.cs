// <copyright file="CleanupExpiredNotifications.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Logic.Interfaces;
using Quartz;

namespace KapitelShelf.Api.Tasks.Maintenance;

/// <summary>
/// Removes all expired notifications.
/// </summary>
public class CleanupExpiredNotifications(
    ITaskRuntimeDataStore dataStore,
    ILogger<TaskBase> logger,
    INotificationsLogic notifications) : TaskBase(dataStore, logger, notifications)
{
    /// <inheritdoc/>
    public override async Task ExecuteTask(IJobExecutionContext context) => await this.Notifications.DeleteExpiredNotificationsAsync();

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

        var internalTask = new InternalTask<CleanupExpiredNotifications>
        {
            Title = "Cleanup expired notifications",
            Category = "Maintenance",
            Description = "Removes all expired notifications.",
            ShouldRecover = true,
            Cronjob = "0 0 * ? * *", // every hour
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
