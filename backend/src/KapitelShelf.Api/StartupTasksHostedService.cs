// <copyright file="StartupTasksHostedService.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Tasks.Maintenance;
using Quartz;

namespace KapitelShelf.Api;

/// <summary>
/// Hosted service for initially starting up system tasks that run automatically.
/// </summary>
public class StartupTasksHostedService(ISchedulerFactory schedulerFactory) : IHostedService
{
    private readonly ISchedulerFactory schedulerFactory = schedulerFactory;

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var scheduler = await this.schedulerFactory.GetScheduler(cancellationToken);

        await CleanupFinishedTasks.Schedule(scheduler);
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken) => await Task.CompletedTask;
}
