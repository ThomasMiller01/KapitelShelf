// <copyright file="BooksQueryExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Extensions;

/// <summary>
/// Extension methods for books queries.
/// </summary>
public static class BooksQueryExtensions
{
    /// <summary>
    /// Apply sorting to the books.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="sortBy">Sort the books by this field.</param>
    /// <param name="sortDir">Sort the books in this direction.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<BookModel> ApplySorting(this IQueryable<BookModel> query, BookSortByDTO sortBy, SortDirectionDTO sortDir)
    {
        var desc = sortDir == SortDirectionDTO.Desc;

        return (sortBy, desc) switch
        {
            // Title
            (BookSortByDTO.Title, false) =>
                query.OrderBy(x => x.Title)
                    .ThenBy(x => x.UpdatedAt),

            (BookSortByDTO.Title, true) =>
                query.OrderByDescending(x => x.Title)
                    .ThenByDescending(x => x.UpdatedAt),

            // Author
            (BookSortByDTO.Author, false) =>
                query.OrderBy(x => x.Author!.LastName)
                     .ThenBy(x => x.Author!.FirstName)
                     .ThenBy(x => x.UpdatedAt),

            (BookSortByDTO.Author, true) =>
                query.OrderByDescending(x => x.Author!.LastName)
                     .ThenByDescending(x => x.Author!.FirstName)
                     .ThenByDescending(x => x.UpdatedAt),

            // Series
            (BookSortByDTO.Series, false) =>
                query.OrderBy(x => x.Series!.Name)
                     .ThenBy(x => x.SeriesNumber)
                     .ThenBy(x => x.UpdatedAt),

            (BookSortByDTO.Series, true) =>
                query.OrderByDescending(x => x.Series!.Name)
                     .ThenByDescending(x => x.SeriesNumber)
                     .ThenByDescending(x => x.UpdatedAt),

            // Volume
            (BookSortByDTO.Volume, false) =>
                query.OrderBy(x => x.SeriesNumber)
                    .ThenBy(x => x.UpdatedAt),

            (BookSortByDTO.Volume, true) =>
                query.OrderByDescending(x => x.SeriesNumber)
                    .ThenByDescending(x => x.UpdatedAt),

            // Pages
            (BookSortByDTO.Pages, false) =>
                query.OrderBy(x => x.PageNumber)
                    .ThenBy(x => x.UpdatedAt),

            (BookSortByDTO.Pages, true) =>
                query.OrderByDescending(x => x.PageNumber)
                    .ThenByDescending(x => x.UpdatedAt),

            // Release
            (BookSortByDTO.Release, false) =>
                query.OrderBy(x => x.ReleaseDate)
                    .ThenBy(x => x.UpdatedAt),

            (BookSortByDTO.Release, true) =>
                query.OrderByDescending(x => x.ReleaseDate)
                    .ThenByDescending(x => x.UpdatedAt),

            // Default
            (BookSortByDTO.Default, false) =>
                query.OrderBy(x => x.UpdatedAt),

            (BookSortByDTO.Default, true) =>
                query.OrderByDescending(x => x.UpdatedAt),

            _ => query.OrderBy(x => x.Title)
                    .ThenBy(x => x.UpdatedAt),
        };
    }
}
