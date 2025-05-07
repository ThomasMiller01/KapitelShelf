// <copyright file="BookMetadataParserFactory.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Logic.BookMetadataParser;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Book metadata parser factory.
/// </summary>
public static class BookMetadataParserFactory
{
    private static readonly List<Type> ParserTypes = [
        typeof(EPUBParser)
    ];

    /// <summary>
    /// Setup a book-metadata parser session to parse the book file.
    /// </summary>
    /// <param name="file">The book file to parse.</param>
    /// <returns>The book metadata.</returns>
    public static BookMetadataParserSession WithFile(IFormFile file)
    {
        if (file is null)
        {
            throw new ArgumentException("File must be set");
        }

        var extension = Path.GetExtension(file.FileName)
            .TrimStart('.')
            .ToLowerInvariant()
            ?? throw new ArgumentException("File name must have an extension", nameof(file));

        var parserType = ParserTypes
            .FirstOrDefault(t =>
            {
                // throwaway instance to check for te supported extensions
                var parser = (IBookMetadataParser)Activator.CreateInstance(t)!;
                return parser.SupportedExtensions.Contains(extension);
            });

        if (parserType is null)
        {
            throw new ArgumentException("Parser must be set");
        }

        // create new parser foreach file
        var parser = Activator.CreateInstance(parserType) as IBookMetadataParser ?? throw new ArgumentException("Parser must be set");
        return new BookMetadataParserSession(parser, file);
    }
}
