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
}
