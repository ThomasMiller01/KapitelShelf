// <copyright file="IBookParser.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.BookParser;

namespace KapitelShelf.Api.Logic.BookParser;

/// <summary>
/// The book metadata parser.
/// </summary>
public interface IBookParser
{
    /// <summary>
    /// Gets the file extensions supported by this parser.
    /// </summary>
    IReadOnlyCollection<string> SupportedExtensions { get; }

    /// <summary>
    /// Parse a book file stream for metadata.
    /// </summary>
    /// <param name="file">The book file to parse.</param>
    /// <returns>The book parsing result.</returns>
    Task<BookParsingResult> Parse(IFormFile file);
}
