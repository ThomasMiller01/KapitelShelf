// <copyright file="CleanupFinishedTasks.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Quartz;
using Quartz.Impl.Matchers;

namespace KapitelShelf.Api.Tasks.Maintenance;

/// <summary>
/// Remove finished jobs and restart error jobs.
/// </summary>
public class CleanupFinishedTasks(TaskRuntimeDataStore dataStore, ILogger<TaskBase> logger, ISchedulerFactory schedulerFactory) : TaskBase(dataStore, logger)
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
}
