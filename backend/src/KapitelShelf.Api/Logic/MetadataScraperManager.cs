// <copyright file="MetadataScraperManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using FuzzySharp;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Logic.MetadataScraper;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Book metadata scraper factory.
/// </summary>
public class MetadataScraperManager : IMetadataScraperManager
{
    private readonly Dictionary<MetadataSources, Type> metadataScraper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataScraperManager"/> class.
    /// </summary>
    public MetadataScraperManager()
        : this(
            new Dictionary<MetadataSources, Type>
            {
                { MetadataSources.OpenLibrary, typeof(OpenLibraryMetadataScraper) },
            })
    {
    }

    // Needed for unit tests
    internal MetadataScraperManager(Dictionary<MetadataSources, Type> metadataScraper)
    {
        this.metadataScraper = metadataScraper;
    }

    /// <inheritdoc/>
    public async Task<List<MetadataScraperDTO>> Scrape(MetadataSources source, string title)
    {
        // get new parser foreach file
        var scraper = GetNewScraper(source);

        var metadata = await scraper.Scrape(title);

        // extend metadata results with scores for later sorting
        foreach (var item in metadata)
        {
            item.TitleMatchScore = Fuzz.Ratio(item.Title, title);
            item.CompletenessScore = GetCompletenessScore(item);
        }

        return metadata;
    }

    /// <summary>
    /// Compute the completeness score of the metadata.
    /// </summary>
    /// <param name="metadata">The metadata.</param>
    /// <returns>The completeness score.</returns>
    internal static int GetCompletenessScore(MetadataScraperDTO metadata)
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

    /// <summary>
    /// Gets the scraper for the given source.
    /// </summary>
    /// <param name="source">The metadata source.</param>
    /// <returns>The metadata scraper.</returns>
    /// <exception cref="ArgumentException">Scraper could not be found.</exception>
    private IMetadataScraper GetNewScraper(MetadataSources source)
    {
        // get scraper for this source
        var scraperType = this.metadataScraper
            .FirstOrDefault(x => x.Key == source).Value
            ?? throw new ArgumentException("Unsupported metadata source", source.ToString());

        // create new scraper foreach request
        var scraper = Activator.CreateInstance(scraperType) as IMetadataScraper ?? throw new ArgumentException("Scraper must be set");
        return scraper;
    }
}
