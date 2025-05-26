// <copyright file="MetadataLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Logic.MetadataScraper;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The logic for handling metadata operations.
/// </summary>
public class MetadataLogic
{
    private readonly Dictionary<MetadataSources, IMetadataScraper> metadataScraper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataLogic"/> class.
    /// </summary>
    public MetadataLogic()
        : this(new Dictionary<MetadataSources, IMetadataScraper>
            {
                { MetadataSources.OpenLibrary, new OpenLibraryMetadataScraper() },
            })
    {
    }

    // Needed for unit tests
    internal MetadataLogic(Dictionary<MetadataSources, IMetadataScraper> metadataScraper)
    {
        this.metadataScraper = metadataScraper;
    }

    /// <summary>
    /// Scrapes metadata for a book from a specified source asynchronously.
    /// </summary>
    /// <param name="source">The metadata source.</param>
    /// <param name="title">The title of the book.</param>
    /// <returns>The scraped metadata.</returns>
    public async Task<List<MetadataDTO>> ScrapeFromSourceAsnyc(MetadataSources source, string title)
    {
        var scraper = this.GetScraper(source);

        return await scraper.Scrape(title);
    }

    private IMetadataScraper GetScraper(MetadataSources source)
    {
        if (!this.metadataScraper.TryGetValue(source, out var scraper))
        {
            throw new ArgumentException($"No metadata scraper found for source {source}");
        }

        return scraper;
    }
}
