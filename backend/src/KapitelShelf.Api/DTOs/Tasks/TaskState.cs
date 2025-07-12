// <copyright file="TaskState.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Tasks;

/// <summary>
/// The task state enum.
/// </summary>
public enum TaskState
{
    /// <summary>
    /// Task is scheduled and will be executed later.
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Task is being executed.
    /// </summary>
    Running = 1,

    /// <summary>
    /// Task finished its execution.
    /// </summary>
    Finished = 2,
}
