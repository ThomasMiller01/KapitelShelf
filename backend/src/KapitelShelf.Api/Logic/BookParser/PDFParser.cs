// <copyright file="PDFParser.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using System.Runtime.CompilerServices;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Api.Extensions;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic.BookParser;

/// <summary>
/// Parse the metadata of .pdf book files.
/// </summary>
public class PDFParser : BookParserBase
{
    /// <inheritdoc/>
    public override IReadOnlyCollection<string> SupportedExtensions { get; } = ["pdf"];

    /// <inheritdoc/>
    public override async Task<BookParsingResult> Parse(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        // read into memory so we can rewind if needed
        await using var ms = new MemoryStream();
        await file.OpenReadStream().CopyToAsync(ms);
        ms.Position = 0;

        // open PDF
        var reader = new PdfReader(ms);
        using var pdf = new PdfDocument(reader);
        var info = pdf.GetDocumentInfo();

        // title
        var titleInfo = info.GetTitle();
        var title = string.IsNullOrEmpty(titleInfo) ? this.ParseTitleFromFile(file.FileName) : this.ParseTitle(titleInfo);

        // description: try Subject, then Keywords
        var description = SanitizeText(info.GetSubject() ?? info.GetMoreInfo("Keywords"));

        // page count
        var pageNumber = pdf.GetNumberOfPages();

        // release date
        var releaseDate = ParseReleaseDate(info);

        // author
        var rawAuthor = info.GetAuthor();
        var (firstName, lastName) = ParseAuthor(rawAuthor);

        // cover
        var coverFile = ParseCover(pdf, title);

        var bookDto = new BookDTO
        {
            Title = title,
            Description = description,
            ReleaseDate = releaseDate,
            PageNumber = pageNumber,
            Series = null,
            Author = new AuthorDTO { FirstName = firstName, LastName = lastName },
            Categories = Array.Empty<CategoryDTO>().ToList(),
            Tags = Array.Empty<TagDTO>().ToList(),
        };

        return new BookParsingResult
        {
            Book = bookDto,
            CoverFile = coverFile,
        };
    }

    /// <inheritdoc/>
    public override Task<List<BookParsingResult>> ParseBulk(IFormFile file) => throw new NotImplementedException();

    internal static DateTime? ParseReleaseDate(PdfDocumentInfo info)
    {
        // PDF dates are often stored as "D:YYYYMMDDHHmmSS"
        var raw = info.GetMoreInfo("CreationDate") ?? info.GetMoreInfo("ModDate");
        if (string.IsNullOrEmpty(raw))
        {
            return null;
        }

        // strip leading "D:" if present
        if (raw.StartsWith("D:", StringComparison.OrdinalIgnoreCase))
        {
            raw = raw[2..];
        }

        // try parse the first 14 digits
        if (DateTime.TryParseExact(
                raw.Length >= 14 ? raw[..14] : raw,
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var dt))
        {
            return dt.ToUniversalTime();
        }

        return null;
    }

    internal static IFormFile? ParseCover(PdfDocument pdf, string title)
    {
        // get cover from the first page of the pdf
        var first_page = pdf.GetPage(1);
        var xobjects = first_page
            .GetResources()
            .GetResource(PdfName.XObject);

        if (xobjects is null)
        {
            return null;
        }

        foreach (var entry in xobjects.EntrySet())
        {
            var xobj = entry.Value;
            if (xobj is PdfStream stream && PdfName.Image.Equals(stream.GetAsName(PdfName.Subtype)))
            {
                // wrap it in an image XObject
                var imgObj = new PdfImageXObject(stream);
                byte[] imgBytes = imgObj.GetImageBytes(true);

                return imgBytes.ToFile($"{title}.png");
            }
        }

        return null;
    }
}
