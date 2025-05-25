// <copyright file="DocParser.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.DTOs.Category;
using NPOI.HWPF;

namespace KapitelShelf.Api.Logic.BookParser;

/// <summary>
/// Parse the metadata of .doc book files.
/// </summary>
public class DocParser : BookParserBase
{
    /// <inheritdoc/>
    public override IReadOnlyCollection<string> SupportedExtensions { get; } = ["doc"];

    /// <inheritdoc/>
    public override async Task<BookParsingResult> Parse(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        using var ms = new MemoryStream();
        await file.OpenReadStream().CopyToAsync(ms);
        ms.Position = 0;

        var hwpf = new HWPFDocument(ms);
        var info = hwpf.SummaryInformation;
        var docInfo = hwpf.DocumentSummaryInformation;

        // title
        var title = string.IsNullOrEmpty(info.Title) ? this.ParseTitleFromFile(file.FileName) : info.Title;

        // description
        var description = info.Subject ?? string.Empty;

        // author
        var (firstName, lastName) = this.ParseAuthor(info.Author ?? string.Empty);

        // releaseDate
        var releaseDate = info.CreateDateTime?.ToUniversalTime();

        // pageNumber
        var pageNumber = info.PageCount;

        // categories
        var categories = !string.IsNullOrWhiteSpace(docInfo.Category)
            ? [new CategoryDTO() { Name = docInfo.Category }]
            : Array.Empty<CategoryDTO>().ToList();

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
            PageNumber = pageNumber,
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
