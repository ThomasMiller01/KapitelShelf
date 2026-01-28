// <copyright file="TagsQueryExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Extensions;

/// <summary>
/// Extension methods for categories queries.
/// </summary>
public static class TagsQueryExtensions
{
    /// <summary>
    /// Apply sorting to the categories.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="sortBy">Sort the categories by this field.</param>
    /// <param name="sortDir">Sort the categories in this direction.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<TagModel> ApplySorting(this IQueryable<TagModel> query, TagSortByDTO sortBy, SortDirectionDTO sortDir)
    {
        return (sortBy, sortDir) switch
        {
            // Name
            (TagSortByDTO.Name, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.Name)
                    .ThenBy(x => x.UpdatedAt),

            (TagSortByDTO.Name, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.Name)
                    .ThenByDescending(x => x.UpdatedAt),

            // Total Books
            (TagSortByDTO.TotalBooks, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.Books.Count())
                     .ThenBy(x => x.UpdatedAt),

            (TagSortByDTO.TotalBooks, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.Books.Count())
                     .ThenByDescending(x => x.UpdatedAt),

            // Default
            (TagSortByDTO.Default, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.UpdatedAt),

            (TagSortByDTO.Default, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.UpdatedAt),

            _ => query.OrderBy(x => x.UpdatedAt),
        };
    }
}
