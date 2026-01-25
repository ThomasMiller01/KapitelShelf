// <copyright file="SeriesQueryExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Extensions;

/// <summary>
/// Extension methods for books queries.
/// </summary>
public static class SeriesQueryExtensions
{
    /// <summary>
    /// Apply sorting to the books.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="sortBy">Sort the books by this field.</param>
    /// <param name="sortDir">Sort the books in this direction.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<SeriesModel> ApplySorting(this IQueryable<SeriesModel> query, SeriesSortByDTO sortBy, SortDirectionDTO sortDir)
    {
        var desc = sortDir == SortDirectionDTO.Desc;

        return (sortBy, desc) switch
        {
            // Name
            (SeriesSortByDTO.Name, false) =>
                query.OrderBy(x => x.Name)
                    .ThenBy(x => x.UpdatedAt),

            (SeriesSortByDTO.Name, true) =>
                query.OrderByDescending(x => x.Name)
                    .ThenByDescending(x => x.UpdatedAt),

            // Total Books
            (SeriesSortByDTO.TotalBooks, false) =>
                query.OrderBy(x => x.Books.Count())
                     .ThenBy(x => x.UpdatedAt),

            (SeriesSortByDTO.TotalBooks, true) =>
                query.OrderByDescending(x => x.Books.Count())
                     .ThenByDescending(x => x.UpdatedAt),

            // Updated
            (SeriesSortByDTO.Updated, false) =>
                query.OrderBy(x => x.UpdatedAt),

            (SeriesSortByDTO.Updated, true) =>
                query.OrderByDescending(x => x.UpdatedAt),

            // Created
            (SeriesSortByDTO.Created, false) =>
                query.OrderBy(x => x.CreatedAt)
                    .ThenBy(x => x.UpdatedAt),

            (SeriesSortByDTO.Created, true) =>
                query.OrderByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.UpdatedAt),

            // Default
            (SeriesSortByDTO.Default, false) =>
                query.ApplySorting(SeriesSortByDTO.Updated, SortDirectionDTO.Asc),

            (SeriesSortByDTO.Default, true) =>
                query.ApplySorting(SeriesSortByDTO.Updated, SortDirectionDTO.Desc),

            _ => query.ApplySorting(SeriesSortByDTO.Updated, SortDirectionDTO.Asc),
        };
    }
}
