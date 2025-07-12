// <copyright file="TaskRuntimeDataStore.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Collections.Concurrent;

namespace KapitelShelf.Api.Tasks;

/// <summary>
/// The task runtime data store is used to pass data between running tasks and the Api at runtime.
/// </summary>
public class TaskRuntimeDataStore
{
    private readonly ConcurrentDictionary<string, int> progress = new();

    /// <summary>
    /// Set the progress for a task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    /// <param name="percentage">The progress percentage.</param>
    public void SetProgress(string jobKey, int percentage) => this.progress[jobKey] = percentage;

    /// <summary>
    /// Get the progress for a task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    /// <returns>The progress percentage.</returns>
    public int? GetProgress(string jobKey) => this.progress.TryGetValue(jobKey, out var percentage) ? percentage : null;

    /// <summary>
    /// Remove progress of the task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    public void ClearProgress(string jobKey) => this.progress.Remove(jobKey, out var _);
}
