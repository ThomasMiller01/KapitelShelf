// <copyright file="WatchlistScraperManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using AutoMapper;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Logic.Interfaces.WatchlistScraper;
using KapitelShelf.Api.Logic.WatchlistScraper;
using KapitelShelf.Data.Models.Watchlists;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Watchlist scraper factory.
/// </summary>
public class WatchlistScraperManager : IWatchlistScraperManager
{
    private readonly Dictionary<LocationTypeDTO, Type> watchlistScraper;

    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="WatchlistScraperManager"/> class.
    /// </summary>
    /// <param name="mapper">The auto mapper.</param>
    public WatchlistScraperManager(IMapper mapper)
        : this(
            new Dictionary<LocationTypeDTO, Type>
            {
                { LocationTypeDTO.Kindle, typeof(AmazonScraper) },
            }, mapper)
    {
    }

    // Needed for unit tests
    internal WatchlistScraperManager(Dictionary<LocationTypeDTO, Type> watchlistScraper, IMapper mapper)
    {
        this.watchlistScraper = watchlistScraper;
        this.mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<List<WatchlistResultModel>> Scrape(SeriesDTO series)
    {
        ArgumentNullException.ThrowIfNull(series);

        if (series.LastVolume?.Location is null)
        {
            throw new ArgumentException("'Series.LastVolume.Location' must be set");
        }

        // get new parser each time
        var scraper = this.GetNewScraper(series.LastVolume.Location.Type);

        return await scraper.Scrape(series);
    }

    /// <inheritdoc/>
    public IWatchlistScraper GetNewScraper(LocationTypeDTO locationType)
    {
        // get scraper for this source
        var scraperType = this.watchlistScraper
            .FirstOrDefault(x => x.Key == locationType).Value
            ?? throw new ArgumentException("Unsupported location type", locationType.ToString());

        // create new scraper foreach request
        var scraper = Activator.CreateInstance(scraperType, this.mapper) as IWatchlistScraper ?? throw new ArgumentException("Scraper must be set");
        return scraper;
    }
}
