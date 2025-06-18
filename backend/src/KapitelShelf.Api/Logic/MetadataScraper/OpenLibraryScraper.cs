// <copyright file="OpenLibraryScraper.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text.Json;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Extensions;

namespace KapitelShelf.Api.Logic.MetadataScraper;

/// <summary>
/// The OpenLibrary metadata scraper.
/// </summary>
public class OpenLibraryScraper(HttpClient httpClient) : MetadataScraperBase
{
    private readonly HttpClient httpClient = httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenLibraryScraper"/> class.
    /// </summary>
    public OpenLibraryScraper()
        : this(CreateHttpClient())
    {
    }

    /// <inheritdoc />
    public override async Task<List<MetadataDTO>> Scrape(string title)
    {
        var results = new List<MetadataDTO>();

        var searchUrl = $"https://openlibrary.org/search.json?title={Uri.EscapeDataString(title)}&limit={Limit}";
        using var response = await ScrapeRetryPolicy.ExecuteAsync(() => this.httpClient.GetAsync(searchUrl));
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var docs = document.RootElement.GetProperty("docs");
        foreach (var item in docs.EnumerateArray())
        {
            // Basic book data
            var metadata = new MetadataDTO
            {
                Title = item.GetPropertyOrDefault("title") ?? title,
                Authors = item.GetArrayOrDefault("author_name"),
                ReleaseDate = item.GetPropertyOrDefault("first_publish_year"),
                Pages = item.GetIntOrNull("number_of_pages_median"),
                Categories = item.GetArrayOrDefault("subject"),
                Series = item.GetFirstOrDefault("series"),
                Volume = item.GetIntOrNull("series_number"),
                CoverUrl = item.TryGetProperty("cover_i", out var coverId) ? $"https://covers.openlibrary.org/b/id/{coverId.GetInt32()}-L.jpg" : null,
            };

            // Try to get more details via /works API (description, tags, etc.)
            if (item.TryGetProperty("key", out var keyElem))
            {
                // e.g. "/works/OL262758W"
                var workKey = keyElem.GetString();
                if (!string.IsNullOrEmpty(workKey))
                {
                    var workUrl = $"https://openlibrary.org{workKey}.json";
                    try
                    {
                        var workResp = await ScrapeRetryPolicy.ExecuteAsync(() => this.httpClient.GetAsync(workUrl));
                        if (workResp.IsSuccessStatusCode)
                        {
                            using var workStream = await workResp.Content.ReadAsStreamAsync();
                            using var workDoc = await JsonDocument.ParseAsync(workStream);
                            var workRoot = workDoc.RootElement;
                            metadata.Description = workRoot.GetPropertyOrDefault("description");
                            metadata.Tags = workRoot.GetArrayOrDefault("subjects");
                        }
                    }
                    catch
                    {
                        // ignore detail fetch errors
                    }
                }
            }

            if (metadata.Series is null)
            {
                // try to get the series from the tags
                var seriesTag = metadata.Tags.FirstOrDefault(x => x.StartsWith("series:", StringComparison.InvariantCulture));
                if (seriesTag is not null)
                {
                    // extract series name after "series:" prefix
                    var trimmed = seriesTag["series:".Length..].Trim();
                    var replaced = trimmed.Replace("_", " ");

                    metadata.Series = replaced;
                }
            }

            // remove the series: tag
            metadata.Tags = metadata.Tags
                .Where(x => !x.StartsWith("series:", StringComparison.InvariantCulture))
                .ToList();

            results.Add(metadata);
        }

        return results;
    }
}
