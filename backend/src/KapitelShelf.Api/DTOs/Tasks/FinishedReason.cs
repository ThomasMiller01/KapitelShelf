// <copyright file="FinishedReason.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Tasks;

/// <summary>
/// The reason for the finished state of the task.
/// </summary>
public enum FinishedReason
{
    /// <summary>
    /// The task completed successfully.
    /// </summary>
    Completed = 0,

    /// <summary>
    /// The task run into an error and could not be re-scheduled.
    /// </summary>
    Error = 1,
}
