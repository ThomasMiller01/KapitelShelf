// <copyright file="SeriesLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Resources;
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
        var suffix = Guid.NewGuid().ToString("N");

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            for (var i = 1; i <= 15; i++)
            {
                context.Series.Add(new SeriesModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"Series_{i:000}_{suffix}",
                    UpdatedAt = DateTime.UtcNow,
                    Books = [],
                });
            }

            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.GetSeriesAsync(
            page: 1,
            pageSize: 10,
            sortBy: SeriesSortByDTO.Default,
            sortDir: SortDirectionDTO.Asc,
            filter: null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.Not.Zero);
            Assert.That(result.Items, Has.Count.EqualTo(10));
        });
    }

    /// <summary>
    /// Tests GetSeriesAsync applies filter and returns only matching results.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetSeriesAsync_AppliesFilter()
    {
        // Setup
        var matchName = $"Match_{Guid.NewGuid():N}";
        var otherName = $"Other_{Guid.NewGuid():N}";

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = Guid.NewGuid(),
                Name = matchName,
                UpdatedAt = DateTime.UtcNow,
                Books = [],
            });

            context.Series.Add(new SeriesModel
            {
                Id = Guid.NewGuid(),
                Name = otherName,
                UpdatedAt = DateTime.UtcNow,
                Books = [],
            });

            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.GetSeriesAsync(
            page: 1,
            pageSize: 10,
            sortBy: SeriesSortByDTO.Default,
            sortDir: SortDirectionDTO.Asc,
            filter: matchName);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].Name, Is.EqualTo(matchName));
        });
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
    /// Tests DeleteSeriesAsync(Guid) removes series, deletes files for its books and calls CleanupDatabase.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteSeriesAsync_RemovesSeries_DeletesBookFiles_AndCallsCleanup()
    {
        // Setup
        var seriesId = Guid.NewGuid();

        var series = new SeriesModel
        {
            Id = seriesId,
            Name = "DeleteSeries".Unique(),
            UpdatedAt = DateTime.UtcNow,
        };

        var book1Id = Guid.NewGuid();
        var book2Id = Guid.NewGuid();

        var book1 = new BookModel
        {
            Id = book1Id,
            Title = "DeleteSeries_Book1".Unique(),
            Description = "Description",
            SeriesId = seriesId,
            UpdatedAt = DateTime.UtcNow,
        };

        var book2 = new BookModel
        {
            Id = book2Id,
            Title = "DeleteSeries_Book2".Unique(),
            Description = "Description",
            SeriesId = seriesId,
            UpdatedAt = DateTime.UtcNow,
        };

        await using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.AddRange(book1, book2);
            await context.SaveChangesAsync();
        }

        this.booksLogic.CleanupDatabase().Returns(Task.CompletedTask);

        // Execute
        var result = await this.testee.DeleteSeriesAsync(seriesId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(seriesId));

        this.booksLogic.Received(1).DeleteFiles(book1Id);
        this.booksLogic.Received(1).DeleteFiles(book2Id);
        await this.booksLogic.Received(1).CleanupDatabase();

        await using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(await context.Series.AnyAsync(x => x.Id == seriesId), Is.False);
        }
    }

    /// <summary>
    /// Tests DeleteSeriesAsync(Guid) does not call DeleteFiles when the series has no books.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteSeriesAsync_DoesNotDeleteFiles_WhenSeriesHasNoBooks()
    {
        // Setup
        var seriesId = Guid.NewGuid();

        await using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "DeleteSeries_NoBooks".Unique(),
                UpdatedAt = DateTime.UtcNow,
                Books = [],
            });

            await context.SaveChangesAsync();
        }

        this.booksLogic.CleanupDatabase().Returns(Task.CompletedTask);

        // Execute
        var result = await this.testee.DeleteSeriesAsync(seriesId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(seriesId));

        this.booksLogic.DidNotReceiveWithAnyArgs().DeleteFiles(default);
        await this.booksLogic.Received(1).CleanupDatabase();

        await using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(await context.Series.AnyAsync(x => x.Id == seriesId), Is.False);
        }
    }

    /// <summary>
    /// Tests DeleteSeriesAsync(Guid) is idempotent: calling twice does not throw and ends with deleted record.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteSeriesAsync_IsIdempotent()
    {
        // Setup
        var seriesId = Guid.NewGuid();

        await using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "DeleteSeries_Idempotent".Unique(),
                UpdatedAt = DateTime.UtcNow,
            });

            await context.SaveChangesAsync();
        }

        this.booksLogic.CleanupDatabase().Returns(Task.CompletedTask);

        // Execute
        var first = await this.testee.DeleteSeriesAsync(seriesId);
        var second = await this.testee.DeleteSeriesAsync(seriesId);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Null);
        });

        await this.booksLogic.Received(1).CleanupDatabase();

        await using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(await context.Series.AnyAsync(x => x.Id == seriesId), Is.False);
        }
    }

    /// <summary>
    /// Tests DeleteSeriesAsync(List{Guid}) deletes all existing series, deletes files for their books and calls CleanupDatabase once.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteSeriesAsyncList_DeletesAllExisting_DeletesFiles_AndCallsCleanupOnce()
    {
        // Setup
        var series1Id = Guid.NewGuid();
        var series2Id = Guid.NewGuid();

        var book1Id = Guid.NewGuid();
        var book2Id = Guid.NewGuid();

        await using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.AddRange(
                new SeriesModel
                {
                    Id = series1Id,
                    Name = "DeleteSeries_List_1".Unique(),
                    UpdatedAt = DateTime.UtcNow,
                },
                new SeriesModel
                {
                    Id = series2Id,
                    Name = "DeleteSeries_List_2".Unique(),
                    UpdatedAt = DateTime.UtcNow,
                });

            context.Books.AddRange(
                new BookModel
                {
                    Id = book1Id,
                    Title = "DeleteSeries_List_Book1".Unique(),
                    Description = "Description",
                    SeriesId = series1Id,
                    UpdatedAt = DateTime.UtcNow,
                },
                new BookModel
                {
                    Id = book2Id,
                    Title = "DeleteSeries_List_Book2".Unique(),
                    Description = "Description",
                    SeriesId = series2Id,
                    UpdatedAt = DateTime.UtcNow,
                });

            await context.SaveChangesAsync();
        }

        this.booksLogic.CleanupDatabase().Returns(Task.CompletedTask);

        // Execute
        await this.testee.DeleteSeriesAsync([series1Id, series2Id]);

        // Assert
        this.booksLogic.Received(1).DeleteFiles(book1Id);
        this.booksLogic.Received(1).DeleteFiles(book2Id);
        await this.booksLogic.Received(1).CleanupDatabase();

        await using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.Multiple(() =>
            {
                Assert.That(context.Series.Any(x => x.Id == series1Id), Is.False);
                Assert.That(context.Series.Any(x => x.Id == series2Id), Is.False);
            });
        }
    }

    /// <summary>
    /// Tests DeleteSeriesAsync(List{Guid}) ignores unknown ids and deletes only existing series.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteSeriesAsyncList_IgnoresUnknownIds_AndDeletesExisting()
    {
        // Setup
        var seriesId = Guid.NewGuid();
        var unknownId = Guid.NewGuid();

        var bookId = Guid.NewGuid();

        await using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "DeleteSeries_List_Existing".Unique(),
                UpdatedAt = DateTime.UtcNow,
            });

            context.Books.Add(new BookModel
            {
                Id = bookId,
                Title = "DeleteSeries_List_ExistingBook".Unique(),
                Description = "Description",
                SeriesId = seriesId,
                UpdatedAt = DateTime.UtcNow,
            });

            await context.SaveChangesAsync();
        }

        this.booksLogic.CleanupDatabase().Returns(Task.CompletedTask);

        // Execute
        await this.testee.DeleteSeriesAsync([seriesId, unknownId]);

        // Assert
        this.booksLogic.Received(1).DeleteFiles(bookId);
        await this.booksLogic.Received(1).CleanupDatabase();

        await using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.Multiple(() =>
            {
                Assert.That(context.Series.Any(x => x.Id == seriesId), Is.False);
                Assert.That(context.Series.Any(x => x.Id == unknownId), Is.False);
            });
        }
    }

    /// <summary>
    /// Tests DeleteSeriesAsync(List{Guid}) handles duplicate ids in input (delete once, cleanup once).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteSeriesAsyncList_HandlesDuplicateIdsInInput()
    {
        // Setup
        var seriesId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        await using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "DeleteSeries_List_DuplicateIds".Unique(),
                UpdatedAt = DateTime.UtcNow,
            });

            context.Books.Add(new BookModel
            {
                Id = bookId,
                Title = "DeleteSeries_List_DuplicateIds_Book".Unique(),
                Description = "Description",
                SeriesId = seriesId,
                UpdatedAt = DateTime.UtcNow,
            });

            await context.SaveChangesAsync();
        }

        this.booksLogic.CleanupDatabase().Returns(Task.CompletedTask);

        // Execute
        await this.testee.DeleteSeriesAsync([seriesId, seriesId, seriesId]);

        // Assert
        this.booksLogic.Received(1).DeleteFiles(bookId);
        await this.booksLogic.Received(1).CleanupDatabase();

        await using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Series.Any(x => x.Id == seriesId), Is.False);
        }
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
    /// Tests AutocompleteSeriesAsync returns empty when input is null/whitespace.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>A task.</returns>
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task AutocompleteSeriesAsync_ReturnsEmpty_WhenInputIsNullOrWhitespace(string? input)
    {
        // execute
        var result = await this.testee.AutocompleteAsync(input);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Tests AutocompleteSeriesAsync returns matching series and respects the 10 item limit.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AutocompleteSeriesAsync_ReturnsMatches_AndLimitsTo10()
    {
        // setup
        var prefix = "AutoSeries".Unique();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            // 12 matching
            for (int i = 0; i < 12; i++)
            {
                context.Series.Add(new SeriesModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"{prefix} {i}".Unique(),
                });
            }

            // 3 non-matching
            for (int i = 0; i < 3; i++)
            {
                context.Series.Add(new SeriesModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"OtherSeries {i}".Unique(),
                });
            }

            await context.SaveChangesAsync();
        }

        // execute
        var result = await this.testee.AutocompleteAsync(prefix);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(5));
        Assert.That(result.All(x => x.Contains(prefix, StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    /// <summary>
    /// Tests AutocompleteSeriesAsync returns empty when nothing matches.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AutocompleteSeriesAsync_ReturnsEmpty_WhenNoMatches()
    {
        // setup
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(new SeriesModel
            {
                Id = Guid.NewGuid(),
                Name = "NoMatchSeries".Unique(),
            });
            await context.SaveChangesAsync();
        }

        // execute
        var result = await this.testee.AutocompleteAsync("definitely-does-not-match".Unique());

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
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
        await this.testee.MergeSeries(targetId, [sourceId]);

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

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.testee.MergeSeries(targetId, [unknownSource]));
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

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.testee.MergeSeries(unknownTarget, [sourceId]));
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
