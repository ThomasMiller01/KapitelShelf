// <copyright file="ConfigureProvider.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Logic.Interfaces;
using Quartz;

namespace KapitelShelf.Api.Tasks.Ai;

/// <summary>
/// Configures the currently selected ai provider.
/// </summary>
public class ConfigureProvider(
    ITaskRuntimeDataStore dataStore,
    ILogger<TaskBase> logger,
    INotificationsLogic notifications,
    IAiManager aiManager) : TaskBase(dataStore, logger, notifications)
{
    private readonly IAiManager aiManager = aiManager;

    /// <inheritdoc/>
    public override async Task ExecuteTask(IJobExecutionContext context)
    {
        var progress = new Progress<int>(p =>
        {
            this.DataStore.SetProgress(JobKey(context), p);
        });

        await this.aiManager.ConfigureCurrentProvider(progress);
    }

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

        var internalTask = new InternalTask<ConfigureProvider>
        {
            Title = "Configure Provider",
            Category = "Ai",
            Description = "Configures the currently selected Ai provider",
            StartNow = true,
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
