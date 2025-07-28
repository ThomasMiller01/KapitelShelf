// <copyright file="InternalTask.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Quartz;

namespace KapitelShelf.Api.DTOs.Tasks;

/// <summary>
/// The task details only used internally.
/// </summary>
/// <typeparam name="T">The job type.</typeparam>
public class InternalTask<T>
    where T : IJob
{
    /// <summary>
    /// Gets or sets the task title.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets the task category.
    /// </summary>
    public string Category { get; set; } = null!;

    /// <summary>
    /// Gets or sets the task description.
    /// </summary>
    public string? Description { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether the task should recover after a forced shutdown.
    /// </summary>
    public bool ShouldRecover { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the task should start now.
    /// </summary>
    public bool StartNow { get; set; } = false;

    /// <summary>
    /// Gets or sets the expression for a cronjob.
    /// </summary>
    public string? Cronjob { get; set; } = null;

    /// <summary>
    /// Gets the job detail for this task.
    /// </summary>
    public JobBuilder JobDetail => this.GetJobDetail();

    /// <summary>
    /// Gets the trigger for this task.
    /// </summary>
    public TriggerBuilder Trigger => this.GetTrigger();

    private JobBuilder GetJobDetail()
    {
        var jobDetail = JobBuilder.Create<T>()
            .WithIdentity(this.Title, this.Category);

        if (this.Description is not null)
        {
            jobDetail = jobDetail.WithDescription(this.Description);
        }

        if (this.ShouldRecover)
        {
            jobDetail = jobDetail.RequestRecovery();
        }

        return jobDetail;
    }

    private TriggerBuilder GetTrigger()
    {
        var trigger = TriggerBuilder.Create()
            .WithIdentity(this.Title, this.Category);

        if (this.StartNow)
        {
            trigger = trigger.StartNow();
        }

        if (this.Cronjob is not null)
        {
            trigger = trigger.WithCronSchedule(this.Cronjob);
        }

        return trigger;
    }
}
