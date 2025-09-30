// <copyright file="IWatchlistScraperManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic.Interfaces.WatchlistScraper;
using KapitelShelf.Data.Models.Watchlists;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// Interface for a watchlist scraper factory.
/// </summary>
public interface IWatchlistScraperManager
{
    /// <summary>
    /// Scrape the volumes from a series from the watchlist.
    /// </summary>
    /// <param name="series">The series.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<List<WatchlistResultModel>> Scrape(SeriesDTO series);

    /// <summary>
    /// Gets the scraper for the given location type.
    /// </summary>
    /// <param name="locationType">The location type.</param>
    /// <returns>The watchlist scraper.</returns>
    /// <exception cref="ArgumentException">Scraper could not be found.</exception>
    IWatchlistScraper GetNewScraper(LocationTypeDTO locationType);
}
