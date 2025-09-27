// <copyright file="SeriesWatchlistItemDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Watchlist;

/// <summary>
/// The series watchlist item dto.
/// </summary>
public class SeriesWatchlistItemDTO
{
    /// <summary>
    /// Gets or sets the series watchlist item id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the book.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description of the book.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the id of the series.
    /// </summary>
    public Guid SeriesId { get; set; }

    /// <summary>
    /// Gets or sets the number of the book in the series.
    /// </summary>
    public int? Volume { get; set; }

    /// <summary>
    /// Gets or sets the authors of the book.
    /// </summary>
    public List<string> Authors { get; set; } = [];

    /// <summary>
    /// Gets or sets the release date of the book.
    /// </summary>
    public string? ReleaseDate { get; set; }

    /// <summary>
    /// Gets or sets the number of pages of the book.
    /// </summary>
    public int? Pages { get; set; }

    /// <summary>
    /// Gets or sets the cover url of the book.
    /// </summary>
    public string? CoverUrl { get; set; }

    /// <summary>
    /// Gets or sets the categories of the book.
    /// </summary>
    public List<string> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets the tags of the book.
    /// </summary>
    public List<string> Tags { get; set; } = [];
}
