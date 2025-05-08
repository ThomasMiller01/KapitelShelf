// <copyright file="BookParsingResult.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Book;

namespace KapitelShelf.Api.DTOs.BookParser;

/// <summary>
/// The result of the parsed book.
/// </summary>
public class BookParsingResult
{
    /// <summary>
    /// Gets or sets the parsed book metadata.
    /// </summary>
    public BookDTO Book { get; set; } = null!;

    /// <summary>
    /// Gets or sets the cover file.
    /// </summary>
    public IFormFile? CoverFile { get; set; }
}
