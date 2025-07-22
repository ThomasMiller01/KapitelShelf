// <copyright file="TaskDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Tasks;

/// <summary>
/// The task dto.
/// </summary>
public class TaskDTO
{
    /// <summary>
    /// Gets or sets the task name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    public string Category { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; } = null;

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    public TaskState State { get; set; }

    /// <summary>
    /// Gets or sets the job progress, if the task is current running.
    /// </summary>
    public int? Progress { get; set; }

    /// <summary>
    /// Gets or sets the job message, if the task is current running.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets a value indicating, whether the cancelation of the task is requested.
    /// </summary>
    public bool? IsCancelationRequested { get; set; }

    /// <summary>
    /// Gets or sets the finished reason, if the task is finished.
    /// </summary>
    public FinishedReason? FinishedReason { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the task is a single execution or continued.
    /// </summary>
    public bool IsSingleExecution { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the task is a cron job.
    /// </summary>
    public bool IsCronJob { get; set; }

    /// <summary>
    /// Gets or sets the cron expression, if the job is a cron job.
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// Gets or sets the time offset when the task was last executed.
    /// </summary>
    public DateTimeOffset? LastExecution { get; set; }

    /// <summary>
    /// Gets or sets the time offset when the task will be next executed.
    /// </summary>
    public DateTimeOffset? NextExecution { get; set; }
}
