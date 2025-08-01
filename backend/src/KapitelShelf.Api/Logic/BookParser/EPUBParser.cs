﻿// <copyright file="EPUBParser.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using System.Runtime.CompilerServices;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Api.Extensions;
using VersOne.Epub;
using VersOne.Epub.Schema;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic.BookParser;

/// <summary>
/// Parse the metadata of .epub book files.
/// </summary>
public class EPUBParser : BookParserBase
{
#pragma warning disable SA1401 // Fields should be private
    internal IEpubBookLoader BookLoader = new EpubBookLoader();
#pragma warning restore SA1401 // Fields should be private

    private static readonly string CalibreTimestampMetaName = "calibre:timestamp";

    private static readonly string CalibreSeriesNameMetaName = "calibre:series";

    private static readonly string CalibreSeriesIndexMetaName = "calibre:series_index";

    /// <inheritdoc/>
    public override IReadOnlyCollection<string> SupportedExtensions { get; } = ["epub"];

    /// <inheritdoc/>
    public override async Task<BookParsingResult> Parse(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        // load the epub
        using var stream = file.OpenReadStream();
        var epubBook = await this.BookLoader.OpenBookAsync(stream);

        var metadata = epubBook.Schema.Package.Metadata;

        // title
        var title = this.ParseTitle((string.IsNullOrEmpty(epubBook.Title)
            ? metadata.Titles.FirstOrDefault()?.Title
            : epubBook.Title) ?? string.Empty);

        // description
        var description = this.SanitizeText((string.IsNullOrEmpty(epubBook.Description)
            ? metadata.Descriptions.FirstOrDefault()?.Description
            : epubBook.Description) ?? string.Empty);

        // page number
        // TODO: https://github.com/ThomasMiller01/KapitelShelf/issues/123
        int? pageNumber = null;

        // release date
        var releaseDate = ParseReleaseDate(metadata);

        // author
        // TODO: https://github.com/ThomasMiller01/KapitelShelf/issues/121
        var author = epubBook.AuthorList.FirstOrDefault();
        var (firstName, lastName) = this.ParseAuthor(author ?? string.Empty);

        // series
        var (seriesName, seriesNumber) = ParseSeries(metadata);
        SeriesDTO? series = null;
        if (seriesName is not null)
        {
            series = new SeriesDTO
            {
                Name = seriesName,
            };
        }

        // cover
        var coverBytes = await epubBook.ReadCoverAsync();
        var coverFile = coverBytes?.ToFile($"{title}.png");

        var bookDto = new BookDTO
        {
            Title = title,
            Description = description,
            ReleaseDate = releaseDate,
            PageNumber = pageNumber,
            Series = series,
            SeriesNumber = seriesNumber,
            Author = new AuthorDTO
            {
                FirstName = firstName,
                LastName = lastName,
            },
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

    internal static DateTime? ParseReleaseDate(EpubMetadata? metadata)
    {
        if (metadata is null)
        {
            return null;
        }

        string? dateString = null;

        var releaseDate = metadata.Dates.FirstOrDefault()?.Date;
        if (releaseDate is not null and not "0101-01-01T00:00:00+00:00")
        {
            dateString = releaseDate;
        }

        var metaItem = metadata.MetaItems.FirstOrDefault(x => x.Name == CalibreTimestampMetaName);
        if (dateString is null && metaItem is not null)
        {
            dateString = metaItem.Content;
        }

        // Try year-only format first
        if (DateTime.TryParseExact(dateString, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var yearDate))
        {
            // Return Jan 1 of that year in UTC
            return DateTime.SpecifyKind(yearDate, DateTimeKind.Utc);
        }

        // Fallback to general parsing (ISO, etc.)
        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDate))
        {
            return parsedDate.ToUniversalTime();
        }

        // no release date found
        return null;
    }

    internal static (string? seriesName, int seriesNumber) ParseSeries(EpubMetadata metadata)
    {
        string? seriesName = null;
        var seriesNumber = 0;

        var nameMetaItem = metadata.MetaItems.FirstOrDefault(x => x.Name == CalibreSeriesNameMetaName);
        if (nameMetaItem is not null)
        {
            seriesName = nameMetaItem.Content;
        }

        var indexMetaItem = metadata.MetaItems.FirstOrDefault(x => x.Name == CalibreSeriesIndexMetaName);
        if (indexMetaItem is not null && float.TryParse(indexMetaItem.Content, CultureInfo.InvariantCulture, out var parsedSeriesNumber))
        {
            // parse as float, but then convert to int
            // series is often set as a float string, e.g. "36.0"
            // also parse as InvariantCulture to treat "." (dot) as a decimal point
            seriesNumber = (int)parsedSeriesNumber;
        }

        return (seriesName, seriesNumber);
    }
}

/// <summary>
/// Interface for loading an EPUB book.
/// </summary>
public interface IEpubBookLoader
{
    /// <summary>
    /// Open the EPUB book from the given stream.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <returns>The read epub.</returns>
    Task<EpubBookRef> OpenBookAsync(Stream stream);
}

/// <summary>
/// Implementation of the EPUB book loader.
/// This is necessary to make the EPUBParser testable.
/// </summary>
public class EpubBookLoader : IEpubBookLoader
{
    /// <inheritdoc/>
    public Task<EpubBookRef> OpenBookAsync(Stream stream) => EpubReader.OpenBookAsync(stream);
}
