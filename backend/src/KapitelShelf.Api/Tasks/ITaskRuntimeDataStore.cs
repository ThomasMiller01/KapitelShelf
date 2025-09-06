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
    /// Set the progress based on the index of the current item from a total number of items.
    /// </summary>
    /// <param name="jobKey">The job key of the task.</param>
    /// <param name="current">The index of the currently finished item.</param>
    /// <param name="total">The total number of items.</param>
    void SetProgress(string jobKey, int current, int total);

    /// <summary>
    /// Set the progress based on multiple items with own progresses.
    /// </summary>
    /// <remarks>
    /// Calculate the global progress of all items:
    /// - each item has its own progress from 0-100%
    /// - based on what item currently is active, the progress is calculated
    /// - Example: current progressBase = 40, progressIncrement = 20, percentage = 50
    ///   => progress = 40 + (50 * 0.2) = 50.
    ///  </remarks>
    /// <param name="jobKey">The job key of the task.</param>
    /// <param name="current">The index of the current item.</param>
    /// <param name="total">The total number of items.</param>
    /// <param name="itemPercentage">The individuel progress of the item.</param>
    void SetProgress(string jobKey, int current, int total, int itemPercentage);

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
