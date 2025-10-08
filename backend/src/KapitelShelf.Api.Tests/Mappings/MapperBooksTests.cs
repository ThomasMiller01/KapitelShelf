// <copyright file="MapperBooksTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the book mapper.
/// </summary>
[TestFixture]
public class MapperBooksTests
{
    private Mapper testee;

    /// <summary>
    /// Sets up the mapper instance before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // setup
#pragma warning disable IDE0022 // Use expression body for method
        this.testee = new Mapper();
#pragma warning restore IDE0022 // Use expression body for method
    }

    /// <summary>
    /// Tests that BookModelToBookDto correctly maps all properties, including manual category and tag logic.
    /// </summary>
    [Test]
    public void BookModelToBookDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var seriesId = Guid.NewGuid();
        var model = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "The Test Book",
            Description = "A description for testing.",
            ReleaseDate = DateTime.UtcNow,
            PageNumber = 321,
            SeriesId = seriesId,
            Series = new SeriesModel
            {
                Id = seriesId,
                Name = "Test Series",
            },
            Author = new AuthorModel
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Doe",
            },
            Categories =
            [
                new BookCategoryModel
                {
                    Category = new CategoryModel
                    {
                        Id = Guid.NewGuid(),
                        Name = "Adventure",
                    },
                },
            ],
            Tags =
            [
                new BookTagModel
                {
                    Tag = new TagModel
                    {
                        Id = Guid.NewGuid(),
                        Name = "Bestseller",
                    },
                },
            ],
        };

        // execute
        var dto = this.testee.BookModelToBookDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Title, Is.EqualTo(model.Title));
            Assert.That(dto.Description, Is.EqualTo(model.Description));
            Assert.That(dto.ReleaseDate, Is.EqualTo(model.ReleaseDate));
            Assert.That(dto.PageNumber, Is.EqualTo(model.PageNumber));
            Assert.That(dto.Categories, Has.Count.EqualTo(1));
            Assert.That(dto.Categories.First().Name, Is.EqualTo("Adventure"));
            Assert.That(dto.Tags, Has.Count.EqualTo(1));
            Assert.That(dto.Tags.First().Name, Is.EqualTo("Bestseller"));
            Assert.That(dto.Series, Is.Not.Null);
            Assert.That(dto.Series!.Id, Is.EqualTo(model.SeriesId));
            Assert.That(dto.Series.LastVolume, Is.Null);
        });
    }

    /// <summary>
    /// Tests that BookModelToBookDtoNullable returns null if model is null.
    /// </summary>
    [Test]
    public void BookModelToBookDtoNullable_ReturnsNull_WhenModelIsNull()
    {
        // execute
        var dto = this.testee.BookModelToBookDtoNullable(null);

        // assert
        Assert.That(dto, Is.Null);
    }

    /// <summary>
    /// Tests that CreateBookDtoToBookModel maps all properties and fills nested objects correctly.
    /// </summary>
    [Test]
    public void CreateBookDtoToBookModel_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new CreateBookDTO
        {
            Title = "Mapped Book",
            Description = "Testing nested mappings.",
            ReleaseDate = DateTime.UtcNow,
            PageNumber = 500,
            Series = new CreateSeriesDTO { Name = "Mapped Series", },
            SeriesNumber = 2,
            Author = new CreateAuthorDTO { FirstName = "John", LastName = "Smith", },
            Location = new CreateLocationDTO { Type = LocationTypeDTO.Skoobe, Url = "https://example.com", },
            Categories =
            [
                new CreateCategoryDTO { Name = "Fantasy", },
                new CreateCategoryDTO { Name = "Drama", },
            ],
            Tags =
            [
                new CreateTagDTO { Name = "Epic", },
                new CreateTagDTO { Name = "Classic", },
            ],
        };

        // execute
        var model = this.testee.CreateBookDtoToBookModel(dto);

        // assert
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model.Id, Is.EqualTo(Guid.Empty));
            Assert.That(model.Title, Is.EqualTo(dto.Title));
            Assert.That(model.Description, Is.EqualTo(dto.Description));
            Assert.That(model.ReleaseDate, Is.EqualTo(dto.ReleaseDate));
            Assert.That(model.PageNumber, Is.EqualTo(dto.PageNumber));
            Assert.That(model.Series, Is.Not.Null);
            Assert.That(model.Series!.Name, Is.EqualTo("Mapped Series"));
            Assert.That(model.SeriesNumber, Is.EqualTo(2));
            Assert.That(model.Author, Is.Not.Null);
            Assert.That(model.Author!.FirstName, Is.EqualTo("John"));
            Assert.That(model.Author.LastName, Is.EqualTo("Smith"));
            Assert.That(model.Location, Is.Not.Null);
            Assert.That(model.Location!.Url, Is.EqualTo("https://example.com"));
            Assert.That(model.Categories, Has.Count.EqualTo(2));
            Assert.That(model.Tags, Has.Count.EqualTo(2));
        });
    }

    /// <summary>
    /// Tests that BookDtoToCreateBookDto correctly maps all simple fields.
    /// </summary>
    [Test]
    public void BookDtoToCreateBookDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new BookDTO
        {
            Id = Guid.NewGuid(),
            Title = "Sample Title",
            Description = "Desc",
            ReleaseDate = DateTime.UtcNow,
            PageNumber = 111,
            Series = new SeriesDTO { Name = "Series Name", },
            SeriesNumber = 3,
            Author = new AuthorDTO { FirstName = "A", LastName = "B", },
            Categories = [new CategoryDTO { Name = "Category1", }],
            Tags = [new TagDTO { Name = "Tag1", }],
        };

        // execute
        var createDto = this.testee.BookDtoToCreateBookDto(dto);

        // assert
        Assert.That(createDto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createDto.Title, Is.EqualTo(dto.Title));
            Assert.That(createDto.Description, Is.EqualTo(dto.Description));
            Assert.That(createDto.PageNumber, Is.EqualTo(dto.PageNumber));
        });
    }

    /// <summary>
    /// Tests that BookSearchViewToBookDto maps manual fields (series, authors, categories, tags) correctly.
    /// </summary>
    [Test]
    public void BookSearchViewToBookDto_MapsAllManualFieldsCorrectly()
    {
        // setup
        var view = new BookSearchView
        {
            Id = Guid.NewGuid(),
            Title = "Search Book",
            Description = "Search Description",
            SeriesName = "Series Test",
            AuthorNames = "John Doe, Jane Smith",
            CategoryNames = "Sci-Fi, Romance",
            TagNames = "Tag1, Tag2",
        };

        // execute
        var dto = this.testee.BookSearchViewToBookDto(view);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Title, Is.EqualTo(view.Title));
            Assert.That(dto.Description, Is.EqualTo(view.Description));
            Assert.That(dto.Series, Is.Not.Null);
            Assert.That(dto.Series!.Name, Is.EqualTo("Series Test"));
            Assert.That(dto.Author, Is.Not.Null);
            Assert.That(dto.Author!.FirstName, Is.EqualTo("John"));
            Assert.That(dto.Author.LastName, Is.EqualTo("Doe"));
            Assert.That(dto.Categories, Has.Count.EqualTo(2));
            Assert.That(dto.Tags, Has.Count.EqualTo(2));
        });
    }

    /// <summary>
    /// Tests that MetadataDtoToCreateBookDto correctly fills fields and parses date.
    /// </summary>
    [Test]
    public void MetadataDtoToCreateBookDto_MapsAllFieldsCorrectly()
    {
        // setup
        var dto = new MetadataDTO
        {
            Title = "Metadata Book",
            Description = "From metadata source",
            Series = "Meta Series",
            Volume = 7,
            Authors = ["Alice Example"],
            ReleaseDate = "2025-10-08",
            Pages = 456,
            Categories = ["Horror", "Thriller"],
            Tags = ["Dark", "Psychological"],
        };

        // execute
        var createDto = this.testee.MetadataDtoToCreateBookDto(dto);

        // assert
        Assert.That(createDto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createDto.Title, Is.EqualTo(dto.Title));
            Assert.That(createDto.Description, Is.EqualTo(dto.Description));
            Assert.That(createDto.Series!.Name, Is.EqualTo("Meta Series"));
            Assert.That(createDto.SeriesNumber, Is.EqualTo(dto.Volume));
            Assert.That(createDto.Author!.FirstName, Is.EqualTo("Alice"));
            Assert.That(createDto.Author.LastName, Is.EqualTo("Example"));
            Assert.That(createDto.Categories, Has.Count.EqualTo(2));
            Assert.That(createDto.Tags, Has.Count.EqualTo(2));
            Assert.That(createDto.PageNumber, Is.EqualTo(456));
            Assert.That(createDto.ReleaseDate?.Date, Is.EqualTo(DateTime.Parse("2025-10-08", CultureInfo.InvariantCulture).ToUniversalTime().Date));
        });
    }
}
