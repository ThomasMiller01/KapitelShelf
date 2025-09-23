// <copyright file="MetadataLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.Interfaces;
using NSubstitute;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the MetadataLogic class.
/// </summary>
[TestFixture]
public class MetadataLogicTests
{
    private IMetadataScraperManager scraperManager;
    private MetadataLogic logic;

    /// <summary>
    /// Sets up before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.scraperManager = Substitute.For<IMetadataScraperManager>();
        this.logic = new MetadataLogic(this.scraperManager);
    }

    /// <summary>
    /// Tests that ScrapeFromSourceAsnyc sorts and maps metadata as expected.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task ScrapeFromSourceAsnyc_ReturnsSortedAndMappedResults()
    {
        // Setup
        var inputList = new List<MetadataDTO>
        {
            new()
            {
                Title = "B",
                TitleMatchScore = 50,
                CompletenessScore = 1,
            },
            new()
            {
                Title = "A",
                TitleMatchScore = 90,
                CompletenessScore = 3,
            },
            new()
            {
                Title = "C",
                TitleMatchScore = 90,
                CompletenessScore = 2,
            },
        };
        this.scraperManager.Scrape(MetadataSources.OpenLibrary, "title").Returns(inputList);

        // Execute
        var result = await this.logic.ScrapeFromSourceAsnyc(MetadataSources.OpenLibrary, "title");

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Title, Is.EqualTo("A")); // best TitleMatchScore, best CompletenessScore
            Assert.That(result[1].Title, Is.EqualTo("C")); // same TitleMatchScore, lower CompletenessScore
            Assert.That(result[2].Title, Is.EqualTo("B")); // lower TitleMatchScore
        });
    }

    /// <summary>
    /// Tests that ScrapeFromSourceAsnyc returns an empty list if manager returns none.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task ScrapeFromSourceAsnyc_ReturnsEmpty_IfManagerReturnsNone()
    {
        // Setup
        this.scraperManager.Scrape(Arg.Any<MetadataSources>(), Arg.Any<string>())
            .Returns([]);

        // Execute
        var result = await this.logic.ScrapeFromSourceAsnyc(MetadataSources.OpenLibrary, "title");

        // Assert
        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Tests that ScrapeFromSourceAsnyc maps all items using the mapper.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task ScrapeFromSourceAsnyc_MapsAllItems()
    {
        // Setup
        var inputList = new List<MetadataDTO>
        {
            new()
            {
                Title = "A",
            },
            new()
            {
                Title = "B",
            },
        };
        this.scraperManager.Scrape(Arg.Any<MetadataSources>(), Arg.Any<string>()).Returns(inputList);

        // Execute
        var result = await this.logic.ScrapeFromSourceAsnyc(MetadataSources.OpenLibrary, "title");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
        });
    }
}
