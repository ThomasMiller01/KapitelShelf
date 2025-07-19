// <copyright file="WaitForTaskListener.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Quartz;

namespace KapitelShelf.Api.Tasks;

/// <summary>
/// Wait for a task to finish listener.
/// </summary>
public class WaitForTaskListener(string jobKey) : IJobListener
{
    private readonly string jobKey = jobKey;

    private readonly TaskCompletionSource<bool> task = new();

    /// <inheritdoc/>
    public string Name => "WaitForTaskListener";

    /// <inheritdoc/>
    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.JobDetail.Key.ToString() == jobKey)
        {
            this.task.TrySetResult(jobException == null);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Wait for the task to finish.
    /// </summary>
    /// <returns>True if task succeeded, otherwise false.</returns>
    public Task<bool> WaitAsync() => this.task.Task;
}
