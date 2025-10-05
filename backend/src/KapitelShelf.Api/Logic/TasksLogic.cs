// <copyright file="TasksLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Tasks;
using Quartz;
using Quartz.Impl.Matchers;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The tasks logic.
/// </summary>
public class TasksLogic(IMapper mapper, ISchedulerFactory schedulerFactory, ITaskRuntimeDataStore dataStore) : ITasksLogic
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
    private readonly ITaskRuntimeDataStore dataStore = dataStore;

    /// <inheritdoc/>
    public async Task<List<TaskDTO>> GetTasks()
    {
        var scheduler = await this.schedulerFactory.GetScheduler();

        var tasks = new List<TaskDTO>();

        // Get all currently executing jobs
        var executingJobs = await scheduler.GetCurrentlyExecutingJobs();

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
                    var state = this.mapper.Map<TaskState>(triggerState);

                    // Check if this job is currently running
                    int? progress = null;
                    string? message = null;
                    bool? isCancelationRequested = null;

                    var runningJobContext = executingJobs.FirstOrDefault(x => x.JobDetail.Key.Equals(jobKey));
                    if (runningJobContext is not null)
                    {
                        state = TaskState.Running;
                        progress = this.dataStore.GetProgress(jobKey.ToString());
                        message = this.dataStore.GetMessage(jobKey.ToString());
                        isCancelationRequested = runningJobContext.CancellationToken.IsCancellationRequested;
                    }

                    tasks.Add(new TaskDTO
                    {
                        Name = jobKey.Name,
                        Category = category,
                        Description = jobDetail?.Description,
                        State = state,
                        Progress = progress,
                        Message = message,
                        IsCancelationRequested = isCancelationRequested,
                        FinishedReason = this.mapper.Map<FinishedReason?>(triggerState),
                        IsSingleExecution = isSingleExecution,
                        IsCronJob = isCronJob,
                        CronExpression = trigger is ICronTrigger cron ? cron.CronExpressionString : null,
                        LastExecution = trigger.GetPreviousFireTimeUtc(),
                        NextExecution = trigger.GetNextFireTimeUtc(),
                    });
                }
            }
        }

        // Add running jobs that no longer have job/trigger records (e.g., one-shot jobs)
        foreach (var jobContext in executingJobs)
        {
            var jobKey = jobContext.JobDetail.Key;
            if (!tasks.Any(t => t.Name == jobKey.Name && t.Category == jobKey.Group))
            {
                tasks.Add(new TaskDTO
                {
                    Name = jobKey.Name,
                    Category = jobKey.Group,
                    Description = jobContext.JobDetail.Description,
                    State = TaskState.Running,
                    Progress = this.dataStore.GetProgress(jobKey.ToString()),
                    Message = this.dataStore.GetMessage(jobKey.ToString()),
                    FinishedReason = null,
                    IsSingleExecution = true, // Best guess for deleted one-shot jobs
                    IsCronJob = false,
                    CronExpression = null,
                    LastExecution = jobContext.FireTimeUtc,
                    NextExecution = jobContext.NextFireTimeUtc,
                });
            }
        }

        return tasks;
    }
}
