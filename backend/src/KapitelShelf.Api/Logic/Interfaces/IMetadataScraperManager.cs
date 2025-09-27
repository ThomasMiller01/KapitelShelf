// <copyright file="IMetadataScraperManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Logic.Interfaces.MetadataScraper;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// Interface for a book metadata scraper factory.
/// </summary>
public interface IMetadataScraperManager
{
    /// <summary>
    /// Scrape the metadata from a source for a title.
    /// </summary>
    /// <param name="source">The metadata source.</param>
    /// <param name="title">The title to filter for.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<List<MetadataDTO>> Scrape(MetadataSources source, string title);

    /// <summary>
    /// Gets the scraper for the given source.
    /// </summary>
    /// <param name="source">The metadata source.</param>
    /// <returns>The metadata scraper.</returns>
    /// <exception cref="ArgumentException">Scraper could not be found.</exception>
    IMetadataScraper GetNewScraper(MetadataSources source);
}
