// <copyright file="CategoriesQueryExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Extensions;

/// <summary>
/// Extension methods for categories queries.
/// </summary>
public static class CategoriesQueryExtensions
{
    /// <summary>
    /// Apply sorting to the categories.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="sortBy">Sort the categories by this field.</param>
    /// <param name="sortDir">Sort the categories in this direction.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<CategoryModel> ApplySorting(this IQueryable<CategoryModel> query, CategorySortByDTO sortBy, SortDirectionDTO sortDir)
    {
        return (sortBy, sortDir) switch
        {
            // Name
            (CategorySortByDTO.Name, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.Name)
                    .ThenBy(x => x.UpdatedAt),

            (CategorySortByDTO.Name, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.Name)
                    .ThenByDescending(x => x.UpdatedAt),

            // Total Books
            (CategorySortByDTO.TotalBooks, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.Books.Count())
                     .ThenBy(x => x.UpdatedAt),

            (CategorySortByDTO.TotalBooks, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.Books.Count())
                     .ThenByDescending(x => x.UpdatedAt),

            // Default
            (CategorySortByDTO.Default, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.UpdatedAt),

            (CategorySortByDTO.Default, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.UpdatedAt),

            _ => query.OrderBy(x => x.UpdatedAt),
        };
    }
}
