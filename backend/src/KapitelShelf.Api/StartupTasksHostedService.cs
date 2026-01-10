// <copyright file="StartupTasksHostedService.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Tasks.CloudStorage;
using KapitelShelf.Api.Tasks.Maintenance;
using KapitelShelf.Api.Tasks.Watchlist;
using Quartz;

namespace KapitelShelf.Api;

/// <summary>
/// Hosted service for initially starting up system tasks that run automatically.
/// </summary>
public class StartupTasksHostedService(ISchedulerFactory schedulerFactory, IDynamicSettingsManager dynamicSettings) : IHostedService
{
    private readonly ISchedulerFactory schedulerFactory = schedulerFactory;

    private readonly IDynamicSettingsManager dynamicSettings = dynamicSettings;

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // ---------- Tasks ----------
        var scheduler = await this.schedulerFactory.GetScheduler(cancellationToken);

        // Maintenance
        await CleanupFinishedTasks.Schedule(scheduler);
        await CleanupDatabase.Schedule(scheduler);
        await CleanupExpiredNotifications.Schedule(scheduler);

        // CloudStorage
        await SyncStorageData.Schedule(scheduler);
        await ScanForBooks.Schedule(scheduler);

        // Watchlist
        await UpdateWatchlists.Schedule(scheduler);

        // ---------- Settings ----------
        await this.dynamicSettings.InitializeOnStartup();
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken) => await Task.CompletedTask;
}
