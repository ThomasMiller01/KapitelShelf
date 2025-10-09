// <copyright file="AmazonScraper.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;
using HtmlAgilityPack;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic.Interfaces.WatchlistScraper;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models.Watchlists;
using Quartz.Util;
using AmazonMetadataScraper = KapitelShelf.Api.Logic.MetadataScraper.AmazonScraper;

namespace KapitelShelf.Api.Logic.WatchlistScraper;

/// <summary>
/// The Amazon metadata scraper.
/// </summary>
public partial class AmazonScraper(HttpClient httpClient, Mapper mapper) : AmazonMetadataScraper(httpClient), IWatchlistScraper
{
    /// <summary>
    /// The batch size for fetching volumes.
    /// </summary>
    private const int BatchSize = 5;

    private readonly HttpClient httpClient = httpClient;

    private readonly Mapper mapper = mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AmazonScraper"/> class.
    /// </summary>
    /// <param name="mapper">The mapper.</param>
    public AmazonScraper(Mapper mapper)
        : this(CreateHttpClient(), mapper)
    {
    }

    /// <inheritdoc />
    public async Task<List<WatchlistResultModel>> Scrape(SeriesDTO series)
    {
        ArgumentNullException.ThrowIfNull(series);

        if (series.LastVolume?.Location is null || series.LastVolume.Location.Url is null)
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

        var seriesASIN = await this.GetSeriesASIN(series.LastVolume.Location.Url);
        if (seriesASIN.IsNullOrWhiteSpace())
        {
            // book is not part of a series
            return [];
        }

        var asins = await this.GetVolumeASINS(seriesASIN!);

        // only take the asins take are after the last volume in the library
        asins = asins
            .Skip(series.LastVolume.SeriesNumber)
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
                Dto = this.mapper.MetadataDtoToWatchlistResultModelNullable(dto),
                Asin = asin!,
            })
            .Where(x => x != null && x.Dto != null)
            .Select(x => new { Dto = x.Dto!, x.Asin });

        var results = new List<WatchlistResultModel>();
        foreach (var watchlistResult in watchlistResults)
        {
            var result = watchlistResult.Dto;
            result.SeriesId = series.Id;
            result.LocationType = this.mapper.LocationTypeDtoToLocationType(series.LastVolume.Location.Type);
            result.LocationUrl = watchlistResult.Asin;

            results.Add(result);
        }

        return results;
    }

    private async Task<string?> GetSeriesASIN(string bookASIN)
    {
        // Scrape book page from amazon
        var bookPageUrl = $"https://www.amazon.com/dp/{bookASIN}?sr=8-1";

        using var bookPageResponse = await ScrapeRetryPolicy.ExecuteAsync(() => this.httpClient.GetAsync(bookPageUrl));
        bookPageResponse.EnsureSuccessStatusCode();

        var bookPageHtml = await bookPageResponse.Content.ReadAsStringAsync();
        var bookPageDocument = new HtmlDocument();
        bookPageDocument.LoadHtml(bookPageHtml);

        var cardContextNode = bookPageDocument.DocumentNode.SelectSingleNode("//div[@id='cardContextDataContainer']");
        var seriesAsin = cardContextNode?.GetAttributeValue("data-series-asin", string.Empty);

        return seriesAsin;
    }

    private async Task<List<string>> GetVolumeASINS(string seriesASIN)
    {
        // Scrape book page from amazon
        var seriesPageUrl = $"https://www.amazon.com/dp/{seriesASIN}";

        using var seriesPageResponse = await ScrapeRetryPolicy.ExecuteAsync(() => this.httpClient.GetAsync(seriesPageUrl));
        seriesPageResponse.EnsureSuccessStatusCode();

        var seriesPageHtml = await seriesPageResponse.Content.ReadAsStringAsync();
        var seriesPageDocument = new HtmlDocument();
        seriesPageDocument.LoadHtml(seriesPageHtml);

        // extract asins
        var asinNodes = seriesPageDocument.DocumentNode.SelectNodes("//a[@class='a-size-medium a-link-normal itemBookTitle']");
        if (asinNodes is null)
        {
            // series has no volumes (for some reason)
            return [];
        }

        List<string> asins = [];
        foreach (var asinNode in asinNodes)
        {
            var url = asinNode.GetAttributeValue("href", string.Empty);
            if (url.IsNullOrWhiteSpace())
            {
                // could not get the url for this volume
                continue;
            }

            var asin = ExtractAsinFromUrl(url);
            if (asin.IsNullOrWhiteSpace())
            {
                // could not extract asin from url
                continue;
            }

            asins.Add(asin!);
        }

        return asins;
    }

    private static string? ExtractAsinFromUrl(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        var match = AsinInUrlRegex().Match(url);
        return match.Success ? match.Groups[1].Value : null;
    }

    [GeneratedRegex(@"(?:/dp/|/gp/product/)([A-Z0-9]{10})", RegexOptions.IgnoreCase)]
    private static partial Regex AsinInUrlRegex();
}
