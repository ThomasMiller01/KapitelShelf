// <copyright file="BookParserManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.Logic.BookParser;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Book metadata parser factory.
/// </summary>
public class BookParserManager
{
    private static readonly List<Type> ParserTypes = [
        typeof(EPUBParser),
        typeof(PDFParser)
    ];

    /// <summary>
    /// Parse a book file.
    /// </summary>
    /// <param name="file">The book file to parse.</param>
    /// <returns>The book metadata.</returns>
    public async Task<BookParsingResult> Parse(IFormFile file)
    {
        if (file is null)
        {
            throw new ArgumentException("File must be set");
        }

        var extension = Path.GetExtension(file.FileName)
            .TrimStart('.')
            .ToLowerInvariant()
            ?? throw new ArgumentException("File name must have an extension", nameof(file));

        // get parser for this file extension
        var parserType = ParserTypes
            .FirstOrDefault(t =>
            {
                // throwaway instance to check for the supported extensions
                var parser = (IBookParser)Activator.CreateInstance(t)!;
                return parser.SupportedExtensions.Contains(extension);
            }) ?? throw new ArgumentException("Unsupported file extension", extension);

        // create new parser foreach file
        var parser = Activator.CreateInstance(parserType) as IBookParser ?? throw new ArgumentException("Parser must be set");

        return await parser.Parse(file);
    }
}
