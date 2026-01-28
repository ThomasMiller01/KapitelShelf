// <copyright file="ICategoriesLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Category;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// Interface for the categories logic.
/// </summary>
public interface ICategoriesLogic
{
    /// <summary>
    /// Get all categories.
    /// </summary>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <param name="sortBy">Sort the categories by this field.</param>
    /// <param name="sortDir">Sort the categories in this direction.</param>
    /// <param name="filter">Filter the categories.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<PagedResult<CategoryDTO>> GetCategoriesAsync(int page, int pageSize, CategorySortByDTO sortBy, SortDirectionDTO sortDir, string? filter);

    /// <summary>
    /// Delete categories in bulk.
    /// </summary>
    /// <param name="categoryIdsToDelete">The categories ids to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteCategoriesAsync(List<Guid> categoryIdsToDelete);

    /// <summary>
    /// Get the autocomplete result for the categoriy.
    /// </summary>
    /// <param name="partialCategoryName">The partial category name.</param>
    /// <returns>The autocomplete result.</returns>
    Task<List<string>> AutocompleteAsync(string? partialCategoryName);

    /// <summary>
    /// Update an category.
    /// </summary>
    /// <param name="categoryId">The id of the category to update.</param>
    /// <param name="categoryDto">The updated category dto.</param>
    /// <returns>A <see cref="Task{CategoryDTO}"/> representing the result of the asynchronous operation.</returns>
    Task<CategoryDTO?> UpdateCategoryAsync(Guid categoryId, CategoryDTO categoryDto);

    /// <summary>
    /// Merge the source categories into the target category.
    /// </summary>
    /// <param name="targetCategoryId">The target category id.</param>
    /// <param name="sourceCategoryIds">The source categories ids.</param>
    /// <returns>A task.</returns>
    Task MergeCategories(Guid targetCategoryId, List<Guid> sourceCategoryIds);
}
