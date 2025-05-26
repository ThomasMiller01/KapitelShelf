// <copyright file="OpenLibraryMetadataScraper.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;

namespace KapitelShelf.Api.Logic.MetadataScraper;

/// <summary>
/// The OpenLibrary metadata scraper.
/// </summary>
public class OpenLibraryMetadataScraper : IMetadataScraper
{
    /// <inheritdoc />
    public Task<List<MetadataDTO>> Scrape(string title) => throw new NotImplementedException();
}
