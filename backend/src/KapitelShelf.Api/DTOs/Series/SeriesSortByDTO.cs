// <copyright file="SeriesSortByDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Series;

/// <summary>
/// The sort by enum for series.
/// </summary>
public enum SeriesSortByDTO
{
    /// <summary>
    /// Default sorting order for series.
    /// </summary>
    Default,

    /// <summary>
    /// Sort by the series name.
    /// </summary>
    Name,

    /// <summary>
    /// Sort by the series rating.
    /// </summary>
    Rating,

    /// <summary>
    /// Sort by the series total books.
    /// </summary>
    TotalBooks,

    /// <summary>
    /// Sort by the series updated time.
    /// </summary>
    Updated,

    /// <summary>
    /// Sort by the series created time.
    /// </summary>
    Created,
}
