// <copyright file="SeriesQueryExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Extensions;

/// <summary>
/// Extension methods for series queries.
/// </summary>
public static class SeriesQueryExtensions
{
    /// <summary>
    /// Apply sorting to the series.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="sortBy">Sort the series by this field.</param>
    /// <param name="sortDir">Sort the series in this direction.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<SeriesModel> ApplySorting(this IQueryable<SeriesModel> query, SeriesSortByDTO sortBy, SortDirectionDTO sortDir)
    {
        return (sortBy, sortDir) switch
        {
            // Name
            (SeriesSortByDTO.Name, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.Name)
                    .ThenBy(x => x.UpdatedAt),

            (SeriesSortByDTO.Name, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.Name)
                    .ThenByDescending(x => x.UpdatedAt),

            // Rating
            (SeriesSortByDTO.Rating, SortDirectionDTO.Asc) =>
                query.OrderBy(x =>
                    x.Rating.HasValue ||
                    x.Books
                        .SelectMany(b => b.UserMetadata)
                        .Any(um => um.Rating.HasValue) ? 0 : 1) // books without rating always at the bottom

                    .ThenBy(x => x.Rating ?? x.Books
                        .SelectMany(y => y.UserMetadata)
                        .Where(y => y.Rating.HasValue)
                        .Average(y => y.Rating))
                     .ThenBy(x => x.UpdatedAt),

            (SeriesSortByDTO.Rating, SortDirectionDTO.Desc) =>
                query.OrderBy(x =>
                    x.Rating.HasValue ||
                    x.Books
                        .SelectMany(x => x.UserMetadata)
                        .Any(x => x.Rating.HasValue) ? 0 : 1) // books without rating always at the bottom

                    .ThenByDescending(x => x.Rating ?? x.Books
                        .SelectMany(y => y.UserMetadata)
                        .Where(y => y.Rating.HasValue)
                        .Average(y => y.Rating))
                     .ThenBy(x => x.UpdatedAt),

            // Total Books
            (SeriesSortByDTO.TotalBooks, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.Books.Count())
                     .ThenBy(x => x.UpdatedAt),

            (SeriesSortByDTO.TotalBooks, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.Books.Count())
                     .ThenByDescending(x => x.UpdatedAt),

            // Updated
            (SeriesSortByDTO.Updated, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.UpdatedAt),

            (SeriesSortByDTO.Updated, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.UpdatedAt),

            // Created
            (SeriesSortByDTO.Created, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.CreatedAt)
                    .ThenBy(x => x.UpdatedAt),

            (SeriesSortByDTO.Created, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.UpdatedAt),

            // Default
            (SeriesSortByDTO.Default, SortDirectionDTO.Asc) =>
                query.ApplySorting(SeriesSortByDTO.Updated, SortDirectionDTO.Asc),

            (SeriesSortByDTO.Default, SortDirectionDTO.Desc) =>
                query.ApplySorting(SeriesSortByDTO.Updated, SortDirectionDTO.Desc),

            _ => query.ApplySorting(SeriesSortByDTO.Updated, SortDirectionDTO.Asc),
        };
    }
}
