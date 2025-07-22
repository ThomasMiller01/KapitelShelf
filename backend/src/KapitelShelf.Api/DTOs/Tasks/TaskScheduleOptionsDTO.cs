// <copyright file="TaskScheduleOptionsDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Tasks;

/// <summary>
/// The task schedule options dto.
/// </summary>
public class TaskScheduleOptionsDTO
{
    /// <summary>
    /// Gets or sets a value indicating whether to wait for the task to finish.
    /// </summary>
    public bool WaitForFinish { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to stop the task, if it is already running.
    /// </summary>
    public bool StopTaskIfRunning { get; set; }

    /// <summary>
    /// Gets a preset for tasks that need to wait to finish.
    /// </summary>
    public static TaskScheduleOptionsDTO WaitPreset => new()
    {
        WaitForFinish = true,
    };

    /// <summary>
    /// Gets a preset for tasks that need to be restarted.
    /// </summary>
    public static TaskScheduleOptionsDTO RestartPreset => new()
    {
        StopTaskIfRunning = true,
    };
}
