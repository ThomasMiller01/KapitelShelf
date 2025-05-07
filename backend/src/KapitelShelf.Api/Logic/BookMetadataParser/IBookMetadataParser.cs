// <copyright file="IBookMetadataParser.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Book;

namespace KapitelShelf.Api.Logic.BookMetadataParser;

/// <summary>
/// The book metadata parser.
/// </summary>
public interface IBookMetadataParser
{
    /// <summary>
    /// Gets the file extensions supported by this parser.
    /// </summary>
    IReadOnlyCollection<string> SupportedExtensions { get; }

    /// <summary>
    /// Parse a book file stream for metadata.
    /// </summary>
    /// <param name="fileStream">The book file stream.</param>
    /// <returns>The metadata of the book.</returns>
    BookDTO Parse(Stream fileStream);
}
