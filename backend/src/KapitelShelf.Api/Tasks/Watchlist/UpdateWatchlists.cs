// <copyright file="UpdateWatchlists.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace KapitelShelf.Api.Tasks.Watchlist;

/// <summary>
/// Updates the watchlist results for series watchlists.
/// </summary>
public class UpdateWatchlists(ITaskRuntimeDataStore dataStore, ILogger<TaskBase> logger, IWatchlistLogic logic, IDbContextFactory<KapitelShelfDBContext> dbContextFactory) : TaskBase(dataStore, logger)
{
    private readonly IWatchlistLogic logic = logic;

    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    /// <inheritdoc/>
    public override async Task ExecuteTask(IJobExecutionContext context)
    {
        using var dbContext = await this.dbContextFactory.CreateDbContextAsync();
        var watchlists = dbContext.Watchlist
            .AsNoTracking()
            .Include(x => x.Series)
            .ToList();

        for (int i = 0; i < watchlists.Count; i++)
        {
            var watchlist = watchlists[i];

            this.DataStore.SetMessage(JobKey(context), $"Checking '{watchlist.Series.Name}' ...");

            try
            {
                await this.logic.UpdateWatchlist(watchlist.Id);

                // wait 10 seconds between series
                await Task.Delay(10000);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error updating series watchlist with id {Id}", watchlist.Id);
            }

            this.DataStore.SetProgress(JobKey(context), i, watchlists.Count);

            this.CheckForInterrupt(context);
        }
    }

    /// <inheritdoc/>
    public override async Task Kill() => await Task.CompletedTask;

    /// <summary>
    /// Schedule this task.
    /// </summary>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="options">The task schedule options.</param>
    /// <returns>The job key.</returns>
    public static async Task<string> Schedule(IScheduler scheduler, TaskScheduleOptionsDTO? options = null)
    {
        ArgumentNullException.ThrowIfNull(scheduler);

        var internalTask = new InternalTask<UpdateWatchlists>
        {
            Title = "Update Watchlists",
            Category = "Watchlist",
            Description = "Searches for new volumes of the watched series.",
            ShouldRecover = true,
            Cronjob = "0 0 * ? * *", // every 1 hour
        };

        var job = internalTask.JobDetail
            .Build();

        var trigger = internalTask.Trigger
            .Build();

        await PreScheduleSteps(scheduler, job, options);

        await scheduler.ScheduleJob(job, [trigger], replace: true);

        await PostScheduleSteps(scheduler, job, options);

        return job.Key.ToString();
    }
}
