// <copyright file="SearchLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.Logic;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the SearchLogic class.
/// </summary>
[TestFixture]
public class SearchLogicTests
{
    private PostgreSqlContainer postgres;

    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private IMapper mapper;
    private SearchLogic testee;

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
            .UseNpgsql(postgres.GetConnectionString(), x => x.MigrationsAssembly("KapitelShelf.Data.Migrations"))
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
        this.testee = new SearchLogic(this.dbContextFactory, this.mapper);
    }

    /// <summary>
    /// Tests SearchBySearchterm returns empty, when the searchterm is whitespace.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SearchBySearchterm_ReturnsEmpty_WhenSearchtermIsNullOrWhitespace()
    {
        // Execute
        var empty = await this.testee.SearchBySearchterm(" ", 1, 10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(empty.TotalCount, Is.EqualTo(0));
            Assert.That(empty.Items, Is.Empty);
        });
    }

    /// <summary>
    /// Tests SearchBySearchterm returns a paged result, when a matching exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SearchBySearchterm_ReturnsPagedResult_WhenMatchingExists()
    {
        // Setup
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1".Unique(),
        };
        var book = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Example".Unique(),
            Description = "Description",
            SeriesId = series.Id,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.SearchBySearchterm(book.Title, 1, 10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items, Has.Count.EqualTo(1));
        });
        Assert.That(result.Items[0].Title, Is.EqualTo(book.Title));
    }

    /// <summary>
    /// Tests SearchBySearchterm paginates correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SearchBySearchterm_PaginatesCorrectly()
    {
        // Setup
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "ExampleSeries".Unique(),
        };

        var uniqueTitle = "Book".Unique();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);

            for (int i = 1; i <= 15; i++)
            {
                var book = new BookModel
                {
                    Id = Guid.NewGuid(),
                    Title = $"{uniqueTitle} {i}",
                    Description = "Description",
                    SeriesId = series.Id,
                };
                context.Books.Add(book);
            }

            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.SearchBySearchterm(uniqueTitle, page: 2, pageSize: 5);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result.Items, Has.Count.EqualTo(5));
        });
    }
}
