// <copyright file="IWatchlistScraper.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic.Interfaces.MetadataScraper;
using KapitelShelf.Data.Models.Watchlists;

namespace KapitelShelf.Api.Logic.Interfaces.WatchlistScraper;

/// <summary>
/// The inferface for watchlist scrapers.
/// </summary>
public interface IWatchlistScraper : IMetadataScraper
{
    /// <summary>
    /// Scrapes the next volume of a series.
    /// </summary>
    /// <param name="series">The series.</param>
    /// <returns>A task representing the asynchronous operation, containing the scraped volumes.</returns>
    Task<List<WatchlistResultModel>> Scrape(SeriesDTO series);
}
