// <copyright file="TextParser.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.BookParser;

namespace KapitelShelf.Api.Logic.BookParser;

/// <summary>
/// Parse the metadata of text book files.
/// </summary>
public class TextParser : BookParserBase
{
    /// <inheritdoc/>
    public override IReadOnlyCollection<string> SupportedExtensions { get; } = ["txt", "htm", "html", "xhtml", "rtf", "odt"];

    /// <inheritdoc/>
    public override async Task<BookParsingResult> Parse(IFormFile file)
    {
        await Task.CompletedTask;

        ArgumentNullException.ThrowIfNull(file);

        // title
        var title = ParseTitle(file.FileName);

        var bookDto = new BookDTO
        {
            Title = title,
        };

        return new BookParsingResult
        {
            Book = bookDto,
            CoverFile = null,
        };
    }

    private static string ParseTitle(string fileName)
    {
        var withoutExtension = fileName
            .Split(".")
            .First();

        return withoutExtension.Replace("_", " ");
    }
}
