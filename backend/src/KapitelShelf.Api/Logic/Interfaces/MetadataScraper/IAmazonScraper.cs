// <copyright file="IAmazonScraper.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;

namespace KapitelShelf.Api.Logic.Interfaces.MetadataScraper;

/// <summary>
/// The inferface for metadata scrapers.
/// </summary>
public interface IAmazonScraper : IMetadataScraper
{
    /// <summary>
    /// Scrapes metadata for a book based on its asin.
    /// </summary>
    /// <param name="asin">The asin of the book.</param>
    /// <returns>A task representing the asynchronous operation, containing the scraped metadata.</returns>
    Task<MetadataDTO?> ScrapeFromAsin(string asin);
}
