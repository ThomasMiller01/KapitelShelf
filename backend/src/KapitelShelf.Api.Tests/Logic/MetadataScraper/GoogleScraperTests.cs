// <copyright file="GoogleScraperTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;
using KapitelShelf.Api.Logic.MetadataScraper;
using RichardSzalay.MockHttp;

namespace KapitelShelf.Api.Tests.Logic.MetadataScraper;

/// <summary>
/// Unit tests for the GoogleScraper class.
/// </summary>
[TestFixture]
public class GoogleScraperTests
{
    private const string Title = "Test Book";

    /// <summary>
    /// Tests that Scrape returns metadata with all expected fields populated.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Scrape_ReturnsMetadata_WithAllExpectedFields()
    {
        // Setup
        var mockHttp = new MockHttpMessageHandler();

        // Sample API response (1 book)
        var apiResponse = new
        {
            items = new[]
            {
                new
                {
                    volumeInfo = new
                    {
                        title = "Test Book Title",
                        authors = new[] { "Author A", "Author B" },
                        description = "A great book about testing.",
                        publishedDate = "2022-08-10",
                        pageCount = 444,
                        categories = new[] { "Testing", "Development" },
                        imageLinks = new
                        {
                            thumbnail = "https://covers.google.com/cover.jpg",
                        },
                    },
                },
            },
        };
        var json = JsonSerializer.Serialize(apiResponse);

        mockHttp
            .When("https://www.googleapis.com/books/v1/volumes*")
            .Respond("application/json", json);

        var scraper = new GoogleScraper(mockHttp.ToHttpClient());

        // Execute
        var results = await scraper.Scrape(Title);

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        var book = results[0];
        Assert.Multiple(() =>
        {
            Assert.That(book.Title, Is.EqualTo("Test Book Title"));
            Assert.That(book.Authors, Is.EquivalentTo(["Author A", "Author B"]));
            Assert.That(book.Description, Is.EqualTo("A great book about testing."));
            Assert.That(book.ReleaseDate, Is.EqualTo("2022-08-10"));
            Assert.That(book.Pages, Is.EqualTo(444));
            Assert.That(book.Categories, Is.EquivalentTo(["Testing", "Development"]));
            Assert.That(book.CoverUrl, Is.EqualTo("https://covers.google.com/cover.jpg"));
            Assert.That(book.Series, Is.Null);
            Assert.That(book.Volume, Is.Null);
            Assert.That(book.Tags, Is.Empty);
        });
    }

    /// <summary>
    /// Tests that Scrape handles no items and returns an empty list.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Scrape_Handles_NoItems_ReturnsEmptyList()
    {
        // Setup
        var mockHttp = new MockHttpMessageHandler();
        var emptyResponse = new { }; // no "items" property
        var emptyJson = JsonSerializer.Serialize(emptyResponse);

        mockHttp
            .When("https://www.googleapis.com/books/v1/volumes*")
            .Respond("application/json", emptyJson);

        var scraper = new GoogleScraper(mockHttp.ToHttpClient());

        // Execute
        var results = await scraper.Scrape(Title);

        // Assert
        Assert.That(results, Is.Empty);
    }

    /// <summary>
    /// Tests that Scrape throws an exception on HTTP failure (e.g., 500).
    /// </summary>
    [Test]
    public void Scrape_Throws_OnHttpFailure()
    {
        // Setup
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When("https://www.googleapis.com/books/v1/volumes*")
            .Respond(HttpStatusCode.InternalServerError);

        var scraper = new GoogleScraper(mockHttp.ToHttpClient());

        // Execute & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => await scraper.Scrape(Title));
    }

    /// <summary>
    /// Tests that Scrape ignores items with missing volumeInfo and still returns valid books.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Scrape_Ignores_ItemsWithMissingVolumeInfo_AndStillReturnsValidBooks()
    {
        // Setup
        var mockHttp = new MockHttpMessageHandler();

        // 2 items: one with volumeInfo, one without
        var apiResponse = new
        {
            items = new object[]
            {
                new
                {
                    volumeInfo = new
                    {
                        title = "Real Book",
                        authors = new[] { "Someone" },
                    },
                },
                new
                {
                },
            },
        };
        var json = JsonSerializer.Serialize(apiResponse);

        mockHttp
            .When("https://www.googleapis.com/books/v1/volumes*")
            .Respond("application/json", json);

        var scraper = new GoogleScraper(mockHttp.ToHttpClient());

        // Execute
        var results = await scraper.Scrape(Title);

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Title, Is.EqualTo("Real Book"));
    }
}
