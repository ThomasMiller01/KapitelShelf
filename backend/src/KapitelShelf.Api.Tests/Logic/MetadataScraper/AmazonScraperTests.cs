// <copyright file="AmazonScraperTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Net;
using KapitelShelf.Api.Logic.MetadataScraper;
using RichardSzalay.MockHttp;

namespace KapitelShelf.Api.Tests.Logic.MetadataScraper;

/// <summary>
/// Unit tests for the AmazonScraper class.
/// </summary>
[TestFixture]
public class AmazonScraperTests
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

        // 1. Search page response (with one book link)
        var searchHtml = @"
<div data-component-type='s-search-result'>
    <a href='/dp/B000123ABC'></a>
</div>
";
        mockHttp
            .When("https://www.amazon.com/s*")
            .Respond("text/html", searchHtml);

        // 2. Book detail page response (with all metadata fields)
        var bookHtml = @"
<html>
  <body>
    <span id='productTitle'>Test Book Title</span>
    <span class='author'><a>Author One</a></span>
    <span class='author'><a>Author Two</a></span>
    <div data-feature-name='bookDescription'><noscript>A detailed description of the book.</noscript></div>
    <img class='a-dynamic-image' src='https://images.amazon.com/cover.jpg' />
    <div data-feature-name='seriesBulletWidget'>
      <a href='/dp/B000SERIES'>Book 4 of 12: Test Series</a>
    </div>
    <div data-rpi-attribute-name='book_details-publication_date'>
      <div class='rpi-attribute-value'>January 1, 2023</div>
    </div>
    <div data-rpi-attribute-name='book_details-ebook_pages'>
      <div class='rpi-attribute-value'>321 pages</div>
    </div>
  </body>
</html>
";
        mockHttp
            .When("https://www.amazon.com/dp/B000123ABC")
            .Respond("text/html", bookHtml);

        var httpClient = mockHttp.ToHttpClient();
        var scraper = new AmazonScraper(httpClient);

        // Execute
        var results = await scraper.Scrape(Title);

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        var book = results[0];

        Assert.Multiple(() =>
        {
            Assert.That(book.Title, Is.EqualTo("Test Book Title"));
            Assert.That(book.Authors, Is.EquivalentTo(["Author One", "Author Two"]));
            Assert.That(book.Description, Is.EqualTo("A detailed description of the book."));
            Assert.That(book.CoverUrl, Is.EqualTo("https://images.amazon.com/cover.jpg"));
            Assert.That(book.Series, Is.EqualTo("Test Series"));
            Assert.That(book.Volume, Is.EqualTo(4));
            Assert.That(book.ReleaseDate, Contains.Substring("2023"));
            Assert.That(book.Pages, Is.EqualTo(321));
        });
    }

    /// <summary>
    /// Tests that Scrape handles no results gracefully and returns an empty list.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Scrape_Handles_NoBooks_ReturnsEmptyList()
    {
        var mockHttp = new MockHttpMessageHandler();
        var emptySearchHtml = @"<html><body><!-- No results --></body></html>";

        mockHttp
            .When("https://www.amazon.com/s*")
            .Respond("text/html", emptySearchHtml);

        var scraper = new AmazonScraper(mockHttp.ToHttpClient());

        var results = await scraper.Scrape(Title);

        Assert.That(results, Is.Empty);
    }

    /// <summary>
    /// Tests that Scrape throws an exception on HTTP failure (e.g., 500).
    /// </summary>
    [Test]
    public void Scrape_Throws_OnHttpFailure()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When("https://www.amazon.com/s*")
            .Respond(HttpStatusCode.InternalServerError);

        var scraper = new AmazonScraper(mockHttp.ToHttpClient());

        Assert.ThrowsAsync<HttpRequestException>(async () => await scraper.Scrape(Title));
    }

    /// <summary>
    /// Tests that Scrape ignores errors for individual book pages and still returns other results.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Scrape_Ignores_BookPageErrors_AndStillReturnsOtherResults()
    {
        var mockHttp = new MockHttpMessageHandler();

        // Two book links, one will fail
        var searchHtml = @"
<div data-component-type='s-search-result'>
    <a href='/dp/FAILBOOK'></a>
</div>
<div data-component-type='s-search-result'>
    <a href='/dp/SUCCESSBOOK'></a>
</div>
";
        mockHttp
            .When("https://www.amazon.com/s*")
            .Respond("text/html", searchHtml);

        // FAILBOOK returns error
        mockHttp
            .When("https://www.amazon.com/dp/FAILBOOK")
            .Respond(HttpStatusCode.InternalServerError);

        // SUCCESSBOOK returns valid html
        var bookHtml = @"
<html>
  <body>
    <span id='productTitle'>Good Book</span>
    <span class='author'><a>Good Author</a></span>
    <div data-feature-name='bookDescription'><noscript>Good Description.</noscript></div>
    <img class='a-dynamic-image' src='https://images.amazon.com/good.jpg' />
  </body>
</html>
";
        mockHttp
            .When("https://www.amazon.com/dp/SUCCESSBOOK")
            .Respond("text/html", bookHtml);

        var scraper = new AmazonScraper(mockHttp.ToHttpClient());

        var results = await scraper.Scrape(Title);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(results[0].Title, Is.EqualTo("Good Book"));
            Assert.That(results[0].Authors, Is.EquivalentTo(["Good Author"]));
            Assert.That(results[0].Description, Is.EqualTo("Good Description."));
            Assert.That(results[0].CoverUrl, Is.EqualTo("https://images.amazon.com/good.jpg"));
        });
    }

    /// <summary>
    /// Tests that Scrape throws a special exception when Amazon blocks scraping.
    /// </summary>
    [Test]
    public void Scrape_Throws_When_BlockedByAmazon()
    {
        var mockHttp = new MockHttpMessageHandler();

        // One book link
        var searchHtml = @"
<div data-component-type='s-search-result'>
    <a href='/dp/BLOCKEDBOOK'></a>
</div>
";
        mockHttp
            .When("https://www.amazon.com/s*")
            .Respond("text/html", searchHtml);

        // Book detail page triggers bot detection
        var blockedHtml = @"Sorry, we just need to make sure you're not a robot.";
        mockHttp
            .When("https://www.amazon.com/dp/BLOCKEDBOOK")
            .Respond("text/html", blockedHtml);

        var scraper = new AmazonScraper(mockHttp.ToHttpClient());

        Assert.ThrowsAsync<HttpRequestException>(async () => await scraper.Scrape(Title));
    }
}
