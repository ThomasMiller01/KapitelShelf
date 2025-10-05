// <copyright file="IMetadataLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// The metadata logic interface.
/// </summary>
public interface IMetadataLogic
{
    /// <summary>
    /// Scrapes metadata for a book from a specified source asynchronously.
    /// </summary>
    /// <param name="source">The metadata source.</param>
    /// <param name="title">The title of the book.</param>
    /// <returns>The scraped metadata.</returns>
    Task<List<MetadataDTO>> ScrapeFromSourceAsnyc(MetadataSources source, string title);

    /// <summary>
    /// Download images server-side as a proxy for the client.
    /// </summary>
    /// <param name="coverUrl">The cover url to download.</param>
    /// <returns>The downloaded data and content type.</returns>
    Task<(byte[] data, string contentType)> ProxyCover(string coverUrl);
}
