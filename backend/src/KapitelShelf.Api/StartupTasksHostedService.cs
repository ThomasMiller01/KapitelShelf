// <copyright file="StartupTasksHostedService.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Tasks;
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

        // Cleanup Finished Tasks
        var job = JobBuilder.Create<CleanupFinishedTasks>()
            .WithIdentity("Cleanup Finished Tasks", "Maintenance")
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity("Cleanup Finished Tasks", "Maintenance")
            .StartNow()
            .WithCronSchedule("0 */15 * ? * *")
            .Build();

        await scheduler.ScheduleJob(job, [trigger], replace: true, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken) => await Task.CompletedTask;
}
