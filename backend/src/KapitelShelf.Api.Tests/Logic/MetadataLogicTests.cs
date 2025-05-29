// <copyright file="MetadataLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.MetadataScraper;
using NSubstitute;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the MetadataLogic class.
/// </summary>
[TestFixture]
public class MetadataLogicTests
{
    private IMetadataScraper mockScraper;
    private IMapper mapper;
    private MetadataLogic testee;

    /// <summary>
    /// Sets up before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.mockScraper = Substitute.For<IMetadataScraper>();
        this.mapper = Testhelper.CreateMapper();

        // Setup: testee with dictionary containing our mock scraper
        var scrapers = new Dictionary<MetadataSources, IMetadataScraper>
        {
            { MetadataSources.OpenLibrary, this.mockScraper },
        };
        this.testee = new MetadataLogic(scrapers, this.mapper);
    }

    /// <summary>
    /// Tests ScrapeFromSourceAsnyc returns mapped and sorted metadata.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ScrapeFromSourceAsync_ReturnsMappedAndSorted()
    {
        // Setup
        var inputTitle = "Test Book";
        var resultList = new List<MetadataScraperDTO>
        {
            new()
            {
                Title = "Test Book",
                Description = "Description_1",
            },
            new()
            {
                Title = "Tst Bok",
                Description = "Description 2",
            },
        };
        this.mockScraper.Scrape(inputTitle).Returns(resultList);

        // Execute
        var results = await this.testee.ScrapeFromSourceAsnyc(MetadataSources.OpenLibrary, inputTitle);

        // Assert
        Assert.That(results, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(results[0].Title, Is.EqualTo("Test Book")); // best match first
            Assert.That(results[1].Title, Is.EqualTo("Tst Bok"));
        });
    }

    /// <summary>
    /// Tests ScrapeFromSourceAsnyc throws if source is not present.
    /// </summary>
    [Test]
    public void ScrapeFromSourceAsync_ThrowsArgumentException_IfNoScraper()
    {
        // Setup
        var newLogic = new MetadataLogic([], this.mapper);

        // Execute and Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await newLogic.ScrapeFromSourceAsnyc(MetadataSources.OpenLibrary, "Some Book");
        });
    }

    /// <summary>
    /// Tests ScrapeFromSourceAsnyc sets TitleMatchScore and CompletenessScore.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ScrapeFromSourceAsync_SetsScores()
    {
        // Setup
        var inputTitle = "Exact Title";
        var dto = new MetadataScraperDTO
        {
            Title = "Exact Title",
            Authors = ["Author"],
            Description = "Desc",
            ReleaseDate = "2024-01-01",
            Series = "Series",
            Volume = 1,
            Pages = 300,
            CoverUrl = "http://cover.com/cover.png",
            Categories = ["Fiction"],
            Tags = ["Tag1"],
        };
        this.mockScraper.Scrape(inputTitle).Returns([dto]);

        // Execute
        var results = await this.testee.ScrapeFromSourceAsnyc(MetadataSources.OpenLibrary, inputTitle);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(dto.TitleMatchScore, Is.EqualTo(100)); // perfect match with Fuzz.Ratio
            Assert.That(dto.CompletenessScore, Is.GreaterThan(0)); // all fields filled
        });
    }

    /// <summary>
    /// Tests ScrapeFromSourceAsnyc returns empty list if scraper returns none.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ScrapeFromSourceAsync_EmptyListIfNoResults()
    {
        // Setup
        this.mockScraper.Scrape(Arg.Any<string>()).Returns([]);

        // Execute
        var results = await this.testee.ScrapeFromSourceAsnyc(MetadataSources.OpenLibrary, "Whatever");

        // Assert
        Assert.That(results, Is.Empty);
    }
}
