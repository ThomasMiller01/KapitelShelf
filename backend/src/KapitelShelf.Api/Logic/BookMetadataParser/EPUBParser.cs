// <copyright file="EPUBParser.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Book;

namespace KapitelShelf.Api.Logic.BookMetadataParser;

/// <summary>
/// Parse the metadata of .epub book files.
/// </summary>
public class EPUBParser : IBookMetadataParser
{
    /// <inheritdoc/>
    public IReadOnlyCollection<string> SupportedExtensions { get; } = ["epub"];

    /// <inheritdoc/>
    public BookDTO Parse(Stream fileStream) => throw new NotImplementedException();
}
