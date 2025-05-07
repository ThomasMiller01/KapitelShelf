// <copyright file="BookMetadataParserSession.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.Logic.BookMetadataParser;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The book metadata parser session stores a book file and gets recreated for each book file.
/// </summary>
public class BookMetadataParserSession : IDisposable
{
    private readonly IBookMetadataParser parser;

    private readonly Stream fileStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookMetadataParserSession"/> class.
    /// </summary>
    /// <param name="file">The book file to parse.</param>
    /// <param name="parser">The book parser.</param>
    public BookMetadataParserSession(IBookMetadataParser parser, IFormFile file)
    {
        if (file is null)
        {
            throw new ArgumentException("File must be set");
        }

        this.parser = parser;
        this.fileStream = file.OpenReadStream();
    }

    /// <summary>
    /// Parse the current book file.
    /// </summary>
    /// <returns>The parsed book metadata.</returns>
    public BookDTO Parse() => this.parser.Parse(this.fileStream);

    /// <inheritdoc/>
    public void Dispose() => this.fileStream.Dispose();
}
