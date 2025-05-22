// <copyright file="SeriesLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the SeriesLogic class.
/// </summary>
[TestFixture]
public class SeriesLogicTests
{
    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private IMapper mapper;
    private IBooksLogic booksLogic;
    private SeriesLogic testee;

    /// <summary>
    /// Sets up a new in-memory database and fakes before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.dbOptions = new DbContextOptionsBuilder<KapitelShelfDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        this.dbContextFactory = Substitute.For<IDbContextFactory<KapitelShelfDBContext>>();
        this.dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(new KapitelShelfDBContext(this.dbOptions)));

        this.mapper = Substitute.For<IMapper>();
        this.booksLogic = Substitute.For<IBooksLogic>();
        this.testee = new SeriesLogic(this.dbContextFactory, this.mapper, this.booksLogic);
    }

    /// <summary>
    /// Tests GetSeriesSummaryAsync returns paged results.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    [Obsolete("This test is obsolete because the method it tests is deprecated.")]
    public async Task GetSeriesSummaryAsync_ReturnsPagedResult()
    {
        // Setup
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "SummarySeries",
            UpdatedAt = DateTime.UtcNow,
            Books = [],
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            await context.SaveChangesAsync();
        }

        var summaryDto = new SeriesSummaryDTO
        {
            Name = series.Name,
        };
        this.mapper.Map<SeriesSummaryDTO>(Arg.Any<SeriesModel>()).Returns(summaryDto);

        // Execute
        var result = await this.testee.GetSeriesSummaryAsync(1, 10);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items[0].Name, Is.EqualTo(series.Name));
        });
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
            Name = "Series1",
            UpdatedAt = DateTime.UtcNow,
            Books = [],
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            await context.SaveChangesAsync();
        }

        var dto = new SeriesDTO
        {
            Id = series.Id,
            Name = series.Name,
        };
        this.mapper.Map<SeriesDTO>(Arg.Any<SeriesModel>()).Returns(dto);

        // Execute
        var result = await this.testee.GetSeriesAsync(1, 10);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items[0].Name, Is.EqualTo(series.Name));
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
            Name = "FoundSeries",
        };
        var book = new BookModel
        {
            Id = Guid.NewGuid(),
            SeriesId = id,
            Title = "Book1",
            Description = "Description1",
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        var dto = new SeriesDTO
        {
            Id = id,
            Name = "FoundSeries",
        };
        this.mapper.Map<SeriesDTO>(Arg.Any<SeriesModel>()).Returns(dto);

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
            Title = "Book1",
            Description = "Description1",
        };
        var series = new SeriesModel
        {
            Id = seriesId,
            Name = "Series1",
            Books = [book],
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        var bookDto = new BookDTO
        {
            Id = book.Id,
        };
        this.mapper.Map<BookDTO>(Arg.Any<BookModel>()).Returns(bookDto);

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
            Assert.That(result.TotalCount, Is.EqualTo(0));
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
            Name = "DeleteMe",
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            await context.SaveChangesAsync();
        }

        var dto = new SeriesDTO
        {
            Id = id,
            Name = "DeleteMe",
        };
        this.mapper.Map<SeriesDTO>(Arg.Any<SeriesModel>()).Returns(dto);
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
        var name = "DuplicateSeries";
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
            Name = "Original",
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            await context.SaveChangesAsync();
        }

        var updatedDto = new SeriesDTO
        {
            Id = id,
            Name = "Updated",
        };
        this.mapper.Map<SeriesDTO>(Arg.Any<SeriesModel>()).Returns(updatedDto);

        // Execute
        var result = await this.testee.UpdateSeriesAsync(id, updatedDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("Updated"));

            using var context = new KapitelShelfDBContext(this.dbOptions);
            Assert.That(context.Series.Single(x => x.Id == id).Name, Is.EqualTo("Updated"));
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
            Name = "NonExistent",
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
            Title = "Book1",
            Description = "Description1",
        };
        var book2 = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book2",
            Description = "Description2",
        };
        var series = new SeriesModel
        {
            Id = seriesId,
            Name = "Series1",
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
    /// Tests GetDuplicatesAsync returns matching series by name.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetDuplicatesAsync_ReturnsMatchingSeries()
    {
        // Setup
        var name = "DuplicateName";
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
        var result = await this.testee.GetDuplicatesAsync("NoSuchSeries");

        // Assert
        Assert.That(result, Is.Empty);
    }
}
