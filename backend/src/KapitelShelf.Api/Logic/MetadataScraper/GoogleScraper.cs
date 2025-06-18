// <copyright file="GoogleScraper.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text.Json;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Extensions;

namespace KapitelShelf.Api.Logic.MetadataScraper;

/// <summary>
/// The Google metadata scraper.
/// </summary>
public class GoogleScraper(HttpClient httpClient) : MetadataScraperBase
{
    private readonly HttpClient httpClient = httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleScraper"/> class.
    /// </summary>
    public GoogleScraper()
        : this(CreateHttpClient())
    {
    }

    /// <inheritdoc />
    public override async Task<List<MetadataDTO>> Scrape(string title)
    {
        var results = new List<MetadataDTO>();

        var url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(title)}&maxResults={Limit}";
        using var response = await ScrapeRetryPolicy.ExecuteAsync(() => this.httpClient.GetAsync(url));
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        if (!document.RootElement.TryGetProperty("items", out var items))
        {
            return results;
        }

        foreach (var item in items.EnumerateArray())
        {
            if (!item.TryGetProperty("volumeInfo", out var info))
            {
                continue;
            }

            string? coverUrl = null;
            if (info.TryGetProperty("imageLinks", out var imgLinks) && imgLinks.TryGetProperty("thumbnail", out var thumbnail))
            {
                var httpCoverUrl = thumbnail.GetString();
                var httpsCoverUrl = httpCoverUrl?.Replace("http://", "https://", StringComparison.OrdinalIgnoreCase);

                coverUrl = httpCoverUrl;
            }

            var metadata = new MetadataDTO
            {
                Title = info.GetPropertyOrDefault("title") ?? title,
                Authors = info.GetArrayOrDefault("authors"),
                Description = info.GetPropertyOrDefault("description"),
                Series = null, // Google Books API doesn't directly expose series
                Volume = null,
                ReleaseDate = info.GetPropertyOrDefault("publishedDate"),
                Pages = info.GetIntOrNull("pageCount"),
                CoverUrl = coverUrl,
                Categories = info.GetArrayOrDefault("categories"),
                Tags = [],
            };

            results.Add(metadata);
        }

        return results;
    }
}
