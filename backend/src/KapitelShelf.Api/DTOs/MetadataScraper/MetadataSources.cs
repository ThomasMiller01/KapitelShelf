// <copyright file="MetadataSources.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.MetadataScraper;

/// <summary>
/// The metadata sources for book metadata scraping.
/// </summary>
public enum MetadataSources
{
    /// <summary>
    /// OpenLibrary.
    /// </summary>
    OpenLibrary = 0,

    /// <summary>
    /// Google Books.
    /// </summary>
    GoogleBooks = 1,

    /// <summary>
    /// Amazon Books.
    /// </summary>
    Amazon = 2,
}
