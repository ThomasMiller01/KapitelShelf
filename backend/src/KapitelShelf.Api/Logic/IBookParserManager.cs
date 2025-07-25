// <copyright file="IBookParserManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.BookParser;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Interface for a book metadata parser factory.
/// </summary>
public interface IBookParserManager
{
    /// <summary>
    /// Parses a book file and extracts metadata.
    /// </summary>
    /// <param name="file">The book file to parse.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="BookParsingResult"/>.</returns>
    Task<BookParsingResult> Parse(IFormFile file);

    /// <summary>
    /// Parses a bulk file and extracts metadata for multiple books.
    /// </summary>
    /// <param name="file">The bulk file.</param>
    /// <returns>The list of parsed books.</returns>
    Task<List<BookParsingResult>> ParseBulk(IFormFile file);

    /// <summary>
    /// Checks if the given file is a bulk file that can be parsed by a bulk parser.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>True if it is a bulk file, otherwise false.</returns>
    bool IsBulkFile(IFormFile file);

    /// <summary>
    /// Get a list of the supported file endings.
    /// </summary>
    /// <returns>The supported file endings.</returns>
    List<string> SupportedFileEndings();
}
