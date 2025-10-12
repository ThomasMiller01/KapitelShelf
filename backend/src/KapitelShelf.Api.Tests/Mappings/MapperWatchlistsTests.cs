// <copyright file="MapperWatchlistsTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models;
using KapitelShelf.Data.Models.Watchlists;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the watchlists mapper.
/// </summary>
[TestFixture]
public class MapperWatchlistsTests
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
    /// Tests that WatchlistResultModelToBookDto maps all properties and nested values correctly.
    /// </summary>
    [Test]
    public void WatchlistResultModelToBookDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var model = new WatchlistResultModel
        {
            Id = Guid.NewGuid(),
            Title = "Book Title",
            Description = "Desc",
            ReleaseDate = "2024-06-01",
            Pages = 120,
            Volume = 2,
            Authors = ["John Doe"],
            CoverUrl = "/covers/book.png",
            LocationType = LocationType.Kindle,
            LocationUrl = "https://amazon.de/book",
            Categories = ["Fantasy", "Adventure"],
            Tags = ["Tag1", "Tag2"],
        };

        // execute
        var dto = this.testee.WatchlistResultModelToBookDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Title, Is.EqualTo(model.Title));
            Assert.That(dto.Description, Is.EqualTo(model.Description));
            Assert.That(dto.ReleaseDate?.Date, Is.EqualTo(DateTime.Parse("2024-06-01", CultureInfo.InvariantCulture).ToUniversalTime().Date));
            Assert.That(dto.PageNumber, Is.EqualTo(model.Pages));
            Assert.That(dto.SeriesNumber, Is.EqualTo(model.Volume));
            Assert.That(dto.Author, Is.Not.Null);
            Assert.That(dto.Author!.FirstName, Is.EqualTo("John"));
            Assert.That(dto.Author.LastName, Is.EqualTo("Doe"));
            Assert.That(dto.Cover!.FilePath, Is.EqualTo(model.CoverUrl));
            Assert.That(dto.Location!.Url, Is.EqualTo(model.LocationUrl));
            Assert.That(dto.Location.Type.ToString(), Is.EqualTo(model.LocationType.ToString()));
            Assert.That(dto.Categories.Select(x => x.Name), Is.EquivalentTo(model.Categories));
            Assert.That(dto.Tags.Select(x => x.Name), Is.EquivalentTo(model.Tags));
        });
    }

    /// <summary>
    /// Tests that WatchlistResultModelToCreateBookDto maps all properties and nested values correctly.
    /// </summary>
    [Test]
    public void WatchlistResultModelToCreateBookDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var model = new WatchlistResultModel
        {
            Title = "Book Title",
            Description = "Desc",
            ReleaseDate = "2024-06-01",
            Pages = 300,
            Volume = 5,
            Series = new SeriesModel { Name = "SeriesName", },
            Authors = ["Alice Smith"],
            LocationType = LocationType.Onleihe,
            LocationUrl = "https://onleihe.de/book",
            Categories = ["Drama"],
            Tags = ["TagA"],
        };

        // execute
        var dto = this.testee.WatchlistResultModelToCreateBookDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Title, Is.EqualTo(model.Title));
            Assert.That(dto.Description, Is.EqualTo(model.Description));
            Assert.That(dto.ReleaseDate?.Date, Is.EqualTo(DateTime.Parse("2024-06-01", CultureInfo.InvariantCulture).ToUniversalTime().Date));
            Assert.That(dto.PageNumber, Is.EqualTo(model.Pages));
            Assert.That(dto.SeriesNumber, Is.EqualTo(model.Volume));
            Assert.That(dto.Series!.Name, Is.EqualTo(model.Series.Name));
            Assert.That(dto.Author, Is.Not.Null);
            Assert.That(dto.Author!.FirstName, Is.EqualTo("Alice"));
            Assert.That(dto.Author.LastName, Is.EqualTo("Smith"));
            Assert.That(dto.Location!.Url, Is.EqualTo(model.LocationUrl));
            Assert.That(dto.Location.Type.ToString(), Is.EqualTo(model.LocationType.ToString()));
            Assert.That(dto.Categories.Select(x => x.Name), Is.EquivalentTo(model.Categories));
            Assert.That(dto.Tags.Select(x => x.Name), Is.EquivalentTo(model.Tags));
        });
    }

    /// <summary>
    /// Tests that MetadataDtoToWatchlistResultModel copies all relevant properties.
    /// </summary>
    [Test]
    public void MetadataDtoToWatchlistResultModel_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new MetadataDTO
        {
            Title = "Meta Title",
            Description = "Meta Description",
            Volume = 1,
            Authors = ["Bob Meta"],
            ReleaseDate = "2023-01-01",
            Pages = 250,
            CoverUrl = "/cover/meta.png",
            Categories = ["History"],
            Tags = ["TagX", "TagY"],
        };

        // execute
        var model = this.testee.MetadataDtoToWatchlistResultModel(dto);

        // assert
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model.Title, Is.EqualTo(dto.Title));
            Assert.That(model.Description, Is.EqualTo(dto.Description));
            Assert.That(model.Volume, Is.EqualTo(dto.Volume));
            Assert.That(model.Authors, Is.EquivalentTo(dto.Authors));
            Assert.That(model.ReleaseDate, Is.EqualTo(dto.ReleaseDate));
            Assert.That(model.Pages, Is.EqualTo(dto.Pages));
            Assert.That(model.CoverUrl, Is.EqualTo(dto.CoverUrl));
            Assert.That(model.Categories, Is.EquivalentTo(dto.Categories));
            Assert.That(model.Tags, Is.EquivalentTo(dto.Tags));
        });
    }

    /// <summary>
    /// Tests that MetadataDtoToWatchlistResultModelNullable returns null when input is null.
    /// </summary>
    [Test]
    public void MetadataDtoToWatchlistResultModelNullable_ReturnsNull_WhenInputIsNull()
    {
        // execute
        var result = this.testee.MetadataDtoToWatchlistResultModelNullable(null);

        // assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that MetadataDtoToWatchlistResultModelNullable maps correctly when input is valid.
    /// </summary>
    [Test]
    public void MetadataDtoToWatchlistResultModelNullable_ReturnsMappedModel_WhenInputIsValid()
    {
        // setup
        var dto = new MetadataDTO
        {
            Title = "Test",
        };

        // execute
        var result = this.testee.MetadataDtoToWatchlistResultModelNullable(dto);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo(dto.Title));
    }

    /// <summary>
    /// Tests that WatchlistModelToSeriesWatchlistDto maps the series and leaves items empty.
    /// </summary>
    [Test]
    public void WatchlistModelToSeriesWatchlistDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var model = new WatchlistModel
        {
            Id = Guid.NewGuid(),
            Series = new SeriesModel
            {
                Id = Guid.NewGuid(),
                Name = "SeriesName",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Books = [],
            },
        };

        // execute
        var dto = this.testee.WatchlistModelToSeriesWatchlistDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Series.Name, Is.EqualTo(model.Series.Name));
            Assert.That(dto.Items, Is.Empty);
        });
    }

    /// <summary>
    /// Tests that WatchlistResultModelToAuthorDto returns null when there are no authors.
    /// </summary>
    [Test]
    public void WatchlistResultModelToAuthorDto_ReturnsNull_WhenNoAuthors()
    {
        // setup
        var model = new WatchlistResultModel
        {
            Authors = [],
        };

        // execute
        var result = this.testee.WatchlistResultModelToAuthorDto(model);

        // assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that WatchlistResultModelToAuthorDto maps the first author correctly.
    /// </summary>
    [Test]
    public void WatchlistResultModelToAuthorDto_MapsFirstAuthorCorrectly()
    {
        // setup
        var model = new WatchlistResultModel
        {
            Authors = ["Mary Shelley"],
        };

        // execute
        var dto = this.testee.WatchlistResultModelToAuthorDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto!.FirstName, Is.EqualTo("Mary"));
            Assert.That(dto.LastName, Is.EqualTo("Shelley"));
        });
    }

    /// <summary>
    /// Tests that WatchlistResultModelToCreateAuthorDto returns null when there are no authors.
    /// </summary>
    [Test]
    public void WatchlistResultModelToCreateAuthorDto_ReturnsNull_WhenNoAuthors()
    {
        // setup
        var model = new WatchlistResultModel
        {
            Authors = [],
        };

        // execute
        var result = this.testee.WatchlistResultModelToCreateAuthorDto(model);

        // assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that WatchlistResultModelToCreateAuthorDto maps the first author correctly.
    /// </summary>
    [Test]
    public void WatchlistResultModelToCreateAuthorDto_MapsFirstAuthorCorrectly()
    {
        // setup
        var model = new WatchlistResultModel
        {
            Authors = ["Arthur Conan Doyle"],
        };

        // execute
        var dto = this.testee.WatchlistResultModelToCreateAuthorDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto!.FirstName, Is.EqualTo("Arthur"));
            Assert.That(dto.LastName, Is.EqualTo("Conan Doyle"));
        });
    }
}
