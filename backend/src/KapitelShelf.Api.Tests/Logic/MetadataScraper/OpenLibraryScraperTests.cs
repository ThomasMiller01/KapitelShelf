// <copyright file="OpenLibraryScraperTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;
using KapitelShelf.Api.Logic.MetadataScraper;
using RichardSzalay.MockHttp;

namespace KapitelShelf.Api.Tests.Logic.MetadataScraper;

/// <summary>
/// Unit tests for the OpenLibraryScraper class.
/// </summary>
[TestFixture]
public class OpenLibraryScraperTests
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

        // Search API returns 1 result with minimal fields + cover + key for works
        var searchResponse = new
        {
            docs = new[]
            {
                new
                {
                    title = "Test Book",
                    author_name = new[] { "Author One", "Author Two" },
                    first_publish_year = "2024",
                    number_of_pages_median = 321,
                    subject = new[] { "Fiction", "Adventure" },
                    series = (string?)null,
                    series_number = 5,
                    cover_i = 12345,
                    key = "/works/OL12345W",
                },
            },
        };
        var searchJson = JsonSerializer.Serialize(searchResponse);

        // /works API returns additional details (description, tags/subjects)
        var worksResponse = new
        {
            description = "A detailed book.",
            subjects = new[]
            {
                "Fiction",
                "series:Test_Series",
            },
        };
        var worksJson = JsonSerializer.Serialize(worksResponse);

        mockHttp
            .When("https://openlibrary.org/search.json*")
            .Respond("application/json", searchJson);

        mockHttp
            .When("https://openlibrary.org/works/OL12345W.json")
            .Respond("application/json", worksJson);

        var httpClient = mockHttp.ToHttpClient();

        var scraper = new OpenLibraryScraper(httpClient);

        // Execute
        var results = await scraper.Scrape(Title);

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        var book = results[0];

        Assert.Multiple(() =>
        {
            Assert.That(book.Title, Is.EqualTo("Test Book"));
            Assert.That(book.Authors, Is.EquivalentTo(["Author One", "Author Two"]));
            Assert.That(book.ReleaseDate, Is.EqualTo("2024"));
            Assert.That(book.Pages, Is.EqualTo(321));
            Assert.That(book.Categories, Is.EquivalentTo(["Fiction", "Adventure"]));
            Assert.That(book.Series, Is.EqualTo("Test Series")); // series tag extracted and converted
            Assert.That(book.Volume, Is.EqualTo(5));
            Assert.That(book.CoverUrl, Does.Contain("12345-L.jpg"));
            Assert.That(book.Description, Is.EqualTo("A detailed book."));
            Assert.That(book.Tags, Is.EquivalentTo(["Fiction"])); // series: tag should be stripped
        });
    }

    /// <summary>
    /// Tests that Scrape handles no results gracefully and returns an empty list.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Scrape_Handles_NoDocs_ReturnsEmptyList()
    {
        // Setup
        var mockHttp = new MockHttpMessageHandler();
        var emptyResponse = new { docs = Array.Empty<object>() };
        var emptyJson = JsonSerializer.Serialize(emptyResponse);

        mockHttp
            .When("https://openlibrary.org/search.json*")
            .Respond("application/json", emptyJson);

        var scraper = new OpenLibraryScraper(mockHttp.ToHttpClient());

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
            .When("https://openlibrary.org/search.json*")
            .Respond(HttpStatusCode.InternalServerError);

        var scraper = new OpenLibraryScraper(mockHttp.ToHttpClient());

        // Execute and Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => await scraper.Scrape(Title));
    }

    /// <summary>
    /// Tests that Scrape ignores errors when fetching work details and still returns base data.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Scrape_Ignores_WorkDetailsErrors_AndStillReturnsBaseData()
    {
        // Setup
        var mockHttp = new MockHttpMessageHandler();

        // The main search call returns a doc with a key for works
        var searchResponse = new
        {
            docs = new[]
            {
                new
                {
                    title = "ErrorTest",
                    key = "/works/OL99999W",
                },
            },
        };
        mockHttp
            .When("https://openlibrary.org/search.json*")
            .Respond("application/json", JsonSerializer.Serialize(searchResponse));

        // The /works call returns 404
        mockHttp
            .When("https://openlibrary.org/works/OL99999W.json")
            .Respond(HttpStatusCode.NotFound);

        var scraper = new OpenLibraryScraper(mockHttp.ToHttpClient());

        // Execute
        var results = await scraper.Scrape("ErrorTest");

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(results[0].Title, Is.EqualTo("ErrorTest"));

            // No description/tags populated, but base info is present
            Assert.That(results[0].Description, Is.Null.Or.Empty);
            Assert.That(results[0].Tags, Is.Empty);
        });
    }
}
