// <copyright file="TasksLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Tasks;
using Quartz;
using Quartz.Impl.Matchers;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The tasks logic.
/// </summary>
public class TasksLogic(IMapper mapper, ISchedulerFactory schedulerFactory, TaskRuntimeDataStore dataStore)
{
    /// <summary>
    /// The mapper.
    /// </summary>
    private readonly IMapper mapper = mapper;

    /// <summary>
    /// The task scheduler factory.
    /// </summary>
    private readonly ISchedulerFactory schedulerFactory = schedulerFactory;

    /// <summary>
    /// The task runtime data store.
    /// </summary>
    private readonly TaskRuntimeDataStore dataStore = dataStore;

    /// <summary>
    /// Get all tasks.
    /// </summary>
    /// <returns>A list of all tasks.</returns>
    public async Task<List<TaskDTO>> GetTasks()
    {
        var scheduler = await this.schedulerFactory.GetScheduler();

        var tasks = new List<TaskDTO>();

        // Get all currently executing jobs
        var executingJobs = await scheduler.GetCurrentlyExecutingJobs();
        var executinJobKeys = executingJobs
            .Select(ctx => ctx.JobDetail.Key)
            .ToHashSet();

        var categories = await scheduler.GetJobGroupNames();
        foreach (var category in categories)
        {
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(category));
            foreach (var jobKey in jobKeys)
            {
                var jobDetail = await scheduler.GetJobDetail(jobKey);
                var triggers = await scheduler.GetTriggersOfJob(jobKey);
                foreach (var trigger in triggers)
                {
                    var triggerState = await scheduler.GetTriggerState(trigger.Key);
                    var isSingleExecution = trigger is ISimpleTrigger simple && simple.RepeatCount == 0;
                    var isCronJob = trigger is ICronTrigger;

                    // Check if this job is currently running
                    var isRunning = executinJobKeys.Contains(jobKey);
                    var state = isRunning ? TaskState.Running : this.mapper.Map<TaskState>(triggerState);

                    var progress = this.dataStore.GetProgress(jobKey.ToString());

                    tasks.Add(new TaskDTO
                    {
                        Name = jobKey.Name,
                        Category = category,
                        State = state,
                        Progress = progress,
                        IsSingleExecution = isSingleExecution,
                        IsCronJob = isCronJob,
                        CronExpression = trigger is ICronTrigger cron ? cron.CronExpressionString : null,
                        LastExecution = trigger.GetPreviousFireTimeUtc(),
                        NextExecution = trigger.GetNextFireTimeUtc(),
                    });
                }
            }
        }

        return tasks;
    }

    /// <summary>
    /// Create a dummy task.
    /// </summary>
    /// <returns>A task.</returns>
    /// <remarks>REMOVE BEFORE NEXT RELEASE!.</remarks>
    [Obsolete("REMOVE BEFORE NEXT RELEASE!")]
    public async Task CreateDummyTask()
    {
        var scheduler = await this.schedulerFactory.GetScheduler();

        var job = JobBuilder.Create<DummyTask>()
            .WithIdentity("Dummy Task", "Dummy")
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity("Dummy Task Trigger", "Dummy")
            .WithCronSchedule("0 */5 * ? * *")
            .Build();

        await scheduler.ScheduleJob(job, trigger);

        var job2 = JobBuilder.Create<DummyTask>()
            .WithIdentity("Dummy Task 2", "Dummy")
            .Build();

        var trigger2 = TriggerBuilder.Create()
            .WithIdentity("Dummy Task Trigger 2", "Dummy")
            .WithCronSchedule("0 */1 * ? * *")
            .Build();

        await scheduler.ScheduleJob(job2, trigger2);
    }
}
