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

    private readonly ConcurrentDictionary<string, string> message = new();

    /// <summary>
    /// Set the progress for a task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    /// <param name="percentage">The progress percentage.</param>
    public void SetProgress(string jobKey, int percentage) => this.progress[jobKey] = percentage;

    /// <summary>
    /// Set the progress for a task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    /// <param name="current">The index of the currently finished item.</param>
    /// <param name="total">The total number of items.</param>
    public void SetProgress(string jobKey, int current, int total)
    {
        var percentage = (int)Math.Floor((double)current / total * 100);
        SetProgress(jobKey, percentage);
    }

    /// <summary>
    /// Get the progress for a task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    /// <returns>The progress percentage.</returns>
    public int? GetProgress(string jobKey) => this.progress.TryGetValue(jobKey, out var percentage) ? percentage : null;

    /// <summary>
    /// Remove all data of the task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    public void ClearData(string jobKey)
    {
        this.progress.Remove(jobKey, out var _);
        this.message.Remove(jobKey, out var _);
    }

    /// <summary>
    /// Set the message for a task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    /// <param name="message">The message.</param>
    public void SetMessage(string jobKey, string message) => this.message[jobKey] = message;

    /// <summary>
    /// Get the message for a task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    /// <returns>The message.</returns>
    public string? GetMessage(string jobKey) => this.message.TryGetValue(jobKey, out var msg) ? msg : null;
}
