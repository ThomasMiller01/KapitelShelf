// <copyright file="TaskBase.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Runtime.CompilerServices;
using KapitelShelf.Api.DTOs.Tasks;
using Quartz;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Tasks;

/// <summary>
/// The base class for all tasks to inherit from.
/// </summary>
public abstract class TaskBase(ITaskRuntimeDataStore dataStore, ILogger<TaskBase> logger) : IJob
{
#pragma warning disable SA1401 // Fields should be private
    internal readonly ILogger<TaskBase> Logger = logger;

    internal readonly ITaskRuntimeDataStore DataStore = dataStore;
#pragma warning restore SA1401 // Fields should be private

    /// <inheritdoc/>
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(context);

            this.DataStore.SetProgress(JobKey(context), 0);

            await this.ExecuteTask(context);

            this.DataStore.ClearData(JobKey(context));
        }
        catch (JobExecutionException)
        {
            // rethrow job execution exception
            throw;
        }
        catch (OperationCanceledException)
        {
            this.Logger.LogWarning("Task '{JobKey}' was canceled", context?.JobDetail.Key.ToString());
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
    /// Kill the task.
    /// </summary>
    /// <returns>A task.</returns>
    public abstract Task Kill();

    /// <summary>
    /// Get the job key for a task.
    /// </summary>
    /// <param name="context">The job context.</param>
    /// <returns>The job key.</returns>
    public static string JobKey(IJobExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.JobDetail.Key.ToString();
    }

    /// <summary>
    /// Check if the scheduler interrupted this job.
    /// </summary>
    /// <param name="context">The context.</param>
    public void CheckForInterrupt(IJobExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.CancellationToken.ThrowIfCancellationRequested();
    }

    /// <summary>
    /// Steps to run pre-schedule.
    /// </summary>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="job">The job.</param>
    /// <param name="options">The task schedule options.</param>
    /// <returns>A task.</returns>
    public static async Task PreScheduleSteps(IScheduler scheduler, IJobDetail job, TaskScheduleOptionsDTO? options)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(job);

        if (options is null)
        {
            return;
        }

        if (options.StopTaskIfRunning)
        {
            await InterruptAndWait(scheduler, job, timeout: 30);
        }

        if (options.WaitForFinish)
        {
            WaitForTaskListener listener = new(job.Key.ToString());
            scheduler.ListenerManager.AddJobListener(listener);
        }
    }

    /// <summary>
    /// Steps to run post-schedule.
    /// </summary>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="job">The job.</param>
    /// <param name="options">The task schedule options.</param>
    /// <returns>A task.</returns>
    public static async Task PostScheduleSteps(IScheduler scheduler, IJobDetail job, TaskScheduleOptionsDTO? options)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(job);

        if (options is null)
        {
            return;
        }

        if (options.WaitForFinish)
        {
            var listener = scheduler.ListenerManager.GetJobListener(WaitForTaskListener.PublicName);
            if (listener is not null)
            {
                await ((WaitForTaskListener)listener).WaitAsync(60);
            }
        }
    }

    /// <summary>
    /// Interrupt a job and wait for it to finish.
    /// </summary>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="job">The job.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>A task.</returns>
    internal static async Task InterruptAndWait(IScheduler scheduler, IJobDetail job, int timeout = 30)
    {
        var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<TaskBase>();

        // Request job interruption
        await scheduler.Interrupt(job.Key);

        var sw = Stopwatch.StartNew();
        bool finished = false;
        IJobExecutionContext? runningContext = null;

        // Poll every 500ms for up to the timeout
        while (sw.Elapsed.TotalSeconds < timeout)
        {
            var executingJobs = await scheduler.GetCurrentlyExecutingJobs();
            runningContext = executingJobs.FirstOrDefault(ctx => ctx.JobDetail.Key.Equals(job.Key));

            if (runningContext == null)
            {
                finished = true;
                break; // job has finished
            }

            await Task.Delay(500);
        }

        // If still running after timeout, try to call Kill
        if (!finished && runningContext != null)
        {
            if (runningContext.JobInstance is TaskBase taskInstance)
            {
                logger.LogWarning("Killing job with key '{JobKey}'", JobKey(runningContext));
                await taskInstance.Kill();
            }
            else
            {
                logger.LogError("Job with key '{JobKey}' could not be killed", JobKey(runningContext));
            }
        }
    }
}
