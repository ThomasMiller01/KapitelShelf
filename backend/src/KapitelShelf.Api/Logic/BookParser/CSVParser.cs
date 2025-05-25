// <copyright file="CSVParser.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Tag;

namespace KapitelShelf.Api.Logic.BookParser;

/// <summary>
/// Import books from .csv files.
/// </summary>
public class CSVParser : BookParserBase
{
    private static readonly CsvConfiguration CSVConfig = new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        MissingFieldFound = null, // Ignore missing fields
        BadDataFound = null, // Ignore bad data
        TrimOptions = TrimOptions.Trim, // Trim whitespace from fields
    };

    /// <inheritdoc/>
    public override IReadOnlyCollection<string> SupportedExtensions { get; } = ["csv"];

    /// <inheritdoc/>
    public override Task<BookParsingResult> Parse(IFormFile file) => throw new NotImplementedException();

    /// <inheritdoc/>
    /// <summary>
    /// Parses a CSV file into multiple BookParsingResult objects.
    /// </summary>
    /// <param name="file">The CSV file to parse.</param>
    /// <returns>List of BookParsingResult.</returns>
    public override async Task<List<BookParsingResult>> ParseBulk(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        using var ms = new MemoryStream();
        await file.OpenReadStream().CopyToAsync(ms);
        ms.Position = 0;

        using var reader = new StreamReader(ms);
        using var csv = new CsvReader(reader, CSVConfig);

        var results = new List<BookParsingResult>();
        await foreach (var record in csv.GetRecordsAsync<CsvBookRow>())
        {
            var title = this.ParseTitle(record.Title);
            if (string.IsNullOrEmpty(title))
            {
                // Title is required, skip this record
                continue;
            }

            var description = this.SanitizeText(record.Description ?? string.Empty);
            var (firstName, lastName) = this.ParseAuthor(record.Author ?? string.Empty);

            var book = new BookDTO
            {
                Title = title,
                Description = description,
                ReleaseDate = ParseReleaseDate(record.ReleaseDate),
                PageNumber = ParseInt(record.Pages),
                Series = string.IsNullOrWhiteSpace(record.SeriesName) ? null : new SeriesDTO
                {
                    Name = record.SeriesName,
                },
                SeriesNumber = ParseInt(record.SeriesNumber, 0)!.Value,
                Author = new AuthorDTO
                {
                    FirstName = firstName,
                    LastName = lastName,
                },
                Tags = ParseTags(record.Tags),
                Categories = ParseCategories(record.Categories),
                Location = ParseLocation(record.LocationType, record.LocationValue),
            };

            results.Add(new BookParsingResult { Book = book });
        }

        return results;
    }

    private static DateTime? ParseReleaseDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            return null;
        }

        if (DateTime.TryParse(date, CultureInfo.InvariantCulture, out var dt))
        {
            return dt.ToUniversalTime();
        }

        return null;
    }

    private static int? ParseInt(string? val, int? @default = null) => int.TryParse(val, out var result) ? result : @default;

    private static IList<TagDTO> ParseTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return [];
        }

        return tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => new TagDTO { Name = t })
            .ToList();
    }

    private static IList<CategoryDTO> ParseCategories(string? categories)
    {
        if (string.IsNullOrWhiteSpace(categories))
        {
            return [];
        }

        return categories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(c => new CategoryDTO { Name = c })
            .ToList();
    }

    private static LocationDTO? ParseLocation(string? locationType, string? locationValue)
    {
        if (string.IsNullOrWhiteSpace(locationType))
        {
            return null;
        }

        var parsedType = Enum.TryParse<LocationTypeDTO>(
            locationType,
            ignoreCase: true,
            out var typeEnum) ? typeEnum : LocationTypeDTO.KapitelShelf;

        // adapt this based on your LocationDTO implementation
        return new LocationDTO
        {
            Type = parsedType,
            Url = string.IsNullOrWhiteSpace(locationValue) ? null : locationValue.Trim(),
        };
    }

    /// <summary>
    /// Maps the CSV row to the expected fields.
    /// </summary>
    private sealed class CsvBookRow
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Author { get; set; }
        public string? ReleaseDate { get; set; }
        public string? SeriesName { get; set; }
        public string? SeriesNumber { get; set; }
        public string? Pages { get; set; }
        public string? Tags { get; set; }
        public string? Categories { get; set; }
        public string? LocationType { get; set; }
        public string? LocationValue { get; set; }
    }
}
