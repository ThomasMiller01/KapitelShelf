// <copyright file="Mapper.Tasks.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Tasks;
using Quartz;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The tasks mapper.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a trigger state to a task state.
    /// </summary>
    /// <param name="state">The trigger state.</param>
    /// <returns>The task state.</returns>
    public TaskState TriggerStateToTaskState(TriggerState state)
    {
        return state switch
        {
            TriggerState.Normal => TaskState.Scheduled,
            TriggerState.Paused => TaskState.Scheduled,
            TriggerState.Blocked => TaskState.Scheduled,
            TriggerState.Complete => TaskState.Finished,
            TriggerState.Error => TaskState.Finished,
            TriggerState.None => TaskState.Finished,
            _ => TaskState.Scheduled,
        };
    }

    /// <summary>
    /// Map a trigger state to a finished reason.
    /// </summary>
    /// <param name="state">The trigger state.</param>
    /// <returns>The finished reason.</returns>
    public FinishedReason? TriggerStateToFinishedReason(TriggerState state)
    {
        return state switch
        {
            TriggerState.Complete => FinishedReason.Completed,
            TriggerState.Error => FinishedReason.Error,
            TriggerState.None => FinishedReason.Completed,
            _ => null,
        };
    }
}
