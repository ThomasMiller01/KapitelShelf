// <copyright file="AuthorsQueryExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Extensions;

/// <summary>
/// Extension methods for authors queries.
/// </summary>
public static class AuthorsQueryExtensions
{
    /// <summary>
    /// Apply sorting to the books.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="sortBy">Sort the books by this field.</param>
    /// <param name="sortDir">Sort the books in this direction.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<AuthorModel> ApplySorting(this IQueryable<AuthorModel> query, AuthorSortByDTO sortBy, SortDirectionDTO sortDir)
    {
        return (sortBy, sortDir) switch
        {
            // First Name
            (AuthorSortByDTO.FirstName, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.FirstName)
                    .ThenBy(x => x.UpdatedAt),

            (AuthorSortByDTO.FirstName, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.FirstName)
                    .ThenByDescending(x => x.UpdatedAt),

            // Last Name
            (AuthorSortByDTO.LastName, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.LastName)
                    .ThenBy(x => x.UpdatedAt),

            (AuthorSortByDTO.LastName, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.LastName)
                    .ThenByDescending(x => x.UpdatedAt),

            // Default
            (AuthorSortByDTO.Default, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.UpdatedAt),

            (AuthorSortByDTO.Default, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.UpdatedAt),

            _ => query.OrderBy(x => x.UpdatedAt),
        };
    }
}
