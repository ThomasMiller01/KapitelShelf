// <copyright file="MetadataLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using FuzzySharp;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Logic.MetadataScraper;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The logic for handling metadata operations.
/// </summary>
public class MetadataLogic
{
    private readonly Dictionary<MetadataSources, IMetadataScraper> metadataScraper;

    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataLogic"/> class.
    /// </summary>
    /// <param name="mapper">The auto mapper.</param>
    public MetadataLogic(IMapper mapper)
        : this(
            new Dictionary<MetadataSources, IMetadataScraper>
            {
                { MetadataSources.OpenLibrary, new OpenLibraryMetadataScraper() },
            },
            mapper)
    {
    }

    // Needed for unit tests
    internal MetadataLogic(Dictionary<MetadataSources, IMetadataScraper> metadataScraper, IMapper mapper)
    {
        this.metadataScraper = metadataScraper;
        this.mapper = mapper;
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

        var metadata = await scraper.Scrape(title);

        // extend metadata results with scores for later sorting
        foreach (var item in metadata)
        {
            item.TitleMatchScore = Fuzz.Ratio(item.Title, title);
            item.CompletenessScore = GetCompletenessScore(item);
        }

        // sort first by best title, then by completeness
        var sorted = metadata
            .OrderByDescending(item => item.TitleMatchScore)
            .ThenByDescending(item => item.CompletenessScore)
            .Select(this.mapper.Map<MetadataDTO>)
            .ToList();

        return sorted;
    }

    private IMetadataScraper GetScraper(MetadataSources source)
    {
        if (!this.metadataScraper.TryGetValue(source, out var scraper))
        {
            throw new ArgumentException($"No metadata scraper found for source {source}");
        }

        return scraper;
    }

    // Calculate how complete the metadata is.
    private static int GetCompletenessScore(MetadataScraperDTO metadata)
    {
        int score = 0;

        if (!string.IsNullOrWhiteSpace(metadata.Title))
        {
            score++;
        }

        if (metadata.Authors != null && metadata.Authors.Count > 0)
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(metadata.ReleaseDate))
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(metadata.Description))
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(metadata.Series))
        {
            score++;
        }

        if (metadata.Volume.HasValue)
        {
            score++;
        }

        if (metadata.Pages.HasValue)
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(metadata.CoverUrl))
        {
            score++;
        }

        if (metadata.Categories != null && metadata.Categories.Count > 0)
        {
            score++;
        }

        if (metadata.Tags != null && metadata.Tags.Count > 0)
        {
            score++;
        }

        return score;
    }
}
