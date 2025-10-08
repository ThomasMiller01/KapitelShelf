// <copyright file="MapperMetadataTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Mappings;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the metadata mapper.
/// </summary>
[TestFixture]
public class MapperMetadataTests
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
    /// Tests that MetadataDtoToCreateAuthorDto returns null when Authors is empty.
    /// </summary>
    [Test]
    public void MetadataDtoToCreateAuthorDto_ReturnsNull_WhenNoAuthors()
    {
        // setup
        var metadata = new MetadataDTO
        {
            Title = "Book without Author",
            Authors = [],
        };

        // execute
        var result = this.testee.MetadataDtoToCreateAuthorDto(metadata);

        // assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that MetadataDtoToCreateAuthorDto maps the first author correctly.
    /// </summary>
    [Test]
    public void MetadataDtoToCreateAuthorDto_MapsFirstAuthorCorrectly()
    {
        // setup
        var metadata = new MetadataDTO
        {
            Title = "Test Title",
            Authors = ["John Doe", "Jane Smith"],
        };

        // execute
        var result = this.testee.MetadataDtoToCreateAuthorDto(metadata);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.FirstName, Is.EqualTo("John"));
            Assert.That(result.LastName, Is.EqualTo("Doe"));
        });
    }

    /// <summary>
    /// Tests that MetadataSourceToLocationTypeDto maps MetadataSources.Amazon to LocationTypeDTO.Kindle.
    /// </summary>
    [Test]
    public void MetadataSourceToLocationTypeDto_MapsAmazonToKindle()
    {
        // execute
        var result = this.testee.MetadataSourceToLocationTypeDto(MetadataSources.Amazon);

        // assert
        Assert.That(result, Is.EqualTo(LocationTypeDTO.Kindle));
    }

    /// <summary>
    /// Tests that MetadataSourceToLocationTypeDto maps MetadataSources.GoogleBooks to LocationTypeDTO.KapitelShelf.
    /// </summary>
    [Test]
    public void MetadataSourceToLocationTypeDto_MapsGoogleBooksToKapitelShelf()
    {
        // execute
        var result = this.testee.MetadataSourceToLocationTypeDto(MetadataSources.GoogleBooks);

        // assert
        Assert.That(result, Is.EqualTo(LocationTypeDTO.KapitelShelf));
    }

    /// <summary>
    /// Tests that MetadataSourceToLocationTypeDto maps MetadataSources.OpenLibrary to LocationTypeDTO.KapitelShelf.
    /// </summary>
    [Test]
    public void MetadataSourceToLocationTypeDto_MapsOpenLibraryToKapitelShelf()
    {
        // execute
        var result = this.testee.MetadataSourceToLocationTypeDto(MetadataSources.OpenLibrary);

        // assert
        Assert.That(result, Is.EqualTo(LocationTypeDTO.KapitelShelf));
    }

    /// <summary>
    /// Tests that MetadataSourceToLocationTypeDto maps unknown sources to LocationTypeDTO.KapitelShelf by default.
    /// </summary>
    [Test]
    public void MetadataSourceToLocationTypeDto_MapsUnknownToKapitelShelf()
    {
        // execute
        var result = this.testee.MetadataSourceToLocationTypeDto((MetadataSources)999);

        // assert
        Assert.That(result, Is.EqualTo(LocationTypeDTO.KapitelShelf));
    }
}
