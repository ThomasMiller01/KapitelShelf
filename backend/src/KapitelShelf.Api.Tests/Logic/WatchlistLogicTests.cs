// <copyright file="WatchlistLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using AutoMapper;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Logic.Interfaces.Storage;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using KapitelShelf.Data.Models.User;
using KapitelShelf.Data.Models.Watchlists;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the SeriesLogic class.
/// </summary>
[TestFixture]
public class WatchlistLogicTests
{
    private PostgreSqlContainer postgres;

    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private IMapper mapper;
    private ISeriesLogic seriesLogic;
    private IBooksLogic booksLogic;
    private IWatchlistScraperManager watchlistScraperManager;
    private IBookStorage bookStorage;
    private MetadataLogic metadataLogic;
    private WatchlistLogic testee;

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
    /// Sets up a new in-memory database and fakes before each test.
    /// </summary>
    /// <returns>A task.</returns>
    [SetUp]
    public async Task SetUp()
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

        this.mapper = Testhelper.CreateMapper();
        this.seriesLogic = Substitute.For<ISeriesLogic>();
        this.booksLogic = Substitute.For<IBooksLogic>();
        this.watchlistScraperManager = Substitute.For<IWatchlistScraperManager>();
        this.bookStorage = Substitute.For<IBookStorage>();
        this.metadataLogic = Substitute.For<MetadataLogic>();
        this.testee = new WatchlistLogic(this.dbContextFactory, this.mapper, this.seriesLogic, this.booksLogic, this.watchlistScraperManager, this.bookStorage, this.metadataLogic);
    }

    /// <summary>
    /// Tests that IsOnWatchlist returns true when the watchlist entry exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task IsOnWatchlist_ReturnsTrue_WhenEntryExists()
    {
        // Setup
        var userId = Guid.NewGuid();
        var seriesId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "Series".Unique(),
            });
            context.Users.Add(new UserModel
            {
                Id = userId,
                Username = "User".Unique(),
            });
            context.Watchlist.Add(new WatchlistModel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SeriesId = seriesId,
            });
            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.IsOnWatchlist(seriesId, userId);

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests that IsOnWatchlist returns false when the entry does not exist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task IsOnWatchlist_ReturnsFalse_WhenNotExists()
    {
        // Execute
        var result = await this.testee.IsOnWatchlist(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests that AddToWatchlist adds a new entry for a valid series.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddToWatchlist_AddsNewEntry()
    {
        // Setup
        var userId = Guid.NewGuid();
        var seriesId = Guid.NewGuid();
        var seriesModel = new SeriesModel
        {
            Id = seriesId,
            Name = "Series".Unique(),
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(seriesModel);
            context.Users.Add(new UserModel
            {
                Id = userId,
                Username = "User".Unique(),
            });
            await context.SaveChangesAsync();
        }

        this.seriesLogic.GetSeriesByIdAsync(seriesId)
            .Returns(Task.FromResult<SeriesDTO?>(this.mapper.Map<SeriesDTO>(seriesModel)));

        // Execute
        var dto = await this.testee.AddToWatchlist(seriesId, userId);

        // Assert
        Assert.That(dto, Is.Not.Null);
    }

    /// <summary>
    /// Tests that AddToWatchlist returns null when the series does not exist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddToWatchlist_ReturnsNull_WhenSeriesNotFound()
    {
        // Execute
        var result = await this.testee.AddToWatchlist(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that AddToWatchlist throws when the entry already exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddToWatchlist_Throws_WhenAlreadyExists()
    {
        // Setup
        var userId = Guid.NewGuid();
        var seriesId = Guid.NewGuid();
        var seriesModel = new SeriesModel
        {
            Id = seriesId,
            Name = "Series".Unique(),
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(seriesModel);
            context.Users.Add(new UserModel
            {
                Id = userId,
                Username = "User".Unique(),
            });
            context.Watchlist.Add(new WatchlistModel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SeriesId = seriesId,
            });
            await context.SaveChangesAsync();
        }

        this.seriesLogic.GetSeriesByIdAsync(seriesId)
            .Returns(Task.FromResult<SeriesDTO?>(this.mapper.Map<SeriesDTO>(seriesModel)));

        // Execute, Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.testee.AddToWatchlist(seriesId, userId));
        Assert.That(ex!.Message, Does.Contain("already on the watchlist"));
    }

    /// <summary>
    /// Tests that AddToWatchlist throws when the location type is not supported.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddToWatchlist_Throws_WhenLocationNotSupported()
    {
        // Setup
        var userId = Guid.NewGuid();
        var seriesId = Guid.NewGuid();
        var seriesModel = new SeriesModel
        {
            Id = seriesId,
            Name = "Series".Unique(),
            Books = [
                    new BookModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Book".Unique(),
                        Description = "Description",
                        SeriesId = seriesId,
                        SeriesNumber = 1,
                        Location = new LocationModel
                        {
                            Id = Guid.NewGuid(),
                            Type = LocationType.Physical,
                        },
                    },
                ],
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(seriesModel);
            context.Users.Add(new UserModel
            {
                Id = userId,
                Username = "User".Unique(),
            });
            await context.SaveChangesAsync();
        }

        this.seriesLogic.GetSeriesByIdAsync(seriesId)
            .Returns(Task.FromResult<SeriesDTO?>(this.mapper.Map<SeriesDTO>(seriesModel)));

        // Execute, Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.testee.AddToWatchlist(seriesId, userId));
        Assert.That(ex!.Message, Does.Contain("does not support watchlists"));
    }

    /// <summary>
    /// Tests that RemoveFromWatchlist removes an existing entry.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task RemoveFromWatchlist_RemovesEntry()
    {
        // Setup
        var userId = Guid.NewGuid();
        var seriesId = Guid.NewGuid();
        var watchlist = new WatchlistModel
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SeriesId = seriesId,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "Series".Unique(),
            });
            context.Users.Add(new UserModel
            {
                Id = userId,
                Username = "User".Unique(),
            });
            context.Watchlist.Add(watchlist);
            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.RemoveFromWatchlist(seriesId, userId);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    /// <summary>
    /// Tests that RemoveFromWatchlist returns null when the entry does not exist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task RemoveFromWatchlist_ReturnsNull_WhenNotExists()
    {
        // Execute
        var result = await this.testee.RemoveFromWatchlist(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that RemoveFromWatchlist deletes results when last watcher is removed.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task RemoveFromWatchlist_DeletesResults_WhenLastUserRemoved()
    {
        // Setup
        var seriesId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "Series".Unique(),
            });
            context.Users.Add(new UserModel
            {
                Id = userId,
                Username = "User".Unique(),
            });
            context.Watchlist.Add(new WatchlistModel
            {
                Id = Guid.NewGuid(),
                SeriesId = seriesId,
                UserId = userId,
            });
            context.WatchlistResults.Add(new WatchlistResultModel
            {
                Id = Guid.NewGuid(),
                SeriesId = seriesId,
                Title = "Vol1",
                Volume = 1,
                LocationUrl = "ASIN".Unique(),
            });
            await context.SaveChangesAsync();
        }

        // Execute
        await this.testee.RemoveFromWatchlist(seriesId, userId);

        // Assert
        using var context2 = new KapitelShelfDBContext(this.dbOptions);
        Assert.That(await context2.WatchlistResults.AnyAsync(r => r.SeriesId == seriesId), Is.False);
    }

    /// <summary>
    /// Tests that RemoveFromWatchlist keeps results when other users still watch the series.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task RemoveFromWatchlist_KeepsResults_WhenOtherUserExists()
    {
        // Setup
        var seriesId = Guid.NewGuid();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "Series".Unique(),
            });
            context.Users.Add(new UserModel
            {
                Id = user1,
                Username = "User1".Unique(),
            });
            context.Users.Add(new UserModel
            {
                Id = user2,
                Username = "User2".Unique(),
            });
            context.Watchlist.Add(new WatchlistModel
            {
                Id = Guid.NewGuid(),
                SeriesId = seriesId,
                UserId = user1,
            });
            context.Watchlist.Add(new WatchlistModel
            {
                Id = Guid.NewGuid(),
                SeriesId = seriesId,
                UserId = user2,
            });
            context.WatchlistResults.Add(new WatchlistResultModel
            {
                Id = Guid.NewGuid(),
                SeriesId = seriesId,
                Title = "Vol1",
                Volume = 1,
                LocationUrl = "ASIN".Unique(),
            });
            await context.SaveChangesAsync();
        }

        // Execute
        await this.testee.RemoveFromWatchlist(seriesId, user1);

        // Assert
        using var context2 = new KapitelShelfDBContext(this.dbOptions);
        Assert.That(await context2.WatchlistResults.AnyAsync(r => r.SeriesId == seriesId), Is.True);
    }

    /// <summary>
    /// Tests that GetWatchlistAsync returns items with results.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetWatchlistAsync_ReturnsItemsWithResults()
    {
        // Setup
        var userId = Guid.NewGuid();
        var seriesId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "Series".Unique(),
            });
            context.Users.Add(new UserModel
            {
                Id = userId,
                Username = "User".Unique(),
            });
            context.Watchlist.Add(new WatchlistModel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SeriesId = seriesId,
            });
            context.WatchlistResults.Add(new WatchlistResultModel
            {
                Id = Guid.NewGuid(),
                SeriesId = seriesId,
                Title = "Vol1",
                Volume = 1,
                ReleaseDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                LocationUrl = "ASIN".Unique(),
            });
            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.GetWatchlistAsync(userId);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.First().Items, Is.Not.Empty);
    }

    /// <summary>
    /// Tests that GetWatchlistAsync returns empty when no entries exist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetWatchlistAsync_ReturnsEmpty_WhenNoEntries()
    {
        // Execute
        var result = await this.testee.GetWatchlistAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Tests that UpdateWatchlist adds new scraped volumes.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateWatchlist_AddsScrapedVolumes()
    {
        // Setup
        var seriesId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var watchlistId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "Series".Unique(),
                Books = [],
            });
            context.Users.Add(new UserModel
            {
                Id = userId,
                Username = "User".Unique(),
            });
            context.Watchlist.Add(new WatchlistModel
            {
                Id = watchlistId,
                SeriesId = seriesId,
                UserId = userId,
            });
            await context.SaveChangesAsync();
        }

        this.watchlistScraperManager.Scrape(Arg.Any<SeriesDTO>())
            .Returns([
                new()
                {
                    SeriesId = seriesId,
                    Volume = 1,
                    Title = "Scraped",
                    ReleaseDate = DateTime.UtcNow.AddDays(10).ToString(CultureInfo.InvariantCulture),
                    LocationUrl = "ASIN".Unique(),
                },
            ]);

        // Execute
        await this.testee.UpdateWatchlist(watchlistId);

        // Assert
        using var context2 = new KapitelShelfDBContext(this.dbOptions);
        Assert.That(await context2.WatchlistResults.AnyAsync(x => x.SeriesId == seriesId && x.Title == "Scraped"), Is.True);
    }

    /// <summary>
    /// Tests that UpdateWatchlist updates an existing scraped volume.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateWatchlist_UpdatesExistingVolume()
    {
        // Setup
        var seriesId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var watchlistId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "Series".Unique(),
                Books = [],
            });
            context.Users.Add(new UserModel
            {
                Id = userId,
                Username = "User".Unique(),
            });
            context.Watchlist.Add(new WatchlistModel
            {
                Id = watchlistId,
                SeriesId = seriesId,
                UserId = userId,
            });
            context.WatchlistResults.Add(new WatchlistResultModel
            {
                Id = Guid.NewGuid(),
                SeriesId = seriesId,
                Volume = 1,
                Title = "Old",
                LocationUrl = "ASIN".Unique(),
            });
            await context.SaveChangesAsync();
        }

        this.watchlistScraperManager.Scrape(Arg.Any<SeriesDTO>())
            .Returns([
                new()
                {
                    SeriesId = seriesId,
                    Volume = 1,
                    Title = "Updated",
                    ReleaseDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                    LocationUrl = "ASIN".Unique(),
                },
            ]);

        // Execute
        await this.testee.UpdateWatchlist(watchlistId);

        // Assert
        using var context2 = new KapitelShelfDBContext(this.dbOptions);
        Assert.That(context2.WatchlistResults.Single(r => r.SeriesId == seriesId && r.Volume == 1).Title, Is.EqualTo("Updated"));
    }

    /// <summary>
    /// Tests that UpdateWatchlist ignores duplicate titles already in the library.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateWatchlist_IgnoresDuplicateTitles()
    {
        // Setup
        var seriesId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var watchlistId = Guid.NewGuid();

        var bookTitle = "Book1".Unique();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "Series".Unique(),
                Books = [
                    new BookModel
                    {
                        Id = Guid.NewGuid(),
                        Title = bookTitle,
                        Description = "Description",
                        SeriesId = seriesId,
                    },
                ],
            });
            context.Users.Add(new UserModel
            {
                Id = userId,
                Username = "User".Unique(),
            });
            context.Watchlist.Add(new WatchlistModel
            {
                Id = watchlistId,
                SeriesId = seriesId,
                UserId = userId,
            });
            await context.SaveChangesAsync();
        }

        this.watchlistScraperManager.Scrape(Arg.Any<SeriesDTO>())
            .Returns(
            [
                new()
                {
                    SeriesId = seriesId,
                    Volume = 2,
                    Title = bookTitle,
                },
            ]);

        // Execute
        await this.testee.UpdateWatchlist(watchlistId);

        // Assert
        using var context2 = new KapitelShelfDBContext(this.dbOptions);
        Assert.That(context2.WatchlistResults.Any(r => r.SeriesId == seriesId), Is.False);
    }

    /// <summary>
    /// Tests that UpdateWatchlist ignores volumes lower than the last existing volume.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateWatchlist_IgnoresLowerVolumes()
    {
        // Setup
        var seriesId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var watchlistId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "Series".Unique(),
                Books = [
                    new BookModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Vol2".Unique(),
                        Description = "Description",
                        SeriesId = seriesId,
                        SeriesNumber = 2,
                        ReleaseDate = DateTime.UtcNow,
                    },
                ],
            });
            context.Users.Add(new UserModel
            {
                Id = userId,
                Username = "User".Unique(),
            });
            context.Watchlist.Add(new WatchlistModel
            {
                Id = watchlistId,
                SeriesId = seriesId,
                UserId = userId,
            });
            await context.SaveChangesAsync();
        }

        this.watchlistScraperManager.Scrape(Arg.Any<SeriesDTO>())
            .Returns(
            [
                new()
                {
                    SeriesId = seriesId,
                    Volume = 1,
                    Title = "OldVolume",
                },
            ]);

        // Execute
        await this.testee.UpdateWatchlist(watchlistId);

        // Assert
        using var context2 = new KapitelShelfDBContext(this.dbOptions);
        Assert.That(context2.WatchlistResults.Any(r => r.SeriesId == seriesId), Is.False);
    }
}
