// <copyright file="ITaskRuntimeDataStore.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.Tasks;

/// <summary>
/// The task runtime data store interface.
/// </summary>
public interface ITaskRuntimeDataStore
{
    /// <summary>
    /// Set the progress for a task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    /// <param name="percentage">The progress percentage.</param>
    void SetProgress(string jobKey, int percentage);

    /// <summary>
    /// Set the progress for a task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    /// <param name="current">The index of the currently finished item.</param>
    /// <param name="total">The total number of items.</param>
    void SetProgress(string jobKey, int current, int total);

    /// <summary>
    /// Get the progress for a task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    /// <returns>The progress percentage.</returns>
    int? GetProgress(string jobKey);

    /// <summary>
    /// Remove all data of the task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    void ClearData(string jobKey);

    /// <summary>
    /// Set the message for a task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    /// <param name="message">The message.</param>
    void SetMessage(string jobKey, string message);

    /// <summary>
    /// Get the message for a task.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    /// <returns>The message.</returns>
    string? GetMessage(string jobKey);
}
