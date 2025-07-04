﻿// <copyright file="SearchQueryExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Data.Extensions;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Extensions;

/// <summary>
/// Extension methods for search queries.
/// </summary>
public static class SearchQueryExtensions
{
    /// <summary>
    /// Filter by searchterm query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="searchterm">The searchterm.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<BookSearchView> FilterBySearchtermQuery(this IQueryable<BookSearchView> query, string searchterm)
    {
        return query.Where(x =>

                // full-text search
                x.SearchVector.Matches(EF.Functions.PlainToTsQuery("english", searchterm)) ||

                // partial matches
                EF.Functions.ILike(x.SearchText, $"%{searchterm}%") ||

                // trigram fuzzy match
                PgTrgmExtensions.Similarity(x.SearchText, searchterm) > 0.2);
    }

    /// <summary>
    /// Sort by searchterm query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="searchterm">The searchterm.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<BookSearchView> SortBySearchtermQuery(this IQueryable<BookSearchView> query, string searchterm)
    {
        return query.OrderByDescending(x =>

                // full-text search
                (x.SearchVector.Rank(EF.Functions.PlainToTsQuery("english", searchterm)) * 1.0f) +

                // partial matches

                // boost for title matches
                (EF.Functions.ILike(x.Title, $"%{searchterm}%") ? 0.5f : 0.0f) +

                // soft boost for substring matches
                (EF.Functions.ILike(x.SearchText, $"%{searchterm}%") ? 0.1f : 0.0f) +

                // trigram fuzzy match
                ((float)PgTrgmExtensions.Similarity(x.SearchText, searchterm) * 0.3f));
    }

    /// <summary>
    /// Filter by series name query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="name">The series name.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<SeriesModel> FilterBySeriesNameQuery(this IQueryable<SeriesModel> query, string name)
    {
        return query.Where(x =>

                // full-text search
                EF.Functions.ToTsVector("english", x.Name).Matches(EF.Functions.PlainToTsQuery("english", name)) ||

                // partial matches
                EF.Functions.ILike(x.Name, $"%{name}%") ||

                // trigram fuzzy match
                PgTrgmExtensions.Similarity(x.Name, name) > 0.2);
    }

    /// <summary>
    /// Sort by series name query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="name">The series name.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<SeriesModel> SortBySeriesNameQuery(this IQueryable<SeriesModel> query, string name)
    {
        return query.OrderByDescending(x =>

                // full-text search
                (EF.Functions.ToTsVector("english", x.Name).Rank(EF.Functions.PlainToTsQuery("english", name)) * 1.0f) +

                // partial matches
                // soft boost for substring matches
                (EF.Functions.ILike(x.Name, $"%{name}%") ? 0.1f : 0.0f) +

                // trigram fuzzy match
                ((float)PgTrgmExtensions.Similarity(x.Name, name) * 0.3f));
    }
}
