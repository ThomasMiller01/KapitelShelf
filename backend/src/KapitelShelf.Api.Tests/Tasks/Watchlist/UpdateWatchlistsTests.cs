// <copyright file="UpdateWatchlistsTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Tasks;
using KapitelShelf.Api.Tasks.Watchlist;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using KapitelShelf.Data.Models.User;
using KapitelShelf.Data.Models.Watchlists;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Quartz;
using Testcontainers.PostgreSql;

namespace KapitelShelf.Api.Tests.Tasks.Watchlist;

/// <summary>
/// Unit tests for InitialStorageDownload.
/// </summary>
[TestFixture]
public class UpdateWatchlistsTests
{
    private PostgreSqlContainer postgres;

    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;

    private ITaskRuntimeDataStore dataStore;
    private ILogger<TaskBase> logger;
    private INotificationsLogic notificationsLogic;
    private IWatchlistLogic logic;
    private IJobExecutionContext context;
    private UpdateWatchlists testee;

    /// <summary>
    /// Setup database container.
    /// </summary>
    /// <returns>A task.</returns>
    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        this.postgres = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithImage("postgres:16")
            .Build();

        await this.postgres.StartAsync();
    }

    /// <summary>
    /// Teardown database container.
    /// </summary>
    /// <returns>A task.</returns>
    [OneTimeTearDown]
    public async Task Cleanup() => await this.postgres.DisposeAsync();

    /// <summary>
    /// Sets up testee and dependencies.
    /// </summary>
    /// <returns>A task.</returns>
    [SetUp]
    public async Task Setup()
    {
        this.dbOptions = new DbContextOptionsBuilder<KapitelShelfDBContext>()
            .UseNpgsql(this.postgres.GetConnectionString(), x => x.MigrationsAssembly("KapitelShelf.Data.Migrations"))
            .Options;

        // datamigrations
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            await context.Database.MigrateAsync();
        }

        this.dbContextFactory = Substitute.For<IDbContextFactory<KapitelShelfDBContext>>();
        this.dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(new KapitelShelfDBContext(this.dbOptions)));

        this.dataStore = Substitute.For<ITaskRuntimeDataStore>();
        this.logger = Substitute.For<ILogger<TaskBase>>();
        this.notificationsLogic = Substitute.For<INotificationsLogic>();
        this.logic = Substitute.For<IWatchlistLogic>();
        this.context = Substitute.For<IJobExecutionContext>();

        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.Key.Returns(new JobKey("UpdateWatchlists", "Group"));
        this.context.JobDetail.Returns(jobDetail);

        this.testee = new UpdateWatchlists(this.dataStore, this.logger, this.notificationsLogic, this.logic, this.dbContextFactory)
        {
            WaitDelayMin = 1,
            WaitDelayMax = 1,
        };
    }

    /// <summary>
    /// Tests ExecuteTask calls UpdateWatchlist for each watchlist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_CallsUpdateWatchlist()
    {
        // Setup
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series".Unique(),
        };
        var user = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "User".Unique(),
        };
        var watchlist = new WatchlistModel
        {
            Id = Guid.NewGuid(),
            Series = series,
            SeriesId = series.Id,
            UserId = user.Id,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Users.Add(user);
            context.Watchlist.Add(watchlist);
            context.SaveChanges();
        }

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.Received(1).UpdateWatchlist(watchlist.Id);
        this.dataStore.Received().SetProgress("Group.UpdateWatchlists", 0, 1);
        this.dataStore.Received().SetMessage("Group.UpdateWatchlists", Arg.Is<string>(m => m.Contains(series.Name)));
    }

    /// <summary>
    /// Tests ExecuteTask logs error if UpdateWatchlist throws.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_LogsError_WhenUpdateFails()
    {
        // Setup
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Broken".Unique(),
        };
        var user = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "User".Unique(),
        };
        var watchlist = new WatchlistModel
        {
            Id = Guid.NewGuid(),
            Series = series,
            SeriesId = series.Id,
            UserId = user.Id,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Users.Add(user);
            context.Watchlist.Add(watchlist);
            context.SaveChanges();
        }

        this.logic.When(x => x.UpdateWatchlist(watchlist.Id)).Do(_ => throw new InvalidOperationException("fail"));

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        this.logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error updating series watchlist")),
            Arg.Any<InvalidOperationException>(),
            Arg.Any<Func<object, Exception?, string>>());

        _ = this.notificationsLogic.Received(1).AddNotification(
            "UpdateWatchlistSeriesFailed",
            titleArgs: Arg.Any<object[]>(),
            messageArgs: Arg.Any<object[]>(),
            type: NotificationTypeDto.Error,
            severity: NotificationSeverityDto.High,
            source: "Task [Update Watchlists]",
            userId: user.Id);
    }

    /// <summary>
    /// Tests ExecuteTask does not call UpdateWatchlist for watchlists checked within the last 24 hours.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_DoesNotCallUpdateWatchlist_WhenLastCheckedWithin24Hours()
    {
        // Setup
        var seriesOld = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "SeriesOld".Unique(),
        };
        var seriesRecent = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "SeriesRecent".Unique(),
        };
        var user = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "User".Unique(),
        };

        var watchlistOld = new WatchlistModel
        {
            Id = Guid.NewGuid(),
            Series = seriesOld,
            SeriesId = seriesOld.Id,
            UserId = user.Id,
            LastChecked = DateTime.UtcNow.AddHours(-25),
        };

        var watchlistRecent = new WatchlistModel
        {
            Id = Guid.NewGuid(),
            Series = seriesRecent,
            SeriesId = seriesRecent.Id,
            UserId = user.Id,
            LastChecked = DateTime.UtcNow.AddHours(-1),
        };

        using (var db = new KapitelShelfDBContext(this.dbOptions))
        {
            db.Series.AddRange(seriesOld, seriesRecent);
            db.Users.Add(user);
            db.Watchlist.AddRange(watchlistOld, watchlistRecent);
            db.SaveChanges();
        }

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.Received(1).UpdateWatchlist(watchlistOld.Id);
        await this.logic.DidNotReceive().UpdateWatchlist(watchlistRecent.Id);

        this.dataStore.Received().SetMessage(
            "Group.UpdateWatchlists",
            Arg.Is<string>(m => m.Contains(seriesOld.Name)));
    }

    /// <summary>
    /// Tests Kill does nothing.
    /// </summary>
    [Test]
    public void Kill_DoesNothing() => Assert.DoesNotThrowAsync(this.testee.Kill);

    /// <summary>
    /// Tests Schedule creates and schedules the job.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Schedule_CreatesAndSchedulesJob()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();

        // Execute
        var jobKey = await UpdateWatchlists.Schedule(scheduler);

        // Assert
        Assert.That(jobKey, Is.Not.Null.And.Contain("Update Watchlists"));
        await scheduler.Received().ScheduleJob(
            Arg.Any<IJobDetail>(),
            Arg.Any<IReadOnlyCollection<ITrigger>>(),
            true,
            Arg.Any<CancellationToken>());
    }
}
