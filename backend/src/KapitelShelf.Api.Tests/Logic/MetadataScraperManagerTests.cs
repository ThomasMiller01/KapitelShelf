// <copyright file="MetadataScraperManagerTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.MetadataScraper;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the MetadataScraperManager class.
/// </summary>
[TestFixture]
public class MetadataScraperManagerTests
{
    private MetadataScraperManager testee;
    private static List<MetadataScraperDTO> scraperReturnList;

    /// <summary>
    /// Setup: Use a dictionary that points to our fake/mock scraper type.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        scraperReturnList = null!;
        var scraperDict = new Dictionary<MetadataSources, Type>
        {
            { MetadataSources.OpenLibrary, typeof(FakeScraper) },
        };
        this.testee = new MetadataScraperManager(scraperDict);
    }

    /// <summary>
    /// Tests Scrape returns and scores data from the scraper.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Scrape_ReturnsScoredData()
    {
        // Setup
        scraperReturnList =
        [
            new MetadataScraperDTO
            {
                Title = "Match",
                Authors = ["A"],
                ReleaseDate = "2023",
                Description = "Description",
                Series = "S",
                Volume = 1,
                Pages = 100,
                CoverUrl = "url",
                Categories = ["category"],
                Tags = ["tag"],
            },
            new MetadataScraperDTO
            {
                Title = "Other",
                Authors = [],
                ReleaseDate = null,
                Description = null,
            },
        ];

        // Execute
        var result = await this.testee.Scrape(MetadataSources.OpenLibrary, "Match");

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Title, Is.EqualTo("Match"));
            Assert.That(result[0].TitleMatchScore, Is.GreaterThan(0));
            Assert.That(result[0].CompletenessScore, Is.GreaterThan(result[1].CompletenessScore));
        });
    }

    /// <summary>
    /// Tests Scrape throws if scraper type is missing.
    /// </summary>
    [Test]
    public void Scrape_ThrowsIfScraperMissing()
    {
        // Setup
        var manager = new MetadataScraperManager([]);

        // Execute and Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await manager.Scrape(MetadataSources.OpenLibrary, "title"));
        Assert.That(ex!.Message, Does.Contain("Unsupported metadata source"));
    }

    /// <summary>
    /// Tests Scrape throws if scraper type does not implement IMetadataScraper.
    /// </summary>
    [Test]
    public void Scrape_ThrowsIfScraperInvalid()
    {
        var manager = new MetadataScraperManager(new Dictionary<MetadataSources, Type>
        {
            { MetadataSources.OpenLibrary, typeof(object) },
        });

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await manager.Scrape(MetadataSources.OpenLibrary, "title"));
        Assert.That(ex!.Message, Does.Contain("Scraper must be set"));
    }

    /// <summary>
    /// Tests that Scrape returns an empty list if scraper returns empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Scrape_ReturnsEmptyList_IfScraperReturnsNone()
    {
        scraperReturnList = [];

        var result = await this.testee.Scrape(MetadataSources.OpenLibrary, "title");
        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// A fake metadata scraper for testing.
    /// </summary>
    private sealed class FakeScraper : IMetadataScraper
    {
        public Task<List<MetadataScraperDTO>> Scrape(string title) => Task.FromResult(scraperReturnList ?? []);
    }
}
