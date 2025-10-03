// <copyright file="AmazonScraper.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text.Json;
using AutoMapper;
using HtmlAgilityPack;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic.Interfaces.WatchlistScraper;
using KapitelShelf.Data.Models;
using KapitelShelf.Data.Models.Watchlists;
using AmazonMetadataScraper = KapitelShelf.Api.Logic.MetadataScraper.AmazonScraper;

namespace KapitelShelf.Api.Logic.WatchlistScraper;

/// <summary>
/// The Amazon metadata scraper.
/// </summary>
public partial class AmazonScraper(HttpClient httpClient, IMapper mapper) : AmazonMetadataScraper(httpClient), IWatchlistScraper
{
    /// <summary>
    /// The batch size for fetching volumes.
    /// </summary>
    private const int BatchSize = 5;

    private readonly HttpClient httpClient = httpClient;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AmazonScraper"/> class.
    /// </summary>
    /// <param name="mapper">The auto mapper.</param>
    public AmazonScraper(IMapper mapper)
        : this(CreateHttpClient(), mapper)
    {
    }

    /// <inheritdoc />
    public async Task<List<WatchlistResultModel>> Scrape(SeriesDTO series)
    {
        ArgumentNullException.ThrowIfNull(series);

        if (series.LastVolume?.Location is null)
        {
            return [];
        }

        // Setup headers
        foreach (var header in Headers)
        {
            if (this.httpClient.DefaultRequestHeaders.Contains(header.Key))
            {
                this.httpClient.DefaultRequestHeaders.Remove(header.Key);
            }

            this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Scrape book page from amazon
        var bookPageUrl = $"https://www.amazon.com/dp/{series.LastVolume.Location.Url}?sr=8-1";

        using var bookPageResponse = await ScrapeRetryPolicy.ExecuteAsync(() => this.httpClient.GetAsync(bookPageUrl));
        bookPageResponse.EnsureSuccessStatusCode();

        var bookPageHtml = await bookPageResponse.Content.ReadAsStringAsync();
        var bookPageDocument = new HtmlDocument();
        bookPageDocument.LoadHtml(bookPageHtml);

        // extract series
        var seriesNode = bookPageDocument.DocumentNode.SelectSingleNode("//li[@data-tag-id='In This Series']");
        var asinsJson = seriesNode?.GetAttributeValue("data-asins", string.Empty);
        asinsJson = System.Net.WebUtility.HtmlDecode(asinsJson);
        if (asinsJson is null)
        {
            // book has no series
            return [];
        }

        // extract asins
        using var asinsDocument = JsonDocument.Parse(asinsJson);
        var asins = asinsDocument.RootElement
            .EnumerateArray()
            .Select(el => el.GetProperty("asin").GetString())
            .Where(a => !string.IsNullOrEmpty(a))
            .ToList();

        // Check if any books were found
        if (asins.Count == 0)
        {
            return [];
        }

        // Parse each book seperately
        // process ASINs in batches with delay
        var asinResults = new List<MetadataDTO?>();

        for (int i = 0; i < asins.Count; i += BatchSize)
        {
            var batch = asins
                .Skip(i)
                .Take(BatchSize)
                .Where(x => x != null)
                .Select(x => this.ParseBookLink($"/dp/{x!}"))
                .ToList();

            var batchResults = await Task.WhenAll(batch);
            asinResults.AddRange(batchResults);

            // add delay only if there are more batches left
            if (i + BatchSize < asins.Count)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        var watchlistResults = asinResults
            .Zip(asins, (dto, asin) => new
            {
                Dto = this.mapper.Map<WatchlistResultModel>(dto),
                Asin = asin!,
            })
            .Where(x => x != null);

        var results = new List<WatchlistResultModel>();
        foreach (var watchlistResult in watchlistResults)
        {
            var result = watchlistResult.Dto;
            result.SeriesId = series.Id;
            result.LocationType = this.mapper.Map<LocationType>(series.LastVolume.Location.Type);
            result.LocationUrl = watchlistResult.Asin;

            results.Add(result);
        }

        return results;
    }
}
