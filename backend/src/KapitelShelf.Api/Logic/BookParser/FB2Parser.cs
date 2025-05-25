// <copyright file="FB2Parser.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using System.Xml.Linq;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Api.Extensions;

namespace KapitelShelf.Api.Logic.BookParser;

/// <summary>
/// Parse the metadata of .pdf book files.
/// </summary>
public class FB2Parser : BookParserBase
{
    /// <inheritdoc/>
    public override IReadOnlyCollection<string> SupportedExtensions { get; } = ["fb2"];

    /// <inheritdoc/>
    public override async Task<BookParsingResult> Parse(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        // read into memory so we can rewind if needed
        await using var ms = new MemoryStream();
        await file.OpenReadStream().CopyToAsync(ms);
        ms.Position = 0;

        // open PDF
        var xml = XDocument.Load(ms);
        if (xml is null || xml?.Root is null)
        {
            throw new ArgumentNullException(nameof(xml), "Could not load .fb2 as XML");
        }

        var fb2 = xml!.Root!.GetDefaultNamespace();

        var titleInfo = xml?.Root?
            .Element(fb2 + "description")?
            .Element(fb2 + "title-info");

        var documentInfo = xml?.Root?
            .Element(fb2 + "description")?
            .Element(fb2 + "document-info");

        var publishInfo = xml?.Root?
            .Element(fb2 + "description")?
            .Element(fb2 + "publish-info");

        // title
        var titleElement = titleInfo?.Element(fb2 + "book-title")?.Value;
        var title = this.ParseTitle(titleElement ?? string.Empty);

        // description
        var annotation = titleInfo?.Element(fb2 + "annotation");
        var descriptionElements = annotation?
            .DescendantNodes()
            .OfType<XText>();
        var description = descriptionElements is not null ? this.SanitizeText(string.Concat(descriptionElements.Select(x => x.Value))) : string.Empty;

        // release date
        var releaseDate = ParseReleaseDate(titleInfo, documentInfo, fb2);

        // author
        var (firstName, lastName) = ParseAuthor(titleInfo, documentInfo, fb2);

        // series
        var (seriesName, seriesNumber) = ParseSeries(titleInfo, documentInfo, publishInfo, fb2);
        SeriesDTO? series = null;
        if (seriesName is not null)
        {
            series = new SeriesDTO
            {
                Name = seriesName,
            };
        }

        // categories
        var categories = titleInfo?
            .Elements(fb2 + "genre")
            .Select(x => new CategoryDTO { Name = x.Value })
            .ToList() ?? Array.Empty<CategoryDTO>().ToList();

        // cover
        var coverFile = ParseCover(xml!.Root!, fb2, title);

        var bookDto = new BookDTO
        {
            Title = title,
            Description = description,
            ReleaseDate = releaseDate,
            Series = series,
            Author = new AuthorDTO { FirstName = firstName, LastName = lastName },
            Categories = categories,
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

    private static DateTime? ParseReleaseDate(XElement? titleInfo, XElement? documentInfo, XNamespace fb2)
    {
        var dateValue = titleInfo?.Element(fb2 + "date")?.Attribute("value")
            ?? documentInfo?.Element(fb2 + "date")?.Attribute("value");

        if (dateValue is null)
        {
            return null;
        }

        if (DateTime.TryParse(dateValue.Value, CultureInfo.InvariantCulture, out var date))
        {
            return date.ToUniversalTime();
        }

        return null;
    }

    private static (string firstName, string lastName) ParseAuthor(XElement? titleInfo, XElement? documentInfo, XNamespace fb2)
    {
        var authorElement = titleInfo?.Element(fb2 + "author") ?? documentInfo?.Element(fb2 + "author");
        if (authorElement is null)
        {
            return (string.Empty, string.Empty);
        }

        var firstName = authorElement.Element(fb2 + "first-name")?.Value
            ?? authorElement.Element(fb2 + "nickname")?.Value
            ?? string.Empty;

        var lastName = authorElement.Element(fb2 + "last-name")?.Value ?? string.Empty;

        return (firstName, lastName);
    }

    private static (string? seriesName, int seriesNumber) ParseSeries(XElement? titleInfo, XElement? documentInfo, XElement? publishInfo, XNamespace fb2)
    {
        string? seriesName = null;
        int seriesNumber = 0;

        var sequenceElement = titleInfo?.Element(fb2 + "sequence")
            ?? documentInfo?.Element(fb2 + "sequence")
            ?? publishInfo?.Element(fb2 + "sequence");

        if (sequenceElement is null)
        {
            return (seriesName, seriesNumber);
        }

        seriesName = sequenceElement.Attribute("name")?.Value ?? string.Empty;

        var numberAttribute = sequenceElement.Attribute("number");
        if (float.TryParse(numberAttribute?.Value, CultureInfo.InvariantCulture, out var parsedNumber))
        {
            seriesNumber = (int)parsedNumber;
        }

        return (seriesName, seriesNumber);
    }

    private static IFormFile? ParseCover(XElement root, XNamespace fb2, string title)
    {
        var coverBinary = root
            .Elements(fb2 + "binary")
            .FirstOrDefault(x =>
            {
                var contentType = x.Attribute("content-type")?.Value;
                return contentType is not null && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            });

        if (coverBinary is null)
        {
            return null;
        }

        var coverBytes = Convert.FromBase64String(coverBinary.Value);
        return coverBytes.ToFile($"{title}.png");
    }
}
