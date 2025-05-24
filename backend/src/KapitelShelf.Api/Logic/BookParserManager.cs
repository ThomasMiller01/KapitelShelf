// <copyright file="BookParserManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.Logic.BookParser;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Book metadata parser factory.
/// </summary>
public class BookParserManager : IBookParserManager
{
    private readonly List<Type> parserTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookParserManager"/> class.
    /// </summary>
    public BookParserManager()
        : this([
            typeof(EPUBParser),
            typeof(PDFParser),
            typeof(FB2Parser),
            typeof(TextParser),
            typeof(DocxParser),
            typeof(DocParser),
        ])
    {
    }

    // Needed for unit tests
    internal BookParserManager(List<Type> parserTypes)
    {
        this.parserTypes = parserTypes;
    }

    /// <inheritdoc/>
    public async Task<BookParsingResult> Parse(IFormFile file)
    {
        if (file is null)
        {
            throw new ArgumentException("File must be set");
        }

        // extract file extension
        var extension = Path.GetExtension(file.FileName)
            .TrimStart('.')
            .ToLowerInvariant();

        if (string.IsNullOrEmpty(extension))
        {
            throw new ArgumentException("File name must have an extension", nameof(file));
        }

        // get parser for this file extension
        var parserType = this.parserTypes
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
