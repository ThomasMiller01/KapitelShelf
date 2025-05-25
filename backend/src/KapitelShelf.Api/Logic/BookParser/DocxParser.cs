// <copyright file="DocxParser.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using DocumentFormat.OpenXml.Packaging;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.BookParser;

namespace KapitelShelf.Api.Logic.BookParser;

/// <summary>
/// Parse the metadata of .docx book files.
/// </summary>
public class DocxParser : BookParserBase
{
    /// <inheritdoc/>
    public override IReadOnlyCollection<string> SupportedExtensions { get; } = ["docx"];

    /// <inheritdoc/>
    public override async Task<BookParsingResult> Parse(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        using var ms = new MemoryStream();
        await file.OpenReadStream().CopyToAsync(ms);
        ms.Position = 0;

        using var document = WordprocessingDocument.Open(ms, false);
        var props = document.PackageProperties;

        // title
        var title = string.IsNullOrEmpty(props.Title) ? this.ParseTitleFromFile(file.FileName) : props.Title;

        // description
        var description = props.Subject ?? string.Empty;

        // author
        var (firstName, lastName) = this.ParseAuthor(props.Creator ?? string.Empty);

        // releaseDate
        var releaseDate = props.Created?.ToUniversalTime();

        var bookDto = new BookDTO
        {
            Title = title,
            Description = description,
            Author = new AuthorDTO
            {
                FirstName = firstName,
                LastName = lastName,
            },
            ReleaseDate = releaseDate,
        };

        return new BookParsingResult
        {
            Book = bookDto,
            CoverFile = null,
        };
    }

    /// <inheritdoc/>
    public override Task<List<BookParsingResult>> ParseBulk(IFormFile file) => throw new NotImplementedException();
}
