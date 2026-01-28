// <copyright file="IAuthorsLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Series;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// Interface for the authors logic.
/// </summary>
public interface IAuthorsLogic
{
    /// <summary>
    /// Get all authors.
    /// </summary>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <param name="sortBy">Sort the authors by this field.</param>
    /// <param name="sortDir">Sort the authors in this direction.</param>
    /// <param name="filter">Filter the authors.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<PagedResult<AuthorDTO>> GetAuthorsAsync(int page, int pageSize, AuthorSortByDTO sortBy, SortDirectionDTO sortDir, string? filter);

    /// <summary>
    /// Delete authors in bulk.
    /// </summary>
    /// <param name="authorIdsToDelete">The author ids to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAuthorsAsync(List<Guid> authorIdsToDelete);

    /// <summary>
    /// Get the autocomplete result for the author.
    /// </summary>
    /// <param name="partialAuthor">The partial author.</param>
    /// <returns>The autocomplete result.</returns>
    Task<List<string>> AutocompleteAsync(string? partialAuthor);

    /// <summary>
    /// Update an author.
    /// </summary>
    /// <param name="authorId">The id of the author to update.</param>
    /// <param name="authorDto">The updated author dto.</param>
    /// <returns>A <see cref="Task{AuthorDTO}"/> representing the result of the asynchronous operation.</returns>
    Task<AuthorDTO?> UpdateAuthorAsync(Guid authorId, AuthorDTO authorDto);

    /// <summary>
    /// Merge the source authors into the target authors.
    /// </summary>
    /// <param name="targetAuthorId">The target author id.</param>
    /// <param name="sourceAuthorsIds">The source authors ids.</param>
    /// <returns>A task.</returns>
    Task MergeAuthors(Guid targetAuthorId, List<Guid> sourceAuthorsIds);
}
