// <copyright file="IMetadataScraperManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;

namespace KapitelShelf.Api.Logic;

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
}
