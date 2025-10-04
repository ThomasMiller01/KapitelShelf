// <copyright file="IWatchlistLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Watchlist;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// The watchlist logic interface.
/// </summary>
public interface IWatchlistLogic
{
    /// <summary>
    /// Get the series watchlists for a user.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <returns>The list of watchlists.</returns>
    Task<List<SeriesWatchlistDTO>> GetWatchlistAsync(Guid userId);

    /// <summary>
    /// Check if the series is on the watchlist.
    /// </summary>
    /// <param name="seriesId">The id of the series.</param>
    /// <param name="userId">The id of the user.</param>
    /// <returns>true, if the series is on the watchlist, otherwise false.</returns>
    Task<bool> IsOnWatchlist(Guid seriesId, Guid userId);

    /// <summary>
    /// Add the series to the watchlist of a user.
    /// </summary>
    /// <param name="seriesId">The id of the series.</param>
    /// <param name="userId">The id of the user.</param>
    /// <returns>The new watchlist dto.</returns>
    Task<SeriesWatchlistDTO?> AddToWatchlist(Guid seriesId, Guid userId);

    /// <summary>
    /// Remove the series from the watchlist of a user.
    /// </summary>
    /// <param name="seriesId">The id of the series.</param>
    /// <param name="userId">The id of the user.</param>
    /// <returns>The new watchlist dto.</returns>
    Task<SeriesWatchlistDTO?> RemoveFromWatchlist(Guid seriesId, Guid userId);

    /// <summary>
    /// Update the watchlist and check for new volumes.
    /// </summary>
    /// <param name="watchlistId">The id of the watchlist.</param>
    /// <returns>A task.</returns>
    Task UpdateWatchlist(Guid watchlistId);
}
