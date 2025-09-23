// <copyright file="MetadataLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Logic.Interfaces;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The logic for handling metadata operations.
/// </summary>
public class MetadataLogic(IMetadataScraperManager metadataScraperManager, HttpClient httpClient)
{
    private readonly IMetadataScraperManager metadataScraperManager = metadataScraperManager;

    private readonly HttpClient httpClient = httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataLogic"/> class.
    /// </summary>
    /// <param name="metadataScraperManager">The metadata scraper manager.</param>
    public MetadataLogic(IMetadataScraperManager metadataScraperManager)
        : this(metadataScraperManager, new HttpClient())
    {
    }

    /// <summary>
    /// Scrapes metadata for a book from a specified source asynchronously.
    /// </summary>
    /// <param name="source">The metadata source.</param>
    /// <param name="title">The title of the book.</param>
    /// <returns>The scraped metadata.</returns>
    public async Task<List<MetadataDTO>> ScrapeFromSourceAsnyc(MetadataSources source, string title)
    {
        var metadata = await this.metadataScraperManager.Scrape(source, title);

        // sort first by best title, then by completeness
        var sorted = metadata
            .OrderByDescending(item => item.TitleMatchScore)
            .ThenByDescending(item => item.CompletenessScore)
            .ToList();

        return sorted;
    }

    /// <summary>
    /// Download images server-side as a proxy for the client.
    /// </summary>
    /// <param name="coverUrl">The cover url to download.</param>
    /// <returns>The downloaded data and content type.</returns>
    public async Task<(byte[] data, string contentType)> ProxyCover(string coverUrl)
    {
        using var response = await this.httpClient.GetAsync(coverUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsByteArrayAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        return (data, contentType);
    }
}
