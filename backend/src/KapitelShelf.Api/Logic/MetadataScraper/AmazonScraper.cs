// <copyright file="AmazonScraper.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Logic.Interfaces.MetadataScraper;
using KapitelShelf.Api.Settings;

namespace KapitelShelf.Api.Logic.MetadataScraper;

/// <summary>
/// The Amazon metadata scraper.
/// </summary>
public partial class AmazonScraper(HttpClient httpClient) : MetadataScraperBase, IAmazonScraper
{
    /// <summary>
    /// Override the limit for amazon.
    /// </summary>
    /// <remarks>
    /// Amazon blocks scraping requests fairly quickly.
    /// </remarks>
    protected static new readonly int Limit = 5;

    private readonly HttpClient httpClient = httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AmazonScraper"/> class.
    /// </summary>
    public AmazonScraper()
        : this(CreateHttpClient())
    {
    }

    /// <inheritdoc />
    public override async Task<List<MetadataDTO>> Scrape(string title)
    {
        var results = new List<MetadataDTO>();

        // Setup headers
        foreach (var header in Headers)
        {
            if (this.httpClient.DefaultRequestHeaders.Contains(header.Key))
            {
                this.httpClient.DefaultRequestHeaders.Remove(header.Key);
            }

            this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Scrape book list from amazon
        var bookListUrl = $"https://www.amazon.com/s?k={Uri.EscapeDataString(title)}&i=digital-text";

        using var bookListResponse = await ScrapeRetryPolicy.ExecuteAsync(() => this.httpClient.GetAsync(bookListUrl));
        bookListResponse.EnsureSuccessStatusCode();

        var bookListHtml = await bookListResponse.Content.ReadAsStringAsync();
        var bookListDocument = new HtmlDocument();
        bookListDocument.LoadHtml(bookListHtml);

        // extract book links
        var bookListNodes = bookListDocument.DocumentNode
            .SelectNodes("//div[@data-component-type='s-search-result']");
        var bookLinkNodes = bookListNodes?
            .Select(node => node.SelectSingleNode(".//a[contains(@href, '/dp/')]")?.GetAttributeValue("href", string.Empty));
        var bookLinks = bookLinkNodes?
            .Where(href => !string.IsNullOrWhiteSpace(href))
            .Distinct()
            .Take(Limit)
            .ToList() ?? [];

        // Check if any books were found
        if (bookLinks == null || bookLinks.Count == 0)
        {
            return results;
        }

        // Parse each book link seperately
        var tasks = bookLinks
            .Where(x => x != null)
            .Select(x => ParseBookLink(x!))
            .ToList();

        var taskResults = await Task.WhenAll(tasks);
        results.AddRange(taskResults.Where(x => x != null).Select(x => x!));

        return results;
    }

    /// <inheritdoc/>
    public async Task<MetadataDTO?> ScrapeFromAsin(string asin)
    {
        // Setup headers
        foreach (var header in Headers)
        {
            if (this.httpClient.DefaultRequestHeaders.Contains(header.Key))
            {
                this.httpClient.DefaultRequestHeaders.Remove(header.Key);
            }

            this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Parse book
        return await this.ParseBookLink($"/dp/{asin}");
    }

    private async Task<MetadataDTO?> ParseBookLink(string bookLink)
    {
        var bookUrl = $"https://www.amazon.com{bookLink}";
        try
        {
            using var bookResponse = await ScrapeRetryPolicy.ExecuteAsync(() => this.httpClient.GetAsync(bookUrl));
            bookResponse.EnsureSuccessStatusCode();

            var bookHtml = await bookResponse.Content.ReadAsStringAsync();
            var bookDocument = new HtmlDocument();
            bookDocument.LoadHtml(bookHtml);

            if (bookHtml.Contains("Sorry, we just need to make sure you're not a robot."))
            {
                // Blocked by amazon, possibly due to bot detection
                throw new HttpRequestException(StaticConstants.MetadataScrapingBlockedKey);
            }

            // Title
            var titleNode = bookDocument.DocumentNode.SelectSingleNode("//span[@id='productTitle']");
            var bookTitle = titleNode?.InnerText.Trim() ?? string.Empty;

            // Authors
            var authorNodes = bookDocument.DocumentNode.SelectNodes("//span[contains(@class, 'author')]");
            var authors = authorNodes?
                .Select(n => n.SelectSingleNode(".//a")?.InnerText.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x) && x != "Follow")
                .Distinct()
                .ToList() ?? [];

            // Description
            string? description = ParseDescription(bookDocument);

            // Cover image
            var coverNode = bookDocument.DocumentNode.SelectSingleNode("//img[contains(@class, 'a-dynamic-image')]");
            var coverUrl = coverNode?.GetAttributeValue("src", string.Empty);

            // Series
            (string? series, int? volume) = ParseSeries(bookDocument);

            // Release Date
            var releaseDate = ParseReleaseDate(bookDocument);

            // Pages
            var pages = ParsePages(bookDocument);

            return new MetadataDTO
            {
                Title = bookTitle,
                Authors = authors.Where(x => x != null).Select(x => x ?? string.Empty).ToList() ?? [],
                Description = description,
                CoverUrl = coverUrl,
                Series = series,
                Volume = volume,
                ReleaseDate = releaseDate,
                Pages = pages,
                Categories = [],
                Tags = [],
            };
        }
        catch (HttpRequestException ex) when (ex.Message == StaticConstants.MetadataScrapingBlockedKey)
        {
            // Rethrow
            throw;
        }
        catch
        {
            // ignore errors for individual books, continue
            return null;
        }
    }

    private static string? ParseDescription(HtmlDocument document)
    {
        var descriptionNode = document.DocumentNode.SelectSingleNode("//div[@data-feature-name='bookDescription']//noscript")
            ?? document.DocumentNode.SelectSingleNode("//div[@data-feature-name='bookDescription']");

        if (descriptionNode is null)
        {
            return null;
        }

        // Sometimes the description is inside <noscript> as HTML
        var descriptionHtml = descriptionNode.InnerHtml;
        var descriptionDocument = new HtmlDocument();
        descriptionDocument.LoadHtml(descriptionHtml);

        return CleanSpaces().Replace(descriptionDocument.DocumentNode.InnerText, " ").Trim();
    }

    private static (string? series, int? volume) ParseSeries(HtmlDocument document)
    {
        var seriesNode = document.DocumentNode
                .SelectSingleNode("//div[@data-feature-name='seriesBulletWidget']")?
                .SelectSingleNode(".//a[contains(@href, '/dp/')]");

        if (seriesNode is null)
        {
            return (null, null);
        }

        var seriesNodeText = seriesNode.InnerText.Trim() ?? string.Empty;

        // Book Pattern
        // e.g. Book 4 of 12: He Who Fights with Monsters
        var bookPatternMatch = SeriesBookPattern().Match(seriesNodeText);
        if (bookPatternMatch.Success && bookPatternMatch.Groups.Count == 3)
        {
            var series = bookPatternMatch.Groups[2].Value.Trim();
            int volume = 0;
            if (int.TryParse(bookPatternMatch.Groups[1].Value, out var volumeInt))
            {
                volume = volumeInt;
            }

            return (series, volume);
        }

        // Book Pattern
        // e.g. Part of: He Who Fights with Monsters (12 books)
        var partOfPatternMatch = SeriesPartOfPattern().Match(seriesNodeText);
        if (partOfPatternMatch.Success && partOfPatternMatch.Groups.Count == 2)
        {
            var series = partOfPatternMatch.Groups[1].Value.Trim();
            int? volume = null;

            return (series, volume);
        }

        // Unknown series format
        return (null, null);
    }

    private static string? ParseReleaseDate(HtmlDocument document)
    {
        var releaseDateNode = document.DocumentNode
            .SelectSingleNode("//div[@data-rpi-attribute-name='book_details-publication_date']");

        if (releaseDateNode is null)
        {
            return null;
        }

        var releaseDateValueNode = releaseDateNode.SelectSingleNode(".//div[contains(@class, 'rpi-attribute-value')]");
        if (releaseDateValueNode is null)
        {
            return null;
        }

        var releaseDateText = releaseDateValueNode.InnerText.Trim();
        if (DateTime.TryParse(releaseDateText, CultureInfo.InvariantCulture, out var dt))
        {
            return dt.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            return null;
        }
    }

    private static int? ParsePages(HtmlDocument document)
    {
        var pagesNode = document.DocumentNode.SelectSingleNode("//div[@data-rpi-attribute-name='book_details-ebook_pages']")
            ?? document.DocumentNode.SelectSingleNode("//div[@data-rpi-attribute-name='book_details-fiona_pages']");

        if (pagesNode is null)
        {
            return null;
        }

        var pagesValueNode = pagesNode.SelectSingleNode(".//div[contains(@class, 'rpi-attribute-value')]");
        if (pagesValueNode is null)
        {
            return null;
        }

        var pagesText = pagesValueNode.InnerText.Trim().Split(" ").First();
        if (int.TryParse(pagesText, out var pagesInt))
        {
            return pagesInt;
        }
        else
        {
            return null;
        }
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex CleanSpaces();

    /// <summary>
    /// Series book pattern, e.g. "Book 4 of 12: He Who Fights with Monsters".
    /// </summary>
    [GeneratedRegex(@"Book\s+(\d+)\s+of\s+\d+:\s*(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex SeriesBookPattern();

    /// <summary>
    /// Series part of pattern, e.g. "Part of: He Who Fights with Monsters (12 books)".
    /// </summary>
    [GeneratedRegex(@"Part of:\s*(.+?)(?:\s*\(\d+\s*books?\))?$", RegexOptions.IgnoreCase)]
    private static partial Regex SeriesPartOfPattern();
}
