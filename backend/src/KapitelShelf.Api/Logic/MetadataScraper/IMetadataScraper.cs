// <copyright file="IMetadataScraper.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;

namespace KapitelShelf.Api.Logic.MetadataScraper;

/// <summary>
/// The inferface for metadata scrapers.
/// </summary>
public interface IMetadataScraper
{
    /// <summary>
    /// Scrapes metadata for a book.
    /// </summary>
    /// <param name="title">The title of the book.</param>
    /// <returns>A task representing the asynchronous operation, containing the scraped metadata.</returns>
    Task<List<MetadataDTO>> Scrape(string title);
}
