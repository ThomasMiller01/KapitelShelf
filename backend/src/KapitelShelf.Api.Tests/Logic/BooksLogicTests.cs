// <copyright file="BooksLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Logic.Interfaces.MetadataScraper;
using KapitelShelf.Api.Logic.Interfaces.Storage;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using KapitelShelf.Data.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the BooksLogic class.
/// </summary>
[TestFixture]
public class BooksLogicTests
{
    private PostgreSqlContainer postgres;

    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private IMapper mapper;
    private IBookParserManager bookParserManager;
    private IBookStorage bookStorage;
    private IMetadataScraperManager metadataScraperManager;
    private BooksLogic testee;

    /// <summary>
    /// Setup database container.
    /// </summary>
    /// <returns>A task.</returns>
    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        this.postgres = new PostgreSqlBuilder()
            .WithDatabase("kapitelshelf")
            .WithUsername("kapitelshelf")
            .WithPassword("kapitelshelf")
            .WithImage("postgres:16.8")
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
    /// Sets up the testee and fakes before each test.
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

        this.bookParserManager = Substitute.For<IBookParserManager>();
        this.bookStorage = Substitute.For<IBookStorage>();
        this.metadataScraperManager = Substitute.For<IMetadataScraperManager>();
        this.testee = new BooksLogic(this.dbContextFactory, this.mapper, this.bookParserManager, this.bookStorage, this.metadataScraperManager);
    }

    /// <summary>
    /// Tests GetBooksAsync returns books.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetBooksAsync_ReturnsBooks()
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
            Title = "Book1".Unique(),
            Description = "Description",
            PageNumber = 7,
            SeriesId = series.Id,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            context.SaveChanges();
        }

        // Execute
        var result = await this.testee.GetBooksAsync();

        // Assert
        Assert.That(result, Has.Count.GreaterThanOrEqualTo(1));
    }

    /// <summary>
    /// Tests GetBookByIdAsync returns book when found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetBookByIdAsync_ReturnsBook_WhenFound()
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
            Title = "BookById".Unique(),
            Description = "Description",
            PageNumber = 2,
            SeriesId = series.Id,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            context.SaveChanges();
        }

        // Execute
        var result = await this.testee.GetBookByIdAsync(book.Id, null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Title, Is.EqualTo(book.Title));
            Assert.That(result.Description, Is.EqualTo(book.Description));
            Assert.That(result.PageNumber, Is.EqualTo(book.PageNumber));
        });
    }

    /// <summary>
    /// Tests GetBookByIdAsync returns null when not found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetBookByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Execute
        var result = await this.testee.GetBookByIdAsync(Guid.NewGuid(), null);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests SearchBySearchterm returns empty, when the searchterm is whitespace.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SearchBySearchterm_ReturnsEmpty_WhenSearchtermIsNullOrWhitespace()
    {
        // Execute
        var empty = await this.testee.Search(" ", 1, 10);

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
        var result = await this.testee.Search(book.Title, 1, 10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items, Has.Count.EqualTo(1));
        });
        Assert.That(result.Items[0].Title, Is.EqualTo(book.Title));
    }

    /// <summary>
    /// Tests MarkBookAsVisited adds a new entry if none exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetBookByIdAsync_AddsVisitedBook_WhenNoneExists()
    {
        // Setup
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var user = new UserModel
        {
            Id = userId,
            Username = "User1".Unique(),
            Image = ProfileImageType.MonsieurRead,
            Color = "#33ffff",
        };
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1".Unique(),
        };
        var book = new BookModel
        {
            Id = bookId,
            Title = "MarkVisitedTest".Unique(),
            Description = "Description",
            SeriesId = series.Id,
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Users.Add(user);
            context.Series.Add(series);
            context.Books.Add(book);
            context.SaveChanges();
        }

        // Execute
        var result = await this.testee.GetBookByIdAsync(bookId, userId);

        // Assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var visited = context.VisitedBooks.FirstOrDefault(x => x.BookId == bookId && x.UserId == userId);
            Assert.That(visited, Is.Not.Null);
            Assert.That(visited.VisitedAt, Is.Not.EqualTo(default(DateTime)));
        }
    }

    /// <summary>
    /// Tests MarkBookAsVisited updates VisitedAt if entry exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetBookByIdAsync_UpdatesVisitedAt_WhenEntryExists()
    {
        // Setup
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var user = new UserModel
        {
            Id = userId,
            Username = "User1".Unique(),
            Image = ProfileImageType.MonsieurRead,
            Color = "#33ffff",
        };
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series2".Unique(),
        };
        var book = new BookModel
        {
            Id = bookId,
            Title = "MarkVisitedUpdate".Unique(),
            Description = "Description",
            SeriesId = series.Id,
        };

        var oldTime = DateTime.UtcNow.AddDays(-1);
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Users.Add(user);
            context.Series.Add(series);
            context.Books.Add(book);
            context.VisitedBooks.Add(new VisitedBooksModel
            {
                BookId = bookId,
                UserId = userId,
                VisitedAt = oldTime,
            });
            context.SaveChanges();
        }

        // Execute
        var result = await this.testee.GetBookByIdAsync(bookId, userId);

        // Assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var visited = context.VisitedBooks.FirstOrDefault(x => x.BookId == bookId && x.UserId == userId);
            Assert.That(visited, Is.Not.Null);
            Assert.That(visited.VisitedAt, Is.GreaterThan(oldTime));
        }
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
        var result = await this.testee.Search(uniqueTitle, page: 2, pageSize: 5);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result.Items, Has.Count.EqualTo(5));
        });
    }

    /// <summary>
    /// Tests CreateBookAsync returns null when input is null.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task CreateBookAsync_ReturnsNull_IfNull()
    {
        // Execute
        var result = await this.testee.CreateBookAsync(null!);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests CreateBookAsync throws if duplicate exists.
    /// </summary>
    [Test]
    public void CreateBookAsync_ThrowsOnDuplicate()
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
            Title = "Duplicate".Unique(),
            Description = "Description",
            SeriesId = series.Id,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            context.SaveChanges();
        }

        var createDto = new CreateBookDTO
        {
            Title = book.Title,
            Categories = [],
            Tags = [],
        };

        // Execute
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => this.testee.CreateBookAsync(createDto));

        // Assert
        Assert.That(ex!.Message, Is.EqualTo(StaticConstants.DuplicateExceptionKey));
    }

    /// <summary>
    /// Tests CreateBookAsync creates a new book and sets all fields.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task CreateBookAsync_CreatesBook_WithAllAttributes()
    {
        // Setup
        var author = new CreateAuthorDTO
        {
            FirstName = "A".Unique(),
            LastName = "B".Unique(),
        };
        var category = new CreateCategoryDTO
        {
            Name = "Category".Unique(),
        };
        var tag = new CreateTagDTO
        {
            Name = "Tag".Unique(),
        };
        var series = new CreateSeriesDTO
        {
            Name = "Series".Unique(),
        };
        var createDto = new CreateBookDTO
        {
            Title = "NewBook".Unique(),
            Description = "Description",
            PageNumber = 42,
            ReleaseDate = new DateTime(2023, 1, 1).ToUniversalTime(),
            Author = author,
            Series = series,
            Categories = [category],
            Tags = [tag],
        };

        // Execute
        var result = await this.testee.CreateBookAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Title, Is.EqualTo(createDto.Title));
            Assert.That(result.Description, Is.EqualTo(createDto.Description));
            Assert.That(result.PageNumber, Is.EqualTo(createDto.PageNumber));
            Assert.That(result.ReleaseDate, Is.EqualTo(createDto.ReleaseDate));
            Assert.That(result.Author!.FirstName, Is.EqualTo(author.FirstName));
            Assert.That(result.Author.LastName, Is.EqualTo(author.LastName));
            Assert.That(result.Series, Is.Not.Null);
            Assert.That(result.Series!.Name, Is.EqualTo(series.Name));
            Assert.That(result.Categories, Has.Count.EqualTo(1));
            Assert.That(result.Categories![0].Name, Is.EqualTo(category.Name));
            Assert.That(result.Tags, Has.Count.EqualTo(1));
            Assert.That(result.Tags![0].Name, Is.EqualTo(tag.Name));
        });
    }

    /// <summary>
    /// Tests that CreateBookAsync sets the series to the title when the series is not provided.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the test finishes.</returns>
    [Test]
    public async Task CreateBookAsync_SetsSeriesToTitle_WhenSeriesNotSet()
    {
        // Setup
        var createDto = new CreateBookDTO
        {
            Title = "BookTitle".Unique(),
            Description = "Description",
            Series = null,
            Categories = [],
            Tags = [],
        };

        // Execute
        var result = await this.testee.CreateBookAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Series, Is.Not.Null);
        Assert.That(result.Series!.Name, Is.EqualTo(createDto.Title));
    }

    /// <summary>
    /// Tests that CreateBookAsync reuses an existing series if the name matches.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the test finishes.</returns>
    [Test]
    public async Task CreateBookAsync_UsesExistingSeries_WhenSeriesExists()
    {
        // Setup
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "BookSeries".Unique(),
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            await context.SaveChangesAsync();
        }

        var createDto = new CreateBookDTO
        {
            Title = "Book".Unique(),
            Description = "Description",
            Series = new CreateSeriesDTO
            {
                Name = series.Name,
            },
            Categories = [],
            Tags = [],
        };

        // Execute
        var result = await this.testee.CreateBookAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Series?.Id, Is.EqualTo(series.Id));
    }

    /// <summary>
    /// Tests that CreateBookAsync reuses an existing author if the name matches.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the test finishes.</returns>
    [Test]
    public async Task CreateBookAsync_UsesExistingAuthor_WhenAuthorExists()
    {
        // Setup
        var author = new AuthorModel
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane".Unique(),
            LastName = "Doe".Unique(),
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Authors.Add(author);
            await context.SaveChangesAsync();
        }

        var createDto = new CreateBookDTO
        {
            Title = "Book".Unique(),
            Description = "Description",
            Author = new CreateAuthorDTO
            {
                FirstName = author.FirstName,
                LastName = author.LastName,
            },
            Series = new CreateSeriesDTO
            {
                Name = "Series".Unique(),
            },
            Categories = [],
            Tags = [],
        };

        // Execute
        var result = await this.testee.CreateBookAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    /// <summary>
    /// Tests that CreateBookAsync reuses existing categories.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the test finishes.</returns>
    [Test]
    public async Task CreateBookAsync_UsesExistingCategories()
    {
        // Setup
        var category = new CategoryModel
        {
            Id = Guid.NewGuid(),
            Name = "Category1".Unique(),
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        }

        var createDto = new CreateBookDTO
        {
            Title = "Book".Unique(),
            Description = "Description",
            Series = new CreateSeriesDTO
            {
                Name = "Series".Unique(),
            },
            Categories = [
                new CreateCategoryDTO
                {
                    Name = category.Name,
                },
            ],
            Tags = [],
        };

        // Execute
        var result = await this.testee.CreateBookAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    /// <summary>
    /// Tests that CreateBookAsync reuses existing tags.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the test finishes.</returns>
    [Test]
    public async Task CreateBookAsync_UsesExistingTags()
    {
        // Setup
        var tag = new TagModel
        {
            Id = Guid.NewGuid(),
            Name = "Tag1".Unique(),
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Tags.Add(tag);
            await context.SaveChangesAsync();
        }

        var createDto = new CreateBookDTO
        {
            Title = "Book".Unique(),
            Description = "Description",
            Series = new CreateSeriesDTO
            {
                Name = "Series".Unique(),
            },
            Categories = [],
            Tags = [
                new CreateTagDTO
                {
                    Name = tag.Name,
                },
            ],
        };

        // Execute
        var result = await this.testee.CreateBookAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    /// <summary>
    /// Tests UpdateBookAsync throws if duplicate exists.
    /// </summary>
    [Test]
    public void UpdateBookAsync_ThrowsOnDuplicate()
    {
        // Setup
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1".Unique(),
        };
        var book1 = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book1".Unique(),
            Description = "Description1",
            SeriesId = series.Id,
        };
        var book2 = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book2".Unique(),
            Description = "Description2",
            SeriesId = series.Id,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book1);
            context.Books.Add(book2);
            context.SaveChanges();
        }

        var dto = new BookDTO
        {
            Id = book1.Id,
            Title = book2.Title,
            Categories = [],
            Tags = [],
        };

        // Execute
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => this.testee.UpdateBookAsync(book1.Id, dto));

        // Assert
        Assert.That(ex!.Message, Is.EqualTo(StaticConstants.DuplicateExceptionKey));
    }

    /// <summary>
    /// Tests UpdateBookAsync throws ArgumentNullException if Series is null.
    /// </summary>
    [Test]
    public void UpdateBookAsync_ThrowsArgumentNullException_IfSeriesIsNull()
    {
        // Setup
        // Setup
        var id = Guid.NewGuid();
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1".Unique(),
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Old".Unique(),
            Description = "OldDesc",
            PageNumber = 100,
            ReleaseDate = new DateTime(2000, 1, 1).ToUniversalTime(),
            SeriesId = series.Id,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            context.SaveChanges();
        }

        var bookDto = new BookDTO
        {
            Id = id,
            Title = "Title".Unique(),

            // intentionally set Series to null
            Series = null,

            Author = new AuthorDTO
            {
                FirstName = "First".Unique(),
                LastName = "Last".Unique(),
            },
            Categories = [],
            Tags = [],
        };

        // Execute and Assert
        var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await this.testee.UpdateBookAsync(id, bookDto);
        });

        Assert.Multiple(() =>
        {
            Assert.That(ex.ParamName, Is.EqualTo("Series"));
            Assert.That(ex.Message, Does.Contain("Series cannot be null"));
        });
    }

    /// <summary>
    /// Tests UpdateBookAsync returns null if book not found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateBookAsync_ReturnsNull_IfNotFound()
    {
        // Setup
        var dto = new BookDTO
        {
            Id = Guid.NewGuid(),
            Title = "NotFound".Unique(),
            Categories = [],
            Tags = [],
        };

        // Execute
        var result = await this.testee.UpdateBookAsync(dto.Id, dto);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests UpdateBookAsync updates all attributes.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateBookAsync_UpdatesBook_AllAttributes()
    {
        // Setup
        var id = Guid.NewGuid();
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1".Unique(),
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Old".Unique(),
            Description = "OldDesc",
            PageNumber = 100,
            ReleaseDate = new DateTime(2000, 1, 1).ToUniversalTime(),
            SeriesId = series.Id,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            context.SaveChanges();
        }

        var author = new AuthorDTO
        {
            FirstName = "X".Unique(),
            LastName = "Y".Unique(),
        };
        var category = new CategoryDTO
        {
            Name = "C1".Unique(),
        };
        var tag = new TagDTO
        {
            Name = "T1".Unique(),
        };
        var cover = new FileInfoDTO
        {
            FilePath = "/cover.png",
            FileSizeBytes = 123,
            MimeType = "image/png",
            Sha256 = "hash",
        };
        var location = new LocationDTO
        {
            Type = LocationTypeDTO.KapitelShelf,
            Url = "url",
            FileInfo = cover,
        };

        var updatedDto = new BookDTO
        {
            Id = id,
            Title = "NewTitle".Unique(),
            Description = "DescNew",
            PageNumber = 42,
            ReleaseDate = new DateTime(2025, 1, 1).ToUniversalTime(),
            Author = author,
            Series = new SeriesDTO
            {
                Name = "SeriesNew".Unique(),
            },
            Categories = [category],
            Tags = [tag],
            Cover = cover,
            Location = location,
        };

        // Execute
        var result = await this.testee.UpdateBookAsync(id, updatedDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Title, Is.EqualTo(updatedDto.Title));
            Assert.That(result.Description, Is.EqualTo(updatedDto.Description));
            Assert.That(result.PageNumber, Is.EqualTo(updatedDto.PageNumber));
            Assert.That(result.ReleaseDate, Is.EqualTo(updatedDto.ReleaseDate));
            Assert.That(result.Author!.FirstName, Is.EqualTo(author.FirstName));
            Assert.That(result.Author.LastName, Is.EqualTo(author.LastName));
            Assert.That(result.Series, Is.Not.Null);
            Assert.That(result.Series!.Name, Is.EqualTo(updatedDto.Series.Name));
            Assert.That(result.Categories, Has.Count.EqualTo(1));
            Assert.That(result.Categories![0].Name, Is.EqualTo(category.Name));
            Assert.That(result.Tags, Has.Count.EqualTo(1));
            Assert.That(result.Tags![0].Name, Is.EqualTo(tag.Name));
            Assert.That(result.Cover, Is.Not.Null);
            Assert.That(result.Cover!.FilePath, Is.EqualTo(cover.FilePath));
            Assert.That(result.Location, Is.Not.Null);
            Assert.That(result.Location!.Type, Is.EqualTo(location.Type));
        });
    }

    /// <summary>
    /// Tests that UpdateBookAsync deletes the author if author is set to null.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the test finishes.</returns>
    [Test]
    public async Task UpdateBookAsync_RemovesAuthor_WhenAuthorNull()
    {
        // Setup
        var id = Guid.NewGuid();
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1".Unique(),
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Book".Unique(),
            Description = "Description",
            Author = new AuthorModel
            {
                FirstName = "Jane".Unique(),
                LastName = "Doe".Unique(),
            },
            Categories = [],
            Tags = [],
            SeriesId = series.Id,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        var bookDto = new BookDTO
        {
            Id = id,
            Title = book.Title,
            Description = "Description",
            Author = null!,
            Series = new SeriesDTO
            {
                Id = series.Id,
                Name = series.Name,
            },
            Categories = [],
            Tags = [],
        };

        // Execute
        var result = await this.testee.UpdateBookAsync(id, bookDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Books.Single(x => x.Id == id).Author, Is.Null);
        }
    }

    /// <summary>
    /// Tests that UpdateBookAsync deletes the cover if cover is set to null.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the test finishes.</returns>
    [Test]
    public async Task UpdateBookAsync_RemovesCover_WhenCoverNull()
    {
        // Setup
        var id = Guid.NewGuid();
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1".Unique(),
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Book".Unique(),
            Description = "Description",
            Cover = new FileInfoModel
            {
                FilePath = "cover.png",
                MimeType = "image/png",
                Sha256 = "hash",
                FileSizeBytes = 123,
            },
            Categories = [],
            Tags = [],
            SeriesId = series.Id,
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        var bookDto = new BookDTO
        {
            Id = id,
            Title = book.Title,
            Description = "Description",
            Cover = null!,
            Series = new SeriesDTO
            {
                Id = series.Id,
                Name = series.Name,
            },
            Categories = [],
            Tags = [],
        };

        // Execute
        var result = await this.testee.UpdateBookAsync(id, bookDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Books.Single(x => x.Id == id).Cover, Is.Null);
        }
    }

    /// <summary>
    /// Tests that UpdateBookAsync deletes the location if location is set to null.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the test finishes.</returns>
    [Test]
    public async Task UpdateBookAsync_RemovesLocation_WhenLocationNull()
    {
        // Setup
        var id = Guid.NewGuid();
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1".Unique(),
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Book".Unique(),
            Description = "Description",
            Location = new LocationModel
            {
                Url = "url",
            },
            Categories = [],
            Tags = [],
            SeriesId = series.Id,
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        var bookDto = new BookDTO
        {
            Id = id,
            Title = book.Title,
            Description = "Description",
            Location = null!,
            Series = new SeriesDTO
            {
                Id = series.Id,
                Name = series.Name,
            },
            Categories = [],
            Tags = [],
        };

        // Execute
        var result = await this.testee.UpdateBookAsync(id, bookDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Books.Single(x => x.Id == id).Location, Is.Null);
        }
    }

    /// <summary>
    /// Tests that UpdateBookAsync deletes categories if not present in DTO.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the test finishes.</returns>
    [Test]
    public async Task UpdateBookAsync_RemovesCategories_WhenCategoriesRemoved()
    {
        // Setup
        var id = Guid.NewGuid();
        var category = new CategoryModel
        {
            Name = "Category".Unique(),
        };
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1".Unique(),
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Book".Unique(),
            Description = "Description",
            Categories = [
                new BookCategoryModel
                {
                    Category = category,
                },
            ],
            Tags = [],
            SeriesId = series.Id,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Categories.Add(category);
            context.Series.Add(series);
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        var bookDto = new BookDTO
        {
            Id = id,
            Title = book.Title,
            Description = "Description",
            Series = new SeriesDTO
            {
                Id = series.Id,
                Name = series.Name,
            },
            Categories = [],
            Tags = [],
        };

        // Execute
        var result = await this.testee.UpdateBookAsync(id, bookDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Books.Single(x => x.Id == id).Categories, Is.Empty);
        }
    }

    /// <summary>
    /// Tests that UpdateBookAsync deletes tags if not present in DTO.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the test finishes.</returns>
    [Test]
    public async Task UpdateBookAsync_RemovesTags_WhenTagsRemoved()
    {
        // Setup
        var id = Guid.NewGuid();
        var tag = new TagModel
        {
            Name = "Tag".Unique(),
        };
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1".Unique(),
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Book".Unique(),
            Description = "Description",
            Tags = [
                new BookTagModel
                {
                    Tag = tag,
                },
            ],
            Categories = [],
            SeriesId = series.Id,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Tags.Add(tag);
            context.Series.Add(series);
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        var bookDto = new BookDTO
        {
            Id = id,
            Title = book.Title,
            Description = "Description",
            Series = new SeriesDTO
            {
                Id = series.Id,
                Name = series.Name,
            },
            Tags = [],
            Categories = [],
        };

        // Execute
        var result = await this.testee.UpdateBookAsync(id, bookDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Books.Single(x => x.Id == id).Tags, Is.Empty);
        }
    }

    /// <summary>
    /// Tests DeleteBookAsync removes a book and returns DTO.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DeleteBookAsync_RemovesBook_AndReturnsDto()
    {
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1".Unique(),
        };
        var book = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "DeleteMe".Unique(),
            Description = "Description",
            SeriesId = series.Id,
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            context.Books.Add(book);
            context.SaveChanges();
        }

        // Execute
        var result = await this.testee.DeleteBookAsync(book.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo(book.Title));
    }

    /// <summary>
    /// Tests DeleteBookAsync returns null if book not found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DeleteBookAsync_ReturnsNull_IfNotFound()
    {
        // Execute
        var result = await this.testee.DeleteBookAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that DeleteBookAsync returns null if the book does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the test finishes.</returns>
    [Test]
    public async Task DeleteBookAsync_ReturnsNull_IfBookDoesNotExist()
    {
        // Setup: use random ID
        var id = Guid.NewGuid();

        // Execute
        var result = await this.testee.DeleteBookAsync(id);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests single book import works and returns correct DTO.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ImportBookAsync_ImportsSingleBook()
    {
        // Setup
        var file = Substitute.For<IFormFile>();
        var bookTitle = "SingleBook".Unique();
        var parsingResult = CreateParsingResult(bookTitle);

        this.bookParserManager.IsBulkFile(file).Returns(false);
        this.bookParserManager.Parse(file).Returns(Task.FromResult(parsingResult));

        this.bookStorage.Save(Arg.Any<Guid>(), Arg.Any<IFormFile>()).Returns(Task.FromResult(new FileInfoDTO
        {
            FilePath = "/path/to/singlebook.pdf",
            FileSizeBytes = 123456,
            MimeType = "application/pdf",
            Sha256 = "sha256",
        }));

        // Execute
        var result = await this.testee.ImportBookAsync(file);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsBulkImport, Is.False);
            Assert.That(result.ImportedBooks, Has.Count.EqualTo(1));
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.ImportedBooks[0].Title, Is.EqualTo(bookTitle));
            Assert.That(result.Errors, Is.Empty);
        });
        await this.bookStorage.Received().Save(Arg.Any<Guid>(), file);
    }

    /// <summary>
    /// Tests bulk import imports multiple books.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ImportBookAsync_ImportsBulkBooks()
    {
        // Setup
        var file = Substitute.For<IFormFile>();
        var book1Title = "BulkBook1".Unique();
        var book2Title = "BulkBook2".Unique();
        var parsingResults = new List<BookParsingResult>
        {
            CreateParsingResult(book1Title),
            CreateParsingResult(book2Title),
        };

        this.bookParserManager.IsBulkFile(file).Returns(true);
        this.bookParserManager.ParseBulk(file).Returns(Task.FromResult(parsingResults));

        this.bookStorage.Save(Arg.Any<Guid>(), Arg.Any<IFormFile>()).Returns(Task.FromResult(new FileInfoDTO()));

        // Execute
        var result = await this.testee.ImportBookAsync(file);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsBulkImport, Is.True);
            Assert.That(result.ImportedBooks, Has.Count.EqualTo(2));
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.ImportedBooks.Any(b => b.Title == book1Title), Is.True);
            Assert.That(result.ImportedBooks.Any(b => b.Title == book2Title), Is.True);
            Assert.That(result.Errors, Is.Empty);
        });
    }

    /// <summary>
    /// Tests bulk import skips duplicate and adds error.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ImportBookAsync_BulkImport_AddsError_OnDuplicate()
    {
        // Setup
        var file = Substitute.For<IFormFile>();

        // Simulate first import is duplicate, second succeeds
        var book1Title = "DuplicateBook".Unique();
        var book2Title = "AnotherBook".Unique();
        var parsingResults = new List<BookParsingResult>
        {
            CreateParsingResult(book1Title),
            CreateParsingResult(book2Title),
        };

        this.bookParserManager.IsBulkFile(file).Returns(true);
        this.bookParserManager.ParseBulk(file).Returns(Task.FromResult(parsingResults));

        this.bookStorage.Save(Arg.Any<Guid>(), Arg.Any<IFormFile>()).Returns(Task.FromResult(new FileInfoDTO()));

        // Insert "DuplicateBook" before to cause duplicate
        await this.testee.CreateBookAsync(new CreateBookDTO
        {
            Title = book1Title,
            Description = "This is a duplicate book.",
            Categories = [],
            Tags = [],
            Series = new CreateSeriesDTO
            {
                Name = "TestSeries".Unique(),
            },
        });

        // Execute
        var result = await this.testee.ImportBookAsync(file);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsBulkImport, Is.True);
            Assert.That(result.ImportedBooks.Any(b => b.Title == book2Title), Is.True);
            Assert.That(result.ImportedBooks.Any(b => b.Title == book1Title), Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
        });
        Assert.That(result.Errors[0], Does.Contain(book1Title));
    }

    /// <summary>
    /// Tests single import returns error on duplicate.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ImportBookAsync_SingleImport_ReturnsError_OnDuplicate()
    {
        // Setup
        var file = Substitute.For<IFormFile>();
        var bookTitle = "SingleDuplicate".Unique();
        var parsingResult = CreateParsingResult(bookTitle);

        this.bookParserManager.IsBulkFile(file).Returns(false);
        this.bookParserManager.Parse(file).Returns(Task.FromResult(parsingResult));

        this.bookStorage.Save(Arg.Any<Guid>(), Arg.Any<IFormFile>()).Returns(Task.FromResult(new FileInfoDTO()));

        // Insert first to create duplicate
        await this.testee.CreateBookAsync(new CreateBookDTO
        {
            Title = bookTitle,
            Description = "This is a duplicate book.",
            Categories = [],
            Tags = [],
            Series = new CreateSeriesDTO
            {
                Name = "TestSeries".Unique(),
            },
        });

        // Execute
        var result = await this.testee.ImportBookAsync(file);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsBulkImport, Is.False);
            Assert.That(result.ImportedBooks, Is.Empty);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
        });
        Assert.That(result.Errors[0], Does.Contain(bookTitle));
    }

    /// <summary>
    /// Tests that bulk import handles an empty result gracefully.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ImportBookAsync_BulkImport_HandlesEmptyParsingResult()
    {
        // Setup
        var file = Substitute.For<IFormFile>();

        this.bookParserManager.IsBulkFile(file).Returns(true);
        this.bookParserManager.ParseBulk(file).Returns(Task.FromResult(new List<BookParsingResult>()));

        this.bookStorage.Save(Arg.Any<Guid>(), Arg.Any<IFormFile>()).Returns(Task.FromResult(new FileInfoDTO()));

        // Execute
        var result = await this.testee.ImportBookAsync(file);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsBulkImport, Is.True);
            Assert.That(result.ImportedBooks, Is.Empty);
            Assert.That(result.Errors, Is.Empty);
        });
    }

    /// <summary>
    /// Tests ImportBookFromAsinAsync returns null if scraper returns no metadata.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ImportBookFromAsinAsync_ReturnsNull_WhenMetadataIsNull()
    {
        // Setup
        var asin = "TestAsin";

        var scraper = Substitute.For<IAmazonScraper>();
        scraper.ScrapeFromAsin(asin).Returns((MetadataDTO?)null);
        this.metadataScraperManager.GetNewScraper(MetadataSources.Amazon).Returns(scraper);

        // Execute
        var result = await this.testee.ImportBookFromAsinAsync(asin);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests ImportBookFromAsinAsync imports a new book successfully.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ImportBookFromAsinAsync_ImportsBook_WhenMetadataValid()
    {
        // Setup
        var asin = "TestAsin";
        var metadata = new MetadataDTO
        {
            Title = "AsinBook".Unique(),
            Authors = ["Jane Doe"],
            Series = "TestSeries",
        };

        var scraper = Substitute.For<IAmazonScraper>();
        scraper.ScrapeFromAsin(asin).Returns(metadata);
        this.metadataScraperManager.GetNewScraper(MetadataSources.Amazon).Returns(scraper);

        // Execute
        var result = await this.testee.ImportBookFromAsinAsync(asin);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.IsBulkImport, Is.False);
            Assert.That(result.Errors, Is.Empty);
            Assert.That(result.ImportedBooks, Has.Count.EqualTo(1));
        });
        Assert.That(result.ImportedBooks[0].Title, Is.EqualTo(metadata.Title));
    }

    /// <summary>
    /// Tests ImportBookFromAsinAsync adds an error when book is a duplicate.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ImportBookFromAsinAsync_AddsError_WhenDuplicate()
    {
        // Setup
        var asin = "TestAsin";
        var metadata = new MetadataDTO
        {
            Title = "DuplicateBook".Unique(),
            Authors = ["John Doe"],
            Series = "DupSeries",
        };

        var scraper = Substitute.For<IAmazonScraper>();
        scraper.ScrapeFromAsin(asin).Returns(metadata);
        this.metadataScraperManager.GetNewScraper(MetadataSources.Amazon).Returns(scraper);

        await this.testee.CreateBookAsync(new CreateBookDTO
        {
            Title = metadata.Title,
            Description = "Duplicate",
            Categories = [],
            Tags = [],
            Series = new CreateSeriesDTO
            {
                Name = metadata.Series!,
            },
        });

        // Execute
        var result = await this.testee.ImportBookFromAsinAsync(asin);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.ImportedBooks, Is.Empty);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
        });
        Assert.That(result.Errors[0], Does.Contain(metadata.Title));
    }

    /// <summary>
    /// Tests DeleteFiles deletes the directory.
    /// </summary>
    [Test]
    public void DeleteFiles_DeletesDirectory()
    {
        // Setup
        var id = Guid.NewGuid();

        // Execute
        this.testee.DeleteFiles(id);

        // Assert
        this.bookStorage.Received(1).DeleteDirectory(id);
    }

    /// <summary>
    /// Tests BookFileExists returns true when a file with the same SHA256 exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task BookFileExists_ReturnsTrue_WhenFileWithSameChecksumExists()
    {
        // Setup
        var file = BookParserHelper.CoverBytes.ToFile("TestFile");

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var location = new LocationModel
            {
                FileInfo = this.mapper.Map<FileInfoModel>(file.ToFileInfo("TestFile")),
            };
            var book = new BookModel
            {
                Title = "TestBook".Unique(),
                Description = "Description",
                Location = location,
            };
            var series = new SeriesModel
            {
                Name = "TestSeries".Unique(),
                Books = [book],
            };
            context.Series.Add(series);
            context.SaveChanges();
        }

        // Execute
        var result = await this.testee.BookFileExists(file);

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests BookFileExists returns false when file checksum does not exist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task BookFileExists_ReturnsFalse_WhenFileWithChecksumDoesNotExist()
    {
        // Setup
        var file = BookParserHelper.RedDotBytes.ToFile("TestFile2");

        // Execute
        var result = await this.testee.BookFileExists(file);

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests BookFileExists throws ArgumentNullException when file is null.
    /// </summary>
    [Test]
    public void BookFileExists_ThrowsArgumentNullException_WhenFileIsNull()
    {
        // Execute and Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await this.testee.BookFileExists(null!);
        });
    }

    /// <summary>
    /// Tests CleanupDatabase removes orphaned entries and deletes files.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task CleanupDatabase_RemovesOrphans()
    {
        // Setup
        var authorId = Guid.NewGuid();
        var seriesId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var fileInfoId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Authors.Add(new AuthorModel
            {
                Id = authorId,
                FirstName = "Orphan".Unique(),
                LastName = "Author".Unique(),
            });
            context.Series.Add(new SeriesModel
            {
                Id = seriesId,
                Name = "OrphanSeries".Unique(),
            });
            context.Categories.Add(new CategoryModel
            {
                Id = categoryId,
                Name = "OrphanCategory".Unique(),
            });
            context.Tags.Add(new TagModel
            {
                Id = tagId,
                Name = "OrphanTag".Unique(),
            });
            context.Locations.Add(new LocationModel
            {
                Id = locationId,
                Type = LocationType.KapitelShelf,
            });
            context.FileInfos.Add(new FileInfoModel
            {
                Id = fileInfoId,
                FilePath = "orphan.file",
                MimeType = "mimetype",
                Sha256 = "hash",
            });
            context.SaveChanges();
        }

        // Execute
        await this.testee.CleanupDatabase();

        // Assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.Multiple(() =>
            {
                Assert.That(context.Authors.FirstOrDefault(x => x.Id == authorId), Is.Null);
                Assert.That(context.Series.FirstOrDefault(x => x.Id == seriesId), Is.Null);
                Assert.That(context.Categories.FirstOrDefault(x => x.Id == categoryId), Is.Null);
                Assert.That(context.Tags.FirstOrDefault(x => x.Id == tagId), Is.Null);
                Assert.That(context.Locations.FirstOrDefault(x => x.Id == locationId), Is.Null);
                Assert.That(context.FileInfos.FirstOrDefault(x => x.Id == fileInfoId), Is.Null);
            });
        }
    }

    /// <summary>
    /// Helper for building a parsing result for test.
    /// </summary>
    private static BookParsingResult CreateParsingResult(string title, string? description = null)
    {
        return new BookParsingResult
        {
            Book = new BookDTO
            {
                Title = title,
                Description = description ?? string.Empty,
                Series = new SeriesDTO
                {
                    Name = "TestSeries".Unique(),
                },
                Categories = [
                    new CategoryDTO
                    {
                        Name = "TestCategory".Unique(),
                    },
                ],
                Tags = [
                    new TagDTO
                    {
                        Name = "TestTag".Unique(),
                    },
                ],
            },
        };
    }
}
