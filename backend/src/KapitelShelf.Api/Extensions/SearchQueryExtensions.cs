// <copyright file="SearchQueryExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using DocumentFormat.OpenXml.Wordprocessing;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
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
    public static IQueryable<BookSearchView> FilterBySearchtermQuery(this IQueryable<BookSearchView> query, string? searchterm)
    {
        if (searchterm is null)
        {
            return query;
        }

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
    public static IQueryable<BookSearchView> SortBySearchtermQuery(this IQueryable<BookSearchView> query, string? searchterm)
    {
        if (searchterm is null)
        {
            return query;
        }

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
    /// Apply sorting to the books.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="sortBy">Sort the books by this field.</param>
    /// <param name="sortDir">Sort the books in this direction.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<BookSearchView> ApplySorting(this IQueryable<BookSearchView> query, BookSortByDTO sortBy, SortDirectionDTO sortDir)
    {
        return (sortBy, sortDir) switch
        {
            // Title
            (BookSortByDTO.Title, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.Title)
                    .ThenBy(x => x.BookModel!.UpdatedAt),

            (BookSortByDTO.Title, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.Title)
                    .ThenByDescending(x => x.BookModel!.UpdatedAt),

            // Author
            (BookSortByDTO.Author, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.AuthorNames)
                     .ThenBy(x => x.BookModel!.UpdatedAt),

            (BookSortByDTO.Author, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.AuthorNames)
                     .ThenByDescending(x => x.BookModel!.UpdatedAt),

            // Series
            (BookSortByDTO.Series, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.SeriesName)
                     .ThenBy(x => x.BookModel!.SeriesNumber)
                     .ThenBy(x => x.BookModel!.UpdatedAt),

            (BookSortByDTO.Series, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.SeriesName)
                     .ThenByDescending(x => x.BookModel!.SeriesNumber)
                     .ThenByDescending(x => x.BookModel!.UpdatedAt),

            // Volume
            (BookSortByDTO.Volume, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.BookModel!.SeriesNumber)
                    .ThenBy(x => x.BookModel!.UpdatedAt),

            (BookSortByDTO.Volume, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.BookModel!.SeriesNumber)
                    .ThenByDescending(x => x.BookModel!.UpdatedAt),

            // Pages
            (BookSortByDTO.Pages, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.BookModel!.PageNumber)
                    .ThenBy(x => x.BookModel!.UpdatedAt),

            (BookSortByDTO.Pages, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.BookModel!.PageNumber)
                    .ThenByDescending(x => x.BookModel!.UpdatedAt),

            // Release
            (BookSortByDTO.Release, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.BookModel!.ReleaseDate)
                    .ThenBy(x => x.BookModel!.UpdatedAt),

            (BookSortByDTO.Release, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.BookModel!.ReleaseDate)
                    .ThenByDescending(x => x.BookModel!.UpdatedAt),

            // Default
            (BookSortByDTO.Default, SortDirectionDTO.Asc) =>
                query.OrderBy(x => x.BookModel!.UpdatedAt),

            (BookSortByDTO.Default, SortDirectionDTO.Desc) =>
                query.OrderByDescending(x => x.BookModel!.UpdatedAt),

            _ => query.OrderBy(x => x.Title)
                    .ThenBy(x => x.BookModel!.UpdatedAt),
        };
    }

    /// <summary>
    /// Filter by series name query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="name">The series name.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<SeriesModel> FilterBySeriesNameQuery(this IQueryable<SeriesModel> query, string? name = null)
    {
        if (name is null)
        {
            return query;
        }

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
    public static IQueryable<SeriesModel> SortBySeriesNameQuery(this IQueryable<SeriesModel> query, string? name = null)
    {
        if (name is null)
        {
            return query;
        }

        return query.OrderByDescending(x =>

                // full-text search
                (EF.Functions.ToTsVector("english", x.Name).Rank(EF.Functions.PlainToTsQuery("english", name)) * 1.0f) +

                // partial matches
                // soft boost for substring matches
                (EF.Functions.ILike(x.Name, $"%{name}%") ? 0.1f : 0.0f) +

                // trigram fuzzy match
                ((float)PgTrgmExtensions.Similarity(x.Name, name) * 0.3f));
    }

    /// <summary>
    /// Filter by author query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="author">The author.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<AuthorModel> FilterByAuthorQuery(this IQueryable<AuthorModel> query, string? author)
    {
        if (author is null)
        {
            return query;
        }

        return query.Where(x =>

                // full-text search
                EF.Functions.ToTsVector("english", x.FirstName + " " + x.LastName).Matches(EF.Functions.PlainToTsQuery("english", author)) ||

                // partial matches
                EF.Functions.ILike(x.FirstName + " " + x.LastName, $"%{author}%") ||

                // trigram fuzzy match
                PgTrgmExtensions.Similarity(x.FirstName + " " + x.LastName, author) > 0.2);
    }

    /// <summary>
    /// Sort by author query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="author">The author.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<AuthorModel> SortByAuthorQuery(this IQueryable<AuthorModel> query, string? author)
    {
        if (author is null)
        {
            return query;
        }

        return query.OrderByDescending(x =>

                // full-text search
                (EF.Functions.ToTsVector("english", x.FirstName + " " + x.LastName).Rank(EF.Functions.PlainToTsQuery("english", author)) * 1.0f) +

                // partial matches
                // soft boost for substring matches
                (EF.Functions.ILike(x.FirstName + " " + x.LastName, $"%{author}%") ? 0.1f : 0.0f) +

                // trigram fuzzy match
                ((float)PgTrgmExtensions.Similarity(x.FirstName + " " + x.LastName, author) * 0.3f));
    }

    /// <summary>
    /// Filter by category query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="name">The category name.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<CategoryModel> FilterByCategoryQuery(this IQueryable<CategoryModel> query, string? name)
    {
        if (name is null)
        {
            return query;
        }

        return query.Where(x =>

                // full-text search
                EF.Functions.ToTsVector("english", x.Name).Matches(EF.Functions.PlainToTsQuery("english", name)) ||

                // partial matches
                EF.Functions.ILike(x.Name, $"%{name}%") ||

                // trigram fuzzy match
                PgTrgmExtensions.Similarity(x.Name, name) > 0.2);
    }

    /// <summary>
    /// Sort by category query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="name">The category name.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<CategoryModel> SortByCategoryQuery(this IQueryable<CategoryModel> query, string? name)
    {
        if (name is null)
        {
            return query;
        }

        return query.OrderByDescending(x =>

                // full-text search
                (EF.Functions.ToTsVector("english", x.Name).Rank(EF.Functions.PlainToTsQuery("english", name)) * 1.0f) +

                // partial matches
                // soft boost for substring matches
                (EF.Functions.ILike(x.Name, $"%{name}%") ? 0.1f : 0.0f) +

                // trigram fuzzy match
                ((float)PgTrgmExtensions.Similarity(x.Name, name) * 0.3f));
    }

    /// <summary>
    /// Filter by tag query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="name">The tag name.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<TagModel> FilterByTagQuery(this IQueryable<TagModel> query, string? name)
    {
        if (name is null)
        {
            return query;
        }

        return query.Where(x =>

                // full-text search
                EF.Functions.ToTsVector("english", x.Name).Matches(EF.Functions.PlainToTsQuery("english", name)) ||

                // partial matches
                EF.Functions.ILike(x.Name, $"%{name}%") ||

                // trigram fuzzy match
                PgTrgmExtensions.Similarity(x.Name, name) > 0.2);
    }

    /// <summary>
    /// Sort by tag query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="name">The tag name.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<TagModel> SortByTagQuery(this IQueryable<TagModel> query, string? name)
    {
        if (name is null)
        {
            return query;
        }

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
