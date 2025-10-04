// <copyright file="AmazonScraperTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Net;
using AutoMapper;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic.WatchlistScraper;
using RichardSzalay.MockHttp;

namespace KapitelShelf.Api.Tests.Logic.WatchlistScraper;

/// <summary>
/// Unit tests for the watchlist <see cref="AmazonScraper"/>.
/// </summary>
public class AmazonScraperTests
{
    private IMapper mapper = null!;

    /// <summary>
    /// Setup for each test.
    /// </summary>
    [SetUp]
    public void Setup() => this.mapper = Testhelper.CreateMapper();

    /// <summary>
    /// Tests that Scrape returns results for valid series with multiple ASINs.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Scrape_ReturnsResults_WhenSeriesValid()
    {
        // Setup
        var mockHttp = new MockHttpMessageHandler();

        var asinsJson = /*lang=json,strict*/ "[{\"asin\":\"BOOK1\"},{\"asin\":\"BOOK2\"}]";
        var html = $@"<html><body>
<li data-tag-id='In This Series' data-asins='{WebUtility.HtmlEncode(asinsJson)}'></li>
</body></html>";

        mockHttp.When("https://www.amazon.com/dp/ROOTBOOK")
            .Respond("text/html", html);

        // Fake book detail pages for each asin
        var bookHtml = @"<html><body>
<span id='productTitle'>Sample Title</span>
<span class='author'><a>Sample Author</a></span>
<div data-feature-name='bookDescription'><noscript>Sample Description</noscript></div>
<img class='a-dynamic-image' src='https://images.amazon.com/sample.jpg' />

<!-- series info -->
<div data-feature-name='seriesBulletWidget'>
  <a href='/dp/BOOK1'>Book 1 of 2: Sample Series</a>
</div>

<!-- release date -->
<div data-rpi-attribute-name='book_details-publication_date'>
  <div class='rpi-attribute-value'>January 1, 2024</div>
</div>

<!-- page count -->
<div data-rpi-attribute-name='book_details-ebook_pages'>
  <div class='rpi-attribute-value'>250 pages</div>
</div>
</body></html>";

        mockHttp.When("https://www.amazon.com/dp/BOOK1")
            .Respond("text/html", bookHtml);
        mockHttp.When("https://www.amazon.com/dp/BOOK2")
            .Respond("text/html", bookHtml);

        var scraper = new AmazonScraper(mockHttp.ToHttpClient(), this.mapper);
        var series = new SeriesDTO
        {
            Id = Guid.NewGuid(),
            LastVolume = new()
            {
                Location = new LocationDTO
                {
                    Url = "ROOTBOOK",
                    Type = LocationTypeDTO.Kindle,
                },
            },
        };

        // Execute
        var results = await scraper.Scrape(series);

        // Assert
        Assert.That(results, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(results[0].Title, Is.EqualTo("Sample Title"));
            Assert.That(results[0].Authors, Contains.Item("Sample Author"));
            Assert.That(results[0].CoverUrl, Is.EqualTo("https://images.amazon.com/sample.jpg"));
            Assert.That(results[0].SeriesId, Is.EqualTo(series.Id));
            Assert.That(results[0].LocationType.ToString(), Is.EqualTo("Kindle"));
            Assert.That(results[0].LocationUrl, Is.EqualTo("BOOK1"));
        });
    }

    /// <summary>
    /// Tests that Scrape returns empty when no LastVolume.Location is provided.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Scrape_ReturnsEmpty_WhenNoLocation()
    {
        var scraper = new AmazonScraper(new HttpClient(), this.mapper);
        var series = new SeriesDTO
        {
            Id = Guid.NewGuid(),
            LastVolume = new()
            {
                Location = null,
            },
        };

        var results = await scraper.Scrape(series);

        Assert.That(results, Is.Empty);
    }

    /// <summary>
    /// Tests that Scrape returns empty when ASIN list is empty.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Scrape_ReturnsEmpty_WhenNoAsins()
    {
        var mockHttp = new MockHttpMessageHandler();
        var html = @"<html><body>
<li data-tag-id='In This Series' data-asins='[]'></li>
</body></html>";

        mockHttp.When("https://www.amazon.com/dp/ROOTBOOK")
            .Respond("text/html", html);

        var scraper = new AmazonScraper(mockHttp.ToHttpClient(), this.mapper);
        var series = new SeriesDTO
        {
            Id = Guid.NewGuid(),
            LastVolume = new()
            {
                Location = new LocationDTO
                {
                    Url = "ROOTBOOK",
                    Type = LocationTypeDTO.Kindle,
                },
            },
        };

        var results = await scraper.Scrape(series);

        Assert.That(results, Is.Empty);
    }

    /// <summary>
    /// Tests that Scrape throws when HTTP request fails.
    /// </summary>
    [Test]
    public void Scrape_Throws_OnHttpFailure()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://www.amazon.com/dp/ROOTBOOK")
            .Respond(HttpStatusCode.InternalServerError);

        var scraper = new AmazonScraper(mockHttp.ToHttpClient(), this.mapper);
        var series = new SeriesDTO
        {
            Id = Guid.NewGuid(),
            LastVolume = new()
            {
                Location = new LocationDTO
                {
                    Url = "ROOTBOOK",
                    Type = LocationTypeDTO.Kindle,
                },
            },
        };

        Assert.ThrowsAsync<HttpRequestException>(() => scraper.Scrape(series));
    }

    /// <summary>
    /// Tests that Scrape returns empty when Amazon series node is missing.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Scrape_ReturnsEmpty_WhenNoSeriesNode()
    {
        var mockHttp = new MockHttpMessageHandler();
        var html = @"<html><body><div>No series here</div></body></html>";

        mockHttp.When("https://www.amazon.com/dp/ROOTBOOK")
            .Respond("text/html", html);

        var scraper = new AmazonScraper(mockHttp.ToHttpClient(), this.mapper);
        var series = new SeriesDTO
        {
            Id = Guid.NewGuid(),
            LastVolume = new()
            {
                Location = new LocationDTO
                {
                    Url = "ROOTBOOK",
                    Type = LocationTypeDTO.Kindle,
                },
            },
        };

        var results = await scraper.Scrape(series);

        Assert.That(results, Is.Empty);
    }
}
