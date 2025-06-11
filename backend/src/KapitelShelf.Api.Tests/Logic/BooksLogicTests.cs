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
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the BooksLogic class.
/// </summary>
[TestFixture]
public class BooksLogicTests
{
    private SqliteConnection connection;

    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private IMapper mapper;
    private IBookParserManager bookParserManager;
    private IBookStorage bookStorage;
    private BooksLogic testee;

    /// <summary>
    /// Sets up a new in-memory database and fakes before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // use real database, because in-memory db doesnt support transactions
        this.connection = new SqliteConnection("DataSource=:memory:");
        this.connection.Open();

        this.dbOptions = new DbContextOptionsBuilder<KapitelShelfDBContext>()
            .UseSqlite(connection)
            .Options;

        // datamigrations
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Database.EnsureCreated();
        }

        this.dbContextFactory = Substitute.For<IDbContextFactory<KapitelShelfDBContext>>();
        this.dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(new KapitelShelfDBContext(this.dbOptions)));

        this.mapper = Testhelper.CreateMapper();

        this.bookParserManager = Substitute.For<IBookParserManager>();
        this.bookStorage = Substitute.For<IBookStorage>();
        this.testee = new BooksLogic(this.dbContextFactory, this.mapper, this.bookParserManager, this.bookStorage);
    }

    /// <summary>
    /// Cleans up the in-memory database after each test.
    /// </summary>
    [TearDown]
    public void TearDown() => this.connection.Dispose();

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
            Name = "Series1",
        };
        var book = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book1",
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
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Title, Is.EqualTo("Book1"));
            Assert.That(result[0].Description, Is.EqualTo("Description"));
            Assert.That(result[0].PageNumber, Is.EqualTo(7));
        });
    }

    /// <summary>
    /// GetBooksAsync returns empty when there are no books.
    /// </summary>
    /// /// <returns>A task.</returns>
    [Test]
    public async Task GetBooksAsync_ReturnsEmpty_WhenNoBooks()
    {
        // Execute
        var result = await this.testee.GetBooksAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
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
            Name = "Series1",
        };
        var book = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "BookById",
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
        var result = await this.testee.GetBookByIdAsync(book.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Title, Is.EqualTo("BookById"));
            Assert.That(result.Description, Is.EqualTo("Description"));
            Assert.That(result.PageNumber, Is.EqualTo(2));
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
        var result = await this.testee.GetBookByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
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
            Name = "Series1",
        };
        var book = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Duplicate",
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
            Title = "Duplicate",
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
            FirstName = "A",
            LastName = "B",
        };
        var category = new CreateCategoryDTO
        {
            Name = "Category",
        };
        var tag = new CreateTagDTO
        {
            Name = "Tag",
        };
        var series = new CreateSeriesDTO
        {
            Name = "Series",
        };
        var createDto = new CreateBookDTO
        {
            Title = "New Book",
            Description = "Description",
            PageNumber = 42,
            ReleaseDate = new DateTime(2023, 1, 1),
            Author = author,
            Series = series,
            Categories = [category],
            Tags = [tag],
        };
        var bookModel = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "New Book",
            Description = "Description",
            PageNumber = 42,
            ReleaseDate = new DateTime(2023, 1, 1),
        };
        var seriesModel = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series",
        };
        var authorModel = new AuthorModel
        {
            Id = Guid.NewGuid(),
            FirstName = "A",
            LastName = "B",
        };
        var categoryModel = new CategoryModel
        {
            Id = Guid.NewGuid(),
            Name = "Category",
        };
        var tagModel = new TagModel
        {
            Id = Guid.NewGuid(),
            Name = "Tag",
        };

        // Execute
        var result = await this.testee.CreateBookAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Title, Is.EqualTo("New Book"));
            Assert.That(result.Description, Is.EqualTo("Description"));
            Assert.That(result.PageNumber, Is.EqualTo(42));
            Assert.That(result.ReleaseDate, Is.EqualTo(new DateTime(2023, 1, 1)));
            Assert.That(result.Author!.FirstName, Is.EqualTo("A"));
            Assert.That(result.Author.LastName, Is.EqualTo("B"));
            Assert.That(result.Series, Is.Not.Null);
            Assert.That(result.Series!.Name, Is.EqualTo("Series"));
            Assert.That(result.Categories, Has.Count.EqualTo(1));
            Assert.That(result.Categories![0].Name, Is.EqualTo("Category"));
            Assert.That(result.Tags, Has.Count.EqualTo(1));
            Assert.That(result.Tags![0].Name, Is.EqualTo("Tag"));
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
            Title = "BookTitle",
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
        Assert.That(result.Series!.Name, Is.EqualTo("BookTitle"));
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
            Name = "BookSeries",
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);
            await context.SaveChangesAsync();
        }

        var createDto = new CreateBookDTO
        {
            Title = "Book",
            Description = "Description",
            Series = new CreateSeriesDTO
            {
                Name = "BookSeries",
            },
            Categories = [],
            Tags = [],
        };

        // Execute
        var result = await this.testee.CreateBookAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Series.Count(), Is.EqualTo(1));
        }
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
            FirstName = "Jane",
            LastName = "Doe",
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Authors.Add(author);
            await context.SaveChangesAsync();
        }

        var createDto = new CreateBookDTO
        {
            Title = "Book",
            Description = "Description",
            Author = new CreateAuthorDTO
            {
                FirstName = "Jane",
                LastName = "Doe",
            },
            Series = new CreateSeriesDTO
            {
                Name = "Series",
            },
            Categories = [],
            Tags = [],
        };

        // Execute
        var result = await this.testee.CreateBookAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Authors.Count(), Is.EqualTo(1));
        }
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
            Name = "Category1",
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        }

        var createDto = new CreateBookDTO
        {
            Title = "Book",
            Description = "Description",
            Series = new CreateSeriesDTO
            {
                Name = "Series",
            },
            Categories = [
                new CreateCategoryDTO
                {
                    Name = "Category1",
                },
            ],
            Tags = [],
        };

        // Execute
        var result = await this.testee.CreateBookAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Categories.Count(), Is.EqualTo(1));
        }
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
            Name = "Tag1",
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Tags.Add(tag);
            await context.SaveChangesAsync();
        }

        var createDto = new CreateBookDTO
        {
            Title = "Book",
            Description = "Description",
            Series = new CreateSeriesDTO
            {
                Name = "Series",
            },
            Categories = [],
            Tags = [
                new CreateTagDTO
                {
                    Name = "Tag1",
                },
            ],
        };

        // Execute
        var result = await this.testee.CreateBookAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Tags.Count(), Is.EqualTo(1));
        }
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
            Name = "Series1",
        };
        var book1 = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book1",
            Description = "Description1",
            SeriesId = series.Id,
        };
        var book2 = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book2",
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
            Title = "Book2",
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
            Name = "Series1",
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Old",
            Description = "OldDesc",
            PageNumber = 100,
            ReleaseDate = new DateTime(2000, 1, 1),
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
            Title = "Title",

            // intentionally set Series to null
            Series = null,

            Author = new AuthorDTO
            {
                FirstName = "First",
                LastName = "Last",
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
            Title = "NotFound",
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
            Name = "Series1",
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Old",
            Description = "OldDesc",
            PageNumber = 100,
            ReleaseDate = new DateTime(2000, 1, 1),
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
            FirstName = "X",
            LastName = "Y",
        };
        var category = new CategoryDTO
        {
            Name = "C1",
        };
        var tag = new TagDTO
        {
            Name = "T1",
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
            Title = "NewTitle",
            Description = "DescNew",
            PageNumber = 42,
            ReleaseDate = new DateTime(2025, 1, 1),
            Author = author,
            Series = new SeriesDTO
            {
                Name = "SeriesNew",
            },
            Categories = [category],
            Tags = [tag],
            Cover = cover,
            Location = location,
        };

        var authorModel = new AuthorModel
        {
            Id = Guid.NewGuid(),
            FirstName = "X",
            LastName = "Y",
        };
        var seriesModel = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "SeriesNew",
        };
        var categoryModel = new CategoryModel
        {
            Id = Guid.NewGuid(),
            Name = "C1",
        };
        var tagModel = new TagModel
        {
            Id = Guid.NewGuid(),
            Name = "T1",
        };
        var fileInfoModel = new FileInfoModel
        {
            Id = Guid.NewGuid(),
            FilePath = "/cover.png",
            FileSizeBytes = 123,
            MimeType = "image/png",
            Sha256 = "hash",
        };

        // Execute
        var result = await this.testee.UpdateBookAsync(id, updatedDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Title, Is.EqualTo("NewTitle"));
            Assert.That(result.Description, Is.EqualTo("DescNew"));
            Assert.That(result.PageNumber, Is.EqualTo(42));
            Assert.That(result.ReleaseDate, Is.EqualTo(new DateTime(2025, 1, 1)));
            Assert.That(result.Author!.FirstName, Is.EqualTo("X"));
            Assert.That(result.Author.LastName, Is.EqualTo("Y"));
            Assert.That(result.Series, Is.Not.Null);
            Assert.That(result.Series!.Name, Is.EqualTo("SeriesNew"));
            Assert.That(result.Categories, Has.Count.EqualTo(1));
            Assert.That(result.Categories![0].Name, Is.EqualTo("C1"));
            Assert.That(result.Tags, Has.Count.EqualTo(1));
            Assert.That(result.Tags![0].Name, Is.EqualTo("T1"));
            Assert.That(result.Cover, Is.Not.Null);
            Assert.That(result.Cover!.FilePath, Is.EqualTo("/cover.png"));
            Assert.That(result.Location, Is.Not.Null);
            Assert.That(result.Location!.Type, Is.EqualTo(LocationTypeDTO.KapitelShelf));
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
            Name = "Series1",
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Book",
            Description = "Description",
            Author = new AuthorModel
            {
                FirstName = "Jane",
                LastName = "Doe",
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
            Title = "Book",
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
            Name = "Series1",
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Book",
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
            Title = "Book",
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
            Name = "Series1",
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Book",
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
            Title = "Book",
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
            Name = "Category",
        };
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1",
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Book",
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
            Title = "Book",
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
            Name = "Tag",
        };
        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series1",
        };
        var book = new BookModel
        {
            Id = id,
            Title = "Book",
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
            Title = "Book",
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
            Name = "Series1",
        };
        var book = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "DeleteMe",
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
        Assert.That(result!.Title, Is.EqualTo("DeleteMe"));
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
        var parsingResult = CreateParsingResult("SingleBook");

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
            Assert.That(result.ImportedBooks[0].Title, Is.EqualTo("SingleBook"));
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
        var parsingResults = new List<BookParsingResult>
        {
            CreateParsingResult("BulkBook1"),
            CreateParsingResult("BulkBook2"),
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
            Assert.That(result.ImportedBooks.Any(b => b.Title == "BulkBook1"), Is.True);
            Assert.That(result.ImportedBooks.Any(b => b.Title == "BulkBook2"), Is.True);
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
        var parsingResults = new List<BookParsingResult>
        {
            CreateParsingResult("DuplicateBook"),
            CreateParsingResult("AnotherBook"),
        };

        this.bookParserManager.IsBulkFile(file).Returns(true);
        this.bookParserManager.ParseBulk(file).Returns(Task.FromResult(parsingResults));

        this.bookStorage.Save(Arg.Any<Guid>(), Arg.Any<IFormFile>()).Returns(Task.FromResult(new FileInfoDTO()));

        // Insert "DuplicateBook" before to cause duplicate
        await this.testee.CreateBookAsync(new CreateBookDTO
        {
            Title = "DuplicateBook",
            Description = "This is a duplicate book.",
            Categories = [],
            Tags = [],
            Series = new CreateSeriesDTO
            {
                Name = "TestSeries",
            },
        });

        // Execute
        var result = await this.testee.ImportBookAsync(file);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsBulkImport, Is.True);
            Assert.That(result.ImportedBooks.Any(b => b.Title == "AnotherBook"), Is.True);
            Assert.That(result.ImportedBooks.Any(b => b.Title == "DuplicateBook"), Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
        });
        Assert.That(result.Errors[0], Does.Contain("DuplicateBook"));
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
        var parsingResult = CreateParsingResult("SingleDuplicate");

        this.bookParserManager.IsBulkFile(file).Returns(false);
        this.bookParserManager.Parse(file).Returns(Task.FromResult(parsingResult));

        this.bookStorage.Save(Arg.Any<Guid>(), Arg.Any<IFormFile>()).Returns(Task.FromResult(new FileInfoDTO()));

        // Insert first to create duplicate
        await this.testee.CreateBookAsync(new CreateBookDTO
        {
            Title = "SingleDuplicate",
            Description = "This is a duplicate book.",
            Categories = [],
            Tags = [],
            Series = new CreateSeriesDTO
            {
                Name = "TestSeries",
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
        Assert.That(result.Errors[0], Does.Contain("SingleDuplicate"));
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
    /// Tests CleanupDatabase removes orphaned entries and deletes files.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task CleanupDatabase_RemovesOrphans()
    {
        // Setup
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Authors.Add(new AuthorModel
            {
                Id = Guid.NewGuid(),
                FirstName = "Orphan",
                LastName = "Author",
            });
            context.Series.Add(new SeriesModel
            {
                Id = Guid.NewGuid(),
                Name = "OrphanSeries",
            });
            context.Categories.Add(new CategoryModel
            {
                Id = Guid.NewGuid(),
                Name = "OrphanCategory",
            });
            context.Tags.Add(new TagModel
            {
                Id = Guid.NewGuid(),
                Name = "OrphanTag",
            });
            context.Locations.Add(new LocationModel
            {
                Id = Guid.NewGuid(),
                Type = LocationType.KapitelShelf,
            });
            context.FileInfos.Add(new FileInfoModel
            {
                Id = Guid.NewGuid(),
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
                Assert.That(context.Authors.Count(), Is.EqualTo(0));
                Assert.That(context.Series.Count(), Is.EqualTo(0));
                Assert.That(context.Categories.Count(), Is.EqualTo(0));
                Assert.That(context.Tags.Count(), Is.EqualTo(0));
                Assert.That(context.Locations.Count(), Is.EqualTo(0));
                Assert.That(context.FileInfos.Count(), Is.EqualTo(0));
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
                    Name = "TestSeries",
                },
                Categories = [
                    new CategoryDTO
                    {
                        Name = "TestCategory",
                    },
                ],
                Tags = [
                    new TagDTO
                    {
                        Name = "TestTag",
                    },
                ],
            },
        };
    }
}
