// <copyright file="TaskRuntimeDataStore.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Collections.Concurrent;

namespace KapitelShelf.Api.Tasks;

/// <summary>
/// The task runtime data store is used to pass data between running tasks and the Api at runtime.
/// </summary>
public class TaskRuntimeDataStore : ITaskRuntimeDataStore
{
    private readonly ConcurrentDictionary<string, int> progress = new();

    private readonly ConcurrentDictionary<string, string> message = new();

    /// <inheritdoc/>
    public void SetProgress(string jobKey, int percentage) => this.progress[jobKey] = percentage;

    /// <inheritdoc/>
    public void SetProgress(string jobKey, int current, int total)
    {
        var percentage = (int)Math.Floor((double)current / total * 100);
        SetProgress(jobKey, percentage);
    }

    /// <inheritdoc/>
    public void SetProgress(string jobKey, int current, int total, int itemPercentage)
    {
        if (total <= 0)
        {
            SetProgress(jobKey, 0);
            return;
        }

        var globalPercentage = (int)Math.Floor(((current * 100.0) + itemPercentage) / total);
        SetProgress(jobKey, globalPercentage);
    }

    /// <inheritdoc/>
    public int? GetProgress(string jobKey) => this.progress.TryGetValue(jobKey, out var percentage) ? percentage : null;

    /// <inheritdoc/>
    public void ClearData(string jobKey)
    {
        this.progress.Remove(jobKey, out var _);
        this.message.Remove(jobKey, out var _);
    }

    /// <inheritdoc/>
    public void SetMessage(string jobKey, string message) => this.message[jobKey] = message;

    /// <inheritdoc/>
    public string? GetMessage(string jobKey) => this.message.TryGetValue(jobKey, out var msg) ? msg : null;
}
