// <copyright file="CleanupFinishedTasks.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Tasks;
using Quartz;
using Quartz.Impl.Matchers;

namespace KapitelShelf.Api.Tasks.Maintenance;

/// <summary>
/// Remove finished jobs and restart error jobs.
/// </summary>
public class CleanupFinishedTasks(ITaskRuntimeDataStore dataStore, ILogger<TaskBase> logger, ISchedulerFactory schedulerFactory) : TaskBase(dataStore, logger)
{
    private readonly ISchedulerFactory schedulerFactory = schedulerFactory;

    /// <inheritdoc/>
    public override async Task ExecuteTask(IJobExecutionContext context)
    {
        var scheduler = await schedulerFactory.GetScheduler();

        var jobsToCheck = new List<JobKey>();

        var groups = await scheduler.GetTriggerGroupNames();
        foreach (var group in groups)
        {
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group));
            jobsToCheck.AddRange(jobKeys);
        }

        var i = 0;
        foreach (var jobKey in jobsToCheck)
        {
            this.CheckForInterrupt(context);

            // increment counter for task progress
            i++;

            var triggers = await scheduler.GetTriggersOfJob(jobKey);

            // Check if all triggers are in 'Complete' state
            bool allTriggersComplete = true;
            foreach (var trigger in triggers)
            {
                var state = await scheduler.GetTriggerState(trigger.Key);

                // check if all triggers of this job are completed
                if (state != TriggerState.Complete)
                {
                    allTriggersComplete = false;
                }

                // reset the trigger if it is currently in Error
                if (state == TriggerState.Error)
                {
                    await scheduler.ResetTriggerFromErrorState(trigger.Key);
                }
            }

            // Delete the job if all triggers completed
            if (allTriggersComplete)
            {
                await scheduler.DeleteJob(jobKey);
            }

            // notify job progress
            this.DataStore.SetProgress(JobKey(context), i, groups.Count);
        }
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

        var internalTask = new InternalTask<CleanupFinishedTasks>
        {
            Title = "Cleanup Finished Tasks",
            Category = "Maintenance",
            Description = "Restart failed tasks and delete completed ones.",
            ShouldRecover = true,
            StartNow = true,
            Cronjob = "0 */15 * ? * *", // every 15 minutes
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
