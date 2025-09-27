// <copyright file="SeriesWatchlistDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Series;

namespace KapitelShelf.Api.DTOs.Watchlist;

/// <summary>
/// The series watchlist dto.
/// </summary>
public class SeriesWatchlistDTO
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the series.
    /// </summary>
    public SeriesDTO Series { get; set; } = null!;

    /// <summary>
    /// Gets or sets the watchlist items for this series.
    /// </summary>
    public IList<BookDTO> Items { get; set; } = [];
}
