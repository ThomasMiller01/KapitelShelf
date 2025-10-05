// <copyright file="SeriesLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the SeriesLogic class.
/// </summary>
[TestFixture]
public class SeriesLogicTests
{
    private PostgreSqlContainer postgres;

    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private Mapper mapper;
    private IBooksLogic booksLogic;
    private SeriesLogic testee;

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
        this.booksLogic = Substitute.For<IBooksLogic>();
        this.testee = new SeriesLogic(this.dbContextFactory, this.mapper, this.booksLogic);
    }

    /// <summary>
    /// Tests GetSeriesAsync returns paged results.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetSeriesAsync_ReturnsPagedResult()
    {
        // Setup
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1".Unique(),
            UpdatedAt = DateTime.UtcNow,
            Books = [],
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.GetSeriesAsync(1, 10);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    /// <summary>
    /// Tests GetSeriesByIdAsync returns SeriesDTO when found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetSeriesByIdAsync_ReturnsSeriesDTO_WhenFound()
    {
        // Setup
        var id = Guid.NewGuid();
        var series = new SeriesModel
        {
            Id = id,
            Name = "FoundSeries".Unique(),
        };
        var book = new BookModel
        {
            Id = Guid.NewGuid(),
            SeriesId = id,
            Title = "Book1".Unique(),
            Description = "Description1",
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.GetSeriesByIdAsync(id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(id));
            Assert.That(result.TotalBooks, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// Tests GetSeriesByIdAsync returns null when not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetSeriesByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Execute
        var result = await this.testee.GetSeriesByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests GetBooksBySeriesIdAsync returns paged results.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetBooksBySeriesIdAsync_ReturnsPagedResult()
    {
        // Setup
        var seriesId = Guid.NewGuid();
        var book = new BookModel
        {
            Id = Guid.NewGuid(),
            SeriesId = seriesId,
            SeriesNumber = 1,
            Title = "Book1".Unique(),
            Description = "Description1",
        };
        var series = new SeriesModel
        {
            Id = seriesId,
            Name = "Series".Unique(),
            Books = [book],
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.GetBooksBySeriesIdAsync(seriesId, 1, 10);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items[0].Id, Is.EqualTo(book.Id));
        });
    }

    /// <summary>
    /// Tests GetBooksBySeriesIdAsync returns empty result if no books.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetBooksBySeriesIdAsync_ReturnsEmpty_WhenNoBooks()
    {
        // Setup
        var seriesId = Guid.NewGuid();

        // Execute
        var result = await this.testee.GetBooksBySeriesIdAsync(seriesId, 1, 10);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.Zero);
            Assert.That(result.Items, Is.Empty);
        });
    }

    /// <summary>
    /// Tests DeleteSeriesAsync removes series and calls CleanupDatabase.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteSeriesAsync_RemovesSeriesAndCallsCleanup()
    {
        // Setup
        var id = Guid.NewGuid();
        var series = new SeriesModel
        {
            Id = id,
            Name = "DeleteMe".Unique(),
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            await context.SaveChangesAsync();
        }

        this.booksLogic.CleanupDatabase().Returns(Task.CompletedTask);

        // Execute
        var result = await this.testee.DeleteSeriesAsync(id);

        // Assert
        Assert.That(result, Is.Not.Null);
        await this.booksLogic.Received(1).CleanupDatabase();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(await context.Series.FindAsync(id), Is.Null);
        }
    }

    /// <summary>
    /// Tests DeleteSeriesAsync returns null when series is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteSeriesAsync_ReturnsNull_IfNotFound()
    {
        // Execute
        var result = await this.testee.DeleteSeriesAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests UpdateSeriesAsync throws if a duplicate exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdateSeriesAsync_ThrowsOnDuplicate()
    {
        // Setup
        var id = Guid.NewGuid();
        var name = "DuplicateSeries".Unique();
        var seriesDto = new SeriesDTO
        {
            Id = id,
            Name = name,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            // add a duplicate series with same name
            context.Series.Add(new SeriesModel
            {
                Id = Guid.NewGuid(),
                Name = name,
            });
            await context.SaveChangesAsync();
        }

        // Execute, Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await this.testee.UpdateSeriesAsync(id, seriesDto));
        Assert.That(ex!.Message, Is.EqualTo(StaticConstants.DuplicateExceptionKey));
    }

    /// <summary>
    /// Tests UpdateSeriesAsync updates the entity and returns DTO.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdateSeriesAsync_UpdatesAndReturnsDTO()
    {
        // Setup
        var id = Guid.NewGuid();
        var series = new SeriesModel
        {
            Id = id,
            Name = "Original".Unique(),
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            await context.SaveChangesAsync();
        }

        var updatedDto = new SeriesDTO
        {
            Id = id,
            Name = "Updated".Unique(),
        };

        // Execute
        var result = await this.testee.UpdateSeriesAsync(id, updatedDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo(updatedDto.Name));

            using var context = new KapitelShelfDBContext(this.dbOptions);
            Assert.That(context.Series.Single(x => x.Id == id).Name, Is.EqualTo(updatedDto.Name));
        });
    }

    /// <summary>
    /// Tests UpdateSeriesAsync returns null if series not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdateSeriesAsync_ReturnsNull_IfSeriesNotFound()
    {
        // Setup
        var id = Guid.NewGuid();
        var seriesDto = new SeriesDTO
        {
            Id = id,
            Name = "NonExistent".Unique(),
        };

        // Execute
        var result = await this.testee.UpdateSeriesAsync(id, seriesDto);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests DeleteFilesAsync calls DeleteFiles for all book IDs.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteFilesAsync_DeletesFilesForEachBook()
    {
        // Setup
        var seriesId = Guid.NewGuid();
        var book1 = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book1".Unique(),
            Description = "Description1",
        };
        var book2 = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book2".Unique(),
            Description = "Description2",
        };
        var series = new SeriesModel
        {
            Id = seriesId,
            Name = "Series1".Unique(),
            Books = [book1, book2],
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.AddRange(book1, book2);
            await context.SaveChangesAsync();
        }

        // Execute
        await this.testee.DeleteFilesAsync(seriesId);

        // Assert
        this.booksLogic.Received(1).DeleteFiles(book1.Id);
        this.booksLogic.Received(1).DeleteFiles(book2.Id);
    }

    /// <summary>
    /// Tests MergeSeries moves all books from source to target and deletes source.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MergeSeries_MovesBooksAndDeletesSource()
    {
        // Setup
        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var book = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "MergeBook".Unique(),
            Description = "Description",
            SeriesId = sourceId,
        };
        var sourceSeries = new SeriesModel
        {
            Id = sourceId,
            Name = "SourceSeries".Unique(),
        };
        var targetSeries = new SeriesModel
        {
            Id = targetId,
            Name = "TargetSeries".Unique(),
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.AddRange(sourceSeries, targetSeries);
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        // Execute
        await this.testee.MergeSeries(sourceId, targetId);

        // Assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var updatedBook = await context.Books.FindAsync(book.Id);
            Assert.That(updatedBook, Is.Not.Null);
            Assert.That(updatedBook!.SeriesId, Is.EqualTo(targetId));

            var source = await context.Series.FindAsync(sourceId);
            Assert.That(source, Is.Null);

            var target = await context.Series
                .Include(s => s.Books)
                .FirstOrDefaultAsync(s => s.Id == targetId);
            Assert.That(target, Is.Not.Null);
            Assert.That(target!.Books.Any(b => b.Id == book.Id), Is.True);
        }
    }

    /// <summary>
    /// Tests MergeSeries throws if source series does not exist.
    /// </summary>
    [Test]
    public void MergeSeries_ThrowsOnUnknownSource()
    {
        var unknownSource = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = targetId,
                Name = "Target".Unique(),
            });
            context.SaveChanges();
        }

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.testee.MergeSeries(unknownSource, targetId));
        Assert.That(ex!.Message, Does.Contain("Unknown source series id"));
    }

    /// <summary>
    /// Tests MergeSeries throws if target series does not exist.
    /// </summary>
    [Test]
    public void MergeSeries_ThrowsOnUnknownTarget()
    {
        var sourceId = Guid.NewGuid();
        var unknownTarget = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = sourceId,
                Name = "Source".Unique(),
            });
            context.SaveChanges();
        }

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.testee.MergeSeries(sourceId, unknownTarget));
        Assert.That(ex!.Message, Does.Contain("Unknown target series id"));
    }

    /// <summary>
    /// Tests GetDuplicatesAsync returns matching series by name.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetDuplicatesAsync_ReturnsMatchingSeries()
    {
        // Setup
        var name = "DuplicateName".Unique();
        var match = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = name,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(match);
            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.GetDuplicatesAsync(name);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo(name));
    }

    /// <summary>
    /// Tests GetDuplicatesAsync returns empty if no match.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetDuplicatesAsync_ReturnsEmpty_IfNoMatch()
    {
        // Execute
        var result = await this.testee.GetDuplicatesAsync("NoSuchSeries".Unique());

        // Assert
        Assert.That(result, Is.Empty);
    }
}
