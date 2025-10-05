// <copyright file="ITasksLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Tasks;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// The tasks logic interface.
/// </summary>
public interface ITasksLogic
{
    /// <summary>
    /// Get all tasks.
    /// </summary>
    /// <returns>A list of all tasks.</returns>
    Task<List<TaskDTO>> GetTasks();
}
