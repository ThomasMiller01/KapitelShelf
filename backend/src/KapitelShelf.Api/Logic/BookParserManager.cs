// <copyright file="BookParserManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.Logic.BookParser;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Logic.Interfaces.BookParser;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Book metadata parser factory.
/// </summary>
public class BookParserManager : IBookParserManager
{
    private readonly List<Type> parserTypes;
    private readonly List<Type> bulkParserTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookParserManager"/> class.
    /// </summary>
    public BookParserManager()
        : this(
            [ // parser types
                typeof(EPUBParser),
                typeof(PDFParser),
                typeof(FB2Parser),
                typeof(TextParser),
                typeof(DocxParser),
                typeof(DocParser),
            ],
            [ // bulk parser types
                typeof(CSVParser),
            ])
    {
    }

    // Needed for unit tests
    internal BookParserManager(List<Type> parserTypes, List<Type> bulkParserTypes)
    {
        this.parserTypes = parserTypes;
        this.bulkParserTypes = bulkParserTypes;
    }

    /// <inheritdoc/>
    public bool IsBulkFile(IFormFile file)
    {
        var extension = ExtractExtension(file);
        return this.bulkParserTypes
            .Any(t =>
            {
                // throwaway instance to check for the supported extensions
                var parser = (IBookParser)Activator.CreateInstance(t)!;
                return parser.SupportedExtensions.Contains(extension);
            });
    }

    /// <inheritdoc/>
    public async Task<BookParsingResult> Parse(IFormFile file)
    {
        if (file is null)
        {
            throw new ArgumentException("File must be set");
        }

        // extract file extension
        var extension = ExtractExtension(file);

        // get new parser foreach file
        var parser = GetNewParser(extension, this.parserTypes);

        return await parser.Parse(file);
    }

    /// <inheritdoc/>
    public async Task<List<BookParsingResult>> ParseBulk(IFormFile file)
    {
        if (file is null)
        {
            throw new ArgumentException("File must be set");
        }

        // extract file extension
        var extension = ExtractExtension(file);

        // get new parser foreach file
        var parser = GetNewParser(extension, this.bulkParserTypes);

        return await parser.ParseBulk(file);
    }

    /// <inheritdoc/>
    public List<string> SupportedFileEndings()
    {
        return this.parserTypes
            .SelectMany(t =>
            {
                // throwaway instance to get the supported extensions
                var parser = (IBookParser)Activator.CreateInstance(t)!;
                return parser.SupportedExtensions;
            }).ToList();
    }

    /// <summary>
    /// Extracts the file extension from the given file.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>The extension.</returns>
    /// <exception cref="ArgumentException">Extension could not be extracted.</exception>
    private static string ExtractExtension(IFormFile file)
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

        return extension;
    }

    /// <summary>
    /// Gets the parser for the given file extension.
    /// </summary>
    /// <param name="extension">The file extension.</param>
    /// <param name="parserTypes">The available parser types.</param>
    /// <returns>The book parser.</returns>
    /// <exception cref="ArgumentException">Parser could not be found.</exception>
    private static IBookParser GetNewParser(string extension, List<Type> parserTypes)
    {
        // get parser for this file extension
        var parserType = parserTypes
            .FirstOrDefault(t =>
            {
                // throwaway instance to check for the supported extensions
                var parser = (IBookParser)Activator.CreateInstance(t)!;
                return parser.SupportedExtensions.Contains(extension);
            }) ?? throw new ArgumentException("Unsupported file extension", extension);

        // create new parser foreach file
        var parser = Activator.CreateInstance(parserType) as IBookParser ?? throw new ArgumentException("Parser must be set");
        return parser;
    }
}
