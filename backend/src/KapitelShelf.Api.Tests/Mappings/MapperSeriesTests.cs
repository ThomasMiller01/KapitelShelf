// <copyright file="MapperSeriesTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the series mapper.
/// </summary>
[TestFixture]
public class MapperSeriesTests
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
    /// Tests that SeriesModelToSeriesDto maps all fields correctly, including LastVolume and TotalBooks.
    /// </summary>
    [Test]
    public void SeriesModelToSeriesDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var seriesId = Guid.NewGuid();
        var model = new SeriesModel
        {
            Id = seriesId,
            Name = "Test Series",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow,
            Books =
            [
                new BookModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Book One",
                    SeriesNumber = 1,
                    Description = "Desc 1",
                },
                new BookModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Book Two",
                    SeriesNumber = 2,
                    Description = "Desc 2",
                },
            ],
        };

        // execute
        var dto = this.testee.SeriesModelToSeriesDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Name, Is.EqualTo(model.Name));
            Assert.That(dto.CreatedAt, Is.EqualTo(model.CreatedAt));
            Assert.That(dto.UpdatedAt, Is.EqualTo(model.UpdatedAt));
            Assert.That(dto.TotalBooks, Is.EqualTo(model.Books.Count));
            Assert.That(dto.LastVolume, Is.Not.Null);
            Assert.That(dto.LastVolume!.Title, Is.EqualTo("Book Two"));
        });
    }

    /// <summary>
    /// Tests that SeriesModelToSeriesDto sets LastVolume to null when no books exist.
    /// </summary>
    [Test]
    public void SeriesModelToSeriesDto_SetsLastVolumeToNull_WhenNoBooks()
    {
        // setup
        var model = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Empty Series",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Books = [],
        };

        // execute
        var dto = this.testee.SeriesModelToSeriesDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.LastVolume, Is.Null);
            Assert.That(dto.TotalBooks, Is.EqualTo(0));
        });
    }

    /// <summary>
    /// Tests that SeriesDtoToSeriesModel maps all fields correctly and initializes Books.
    /// </summary>
    [Test]
    public void SeriesDtoToSeriesModel_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new SeriesDTO
        {
            Id = Guid.NewGuid(),
            Name = "Mapped Series",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            TotalBooks = 3,
        };

        // execute
        var model = this.testee.SeriesDtoToSeriesModel(dto);

        // assert
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model.Id, Is.EqualTo(dto.Id));
            Assert.That(model.Name, Is.EqualTo(dto.Name));
            Assert.That(model.CreatedAt, Is.EqualTo(dto.CreatedAt));
            Assert.That(model.UpdatedAt, Is.EqualTo(dto.UpdatedAt));
            Assert.That(model.Books, Is.Empty);
        });
    }

    /// <summary>
    /// Tests that CreateSeriesDtoToSeriesModel maps all properties and ignores Id, CreatedAt, UpdatedAt, and Books.
    /// </summary>
    [Test]
    public void CreateSeriesDtoToSeriesModel_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new CreateSeriesDTO
        {
            Name = "Created Series",
        };

        // execute
        var model = this.testee.CreateSeriesDtoToSeriesModel(dto);

        // assert
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model.Id, Is.EqualTo(Guid.Empty)); // ignored
            Assert.That(model.Name, Is.EqualTo(dto.Name));
            Assert.That(model.CreatedAt, Is.Not.EqualTo(default(DateTime)));
            Assert.That(model.UpdatedAt, Is.Not.EqualTo(default(DateTime)));
            Assert.That(model.Books, Is.Empty);
        });
    }

    /// <summary>
    /// Tests that SeriesDtoToCreateSeriesDto maps all fields correctly and ignores non-transferable properties.
    /// </summary>
    [Test]
    public void SeriesDtoToCreateSeriesDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new SeriesDTO
        {
            Id = Guid.NewGuid(),
            Name = "Mapped Back",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow,
            TotalBooks = 10,
            LastVolume = new BookDTO
            {
                Id = Guid.NewGuid(),
                Title = "Ignored Book",
                Description = "Should be ignored",
            },
        };

        // execute
        var createDto = this.testee.SeriesDtoToCreateSeriesDto(dto);

        // assert
        Assert.That(createDto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createDto.Name, Is.EqualTo(dto.Name));
        });
    }
}
