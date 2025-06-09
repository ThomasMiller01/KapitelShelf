// <copyright file="MetadataLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The logic for handling metadata operations.
/// </summary>
public class MetadataLogic(IMetadataScraperManager metadataScraperManager)
{
    private readonly IMetadataScraperManager metadataScraperManager = metadataScraperManager;

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
}
