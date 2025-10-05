// <copyright file="WatchlistScraperManagerTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.Interfaces.WatchlistScraper;
using KapitelShelf.Data.Models.Watchlists;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the MetadataScraperManager class.
/// </summary>
public class WatchlistScraperManagerTests
{
    private Mapper mapper;
    private WatchlistScraperManager testee;
    private static List<WatchlistResultModel> scraperReturnList;

    /// <summary>
    /// Setup: Use a dictionary that points to our fake/mock scraper type.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        scraperReturnList = null!;

        this.mapper = Testhelper.CreateMapper();
        var scraperDict = new Dictionary<LocationTypeDTO, Type>
        {
            { LocationTypeDTO.Kindle, typeof(FakeScraper) },
        };
        this.testee = new WatchlistScraperManager(scraperDict, this.mapper);
    }

    /// <summary>
    /// Tests Scrape throws if scraper type is missing.
    /// </summary>
    [Test]
    public void Scrape_ThrowsIfScraperMissing()
    {
        // Setup
        var series = new SeriesDTO
        {
            LastVolume = new()
            {
                Location = new LocationDTO
                {
                    Type = LocationTypeDTO.Physical,
                },
            },
        };

        // Execute and Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.testee.Scrape(series));
        Assert.That(ex!.Message, Does.Contain("Unsupported location type"));
    }

    /// <summary>
    /// Tests Scrape throws if scraper type does not implement IWatchlistScraper.
    /// </summary>
    [Test]
    public void Scrape_ThrowsIfInvalidLocation()
    {
        // Setup
        var series = new SeriesDTO
        {
            LastVolume = new()
            {
                Location = null,
            },
        };
        this.testee = new WatchlistScraperManager([], this.mapper);

        // Execute and Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.testee.Scrape(series));
        Assert.That(ex!.Message, Does.Contain("'Series.LastVolume.Location' must be set"));
    }

    /// <summary>
    /// Tests that Scrape returns an empty list if scraper returns empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Scrape_ReturnsEmptyList_IfScraperReturnsNone()
    {
        // Setup
        scraperReturnList = [];

        var series = new SeriesDTO
        {
            LastVolume = new()
            {
                Location = new LocationDTO
                {
                    Type = LocationTypeDTO.Kindle,
                },
            },
        };

        // Execute and Assert
        var result = await this.testee.Scrape(series);
        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// A fake watchlist scraper for testing.
    /// </summary>
#pragma warning disable CS9113 // Parameter is unread.
    private sealed class FakeScraper(Mapper mapper) : IWatchlistScraper
#pragma warning restore CS9113 // Parameter is unread.
    {
        public Task<List<WatchlistResultModel>> Scrape(SeriesDTO series) => Task.FromResult(scraperReturnList ?? []);

        public Task<List<MetadataDTO>> Scrape(string _) => throw new NotImplementedException();
    }
}
