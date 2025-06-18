// <copyright file="MetadataScraperBase.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;
using Polly;
using Polly.Retry;

namespace KapitelShelf.Api.Logic.MetadataScraper;

/// <summary>
/// The base class for the metadata scrapers, containing common functionality.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MetadataScraperBase"/> class.
/// </remarks>
public abstract partial class MetadataScraperBase : IMetadataScraper
{
    /// <summary>
    /// The maximum number of results to return from the metadata scraper.
    /// </summary>
    protected static readonly int Limit = 10;

    /// <summary>
    /// The retry policy for the metadata scraping.
    /// </summary>
    /// <remarks>
    /// Polly retry policy: 3 retries with exponential backoff (0.5s, 1s, 2s).
    /// </remarks>
    protected static readonly AsyncRetryPolicy ScrapeRetryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
            [
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
            ]);

    /// <summary>
    /// Headers to use for the metadata scraping requests.
    /// </summary>
    protected static readonly Dictionary<string, string> Headers = new()
    {
        { "user-agent", "Mozilla/5.0 (X11; Linux x86_64; rv:130.0) Gecko/20100101 Firefox/130.0" },
        { "accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/png,image/svg+xml,*/*;q=0.8" },
        { "accept-language", "en-US,en;q=0.9" },
    };

    /// <inheritdoc/>
    public abstract Task<List<MetadataDTO>> Scrape(string title);

    /// <summary>
    /// Create a new http client.
    /// </summary>
    /// <returns>The http client.</returns>
    protected static HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip |
                                         System.Net.DecompressionMethods.Deflate |
                                         System.Net.DecompressionMethods.Brotli,
        };
        return new HttpClient(handler);
    }
}
