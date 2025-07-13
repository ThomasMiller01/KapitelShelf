// <copyright file="TaskMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Tasks;
using Quartz;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The task mapping profile.
/// </summary>
public class TaskMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskMappingProfile"/> class.
    /// </summary>
    public TaskMappingProfile()
    {
        CreateMap<TriggerState, TaskState>()
            .ConvertUsing(src => MapTriggerStateToTaskState(src));

        CreateMap<TriggerState, FinishedReason?>()
            .ConvertUsing(src => MapTriggerStateToFinishedReason(src));
    }

    /// <summary>
    /// Maps Quartz TriggerState to the TaskState.
    /// </summary>
    /// <param name="state">The Quartz TriggerState.</param>
    /// <returns>The mapped TaskState.</returns>
    private static TaskState MapTriggerStateToTaskState(TriggerState state)
    {
        return state switch
        {
            TriggerState.Normal => TaskState.Scheduled,
            TriggerState.Paused => TaskState.Scheduled,
            TriggerState.Blocked => TaskState.Scheduled,
            TriggerState.Complete => TaskState.Finished,
            TriggerState.Error => TaskState.Finished,
            TriggerState.None => TaskState.Finished,
            _ => TaskState.Scheduled
        };
    }

    /// <summary>
    /// Maps Quartz TriggerState to the TaskState.
    /// </summary>
    /// <param name="state">The Quartz TriggerState.</param>
    /// <returns>The mapped TaskState.</returns>
    private static FinishedReason? MapTriggerStateToFinishedReason(TriggerState state)
    {
        return state switch
        {
            TriggerState.Complete => FinishedReason.Completed,
            TriggerState.Error => FinishedReason.Error,
            TriggerState.None => FinishedReason.Completed,
            _ => null
        };
    }
}
