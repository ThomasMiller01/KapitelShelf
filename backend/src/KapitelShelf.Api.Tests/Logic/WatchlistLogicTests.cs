// <copyright file="WatchlistLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Logic.Interfaces.Storage;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using KapitelShelf.Data.Models.User;
using KapitelShelf.Data.Models.Watchlists;
using Microsoft.AspNetCore.Http;
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
    private Mapper mapper;
    private ISeriesLogic seriesLogic;
    private IBooksLogic booksLogic;
    private IWatchlistScraperManager watchlistScraperManager;
    private IBookStorage bookStorage;
    private IMetadataLogic metadataLogic;
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
        this.metadataLogic = Substitute.For<IMetadataLogic>();
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
            .Returns(Task.FromResult<SeriesDTO?>(this.mapper.SeriesModelToSeriesDto(seriesModel)));

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
            .Returns(Task.FromResult<SeriesDTO?>(this.mapper.SeriesModelToSeriesDto(seriesModel)));

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
            .Returns(Task.FromResult<SeriesDTO?>(this.mapper.SeriesModelToSeriesDto(seriesModel)));

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

    /// <summary>
    /// Tests that AddResultToLibrary returns null when the result does not exist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddResultToLibrary_ReturnsNull_WhenResultNotFound()
    {
        // Execute
        var result = await this.testee.AddResultToLibrary(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that AddResultToLibrary adds a result to the library successfully.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddResultToLibrary_AddsResultToLibrary_Successfully()
    {
        // Setup
        var resultId = Guid.NewGuid();
        var seriesId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "Series".Unique(),
            });
            context.WatchlistResults.Add(new WatchlistResultModel
            {
                Id = resultId,
                SeriesId = seriesId,
                Volume = 1,
                Title = "Test Book",
                LocationUrl = "ASIN".Unique(),
                LocationType = LocationType.Kindle,
                CoverUrl = "https://example.com/cover.png",
            });
            await context.SaveChangesAsync();
        }

        // Mock dependencies
        var createdBook = new BookDTO
        {
            Id = bookId,
            Title = "Test Book",
        };

        this.booksLogic.CreateBookAsync(Arg.Any<CreateBookDTO>())
            .Returns(Task.FromResult<BookDTO?>(createdBook));

        var coverBytes = new byte[] { 1, 2, 3 };
        this.metadataLogic.ProxyCover(Arg.Any<string>())
            .Returns(Task.FromResult<(byte[], string)>((coverBytes, "image/png")));

        var savedFile = new FileInfoDTO
        {
            FilePath = "/books/cover.png",
        };
        this.bookStorage.Save(bookId, Arg.Any<IFormFile>())
            .Returns(Task.FromResult(savedFile));

        // Execute
        var result = await this.testee.AddResultToLibrary(resultId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Title, Is.EqualTo("Test Book"));
            Assert.That(result.Cover, Is.Not.Null);
        });
        Assert.That(result.Cover!.FilePath, Is.EqualTo("/books/cover.png"));

        // Verify that the result has been removed from the DB
        using var context2 = new KapitelShelfDBContext(this.dbOptions);
        Assert.That(await context2.WatchlistResults.AnyAsync(r => r.Id == resultId), Is.False);

        // Verify that the book was updated
        await this.booksLogic.Received().UpdateBookAsync(bookId, Arg.Any<BookDTO>());
    }

    /// <summary>
    /// Tests that AddResultToLibrary returns null when book creation fails.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddResultToLibrary_ReturnsNull_WhenBookCreationFails()
    {
        // Setup
        var resultId = Guid.NewGuid();
        var seriesId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "Series".Unique(),
            });
            context.WatchlistResults.Add(new WatchlistResultModel
            {
                Id = resultId,
                SeriesId = seriesId,
                Volume = 1,
                Title = "Test Book",
                LocationUrl = "ASIN".Unique(),
                LocationType = LocationType.Kindle,
            });
            await context.SaveChangesAsync();
        }

        this.booksLogic.CreateBookAsync(Arg.Any<CreateBookDTO>())
            .Returns(Task.FromResult<BookDTO?>(null));

        // Execute
        var result = await this.testee.AddResultToLibrary(resultId);

        // Assert
        Assert.That(result, Is.Null);

        // Verify the watchlist result still exists
        using var context2 = new KapitelShelfDBContext(this.dbOptions);
        Assert.That(await context2.WatchlistResults.AnyAsync(r => r.Id == resultId), Is.True);
    }

    /// <summary>
    /// Tests that AddResultToLibrary does not upload a cover when none exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddResultToLibrary_SkipsCoverUpload_WhenNoCoverUrl()
    {
        // Setup
        var resultId = Guid.NewGuid();
        var seriesId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "Series".Unique(),
            });
            context.WatchlistResults.Add(new WatchlistResultModel
            {
                Id = resultId,
                SeriesId = seriesId,
                Volume = 1,
                Title = "Test Book",
                LocationUrl = "ASIN".Unique(),
                LocationType = LocationType.Kindle,
                CoverUrl = null,
            });
            await context.SaveChangesAsync();
        }

        this.booksLogic.CreateBookAsync(Arg.Any<CreateBookDTO>())
            .Returns(Task.FromResult<BookDTO?>(new()
            {
                Id = bookId,
                Title = "Test Book",
            }));

        // Execute
        var result = await this.testee.AddResultToLibrary(resultId);

        // Assert
        Assert.That(result, Is.Not.Null);
        await this.metadataLogic.DidNotReceiveWithAnyArgs().ProxyCover(default!);
        await this.bookStorage.DidNotReceiveWithAnyArgs().Save(default, default!);
        await this.booksLogic.Received(0).UpdateBookAsync(Arg.Any<Guid>(), Arg.Any<BookDTO>());
    }
}
