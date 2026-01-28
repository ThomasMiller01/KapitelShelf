// <copyright file="CategoriesController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Resources;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="CategoriesController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The categories logic.</param>
[ApiController]
[Route("categories")]
public class CategoriesController(ILogger<CategoriesController> logger, ICategoriesLogic logic) : ControllerBase
{
    private readonly ILogger<CategoriesController> logger = logger;

    private readonly ICategoriesLogic logic = logic;

    /// <summary>
    /// Fetch all categories.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="sortBy">Sort the categories by this field.</param>
    /// <param name="sortDir">Sort the categories in this direction.</param>
    /// <param name="filter">Filter the categories.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<CategoryDTO>>> GetCategories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        [FromQuery] CategorySortByDTO sortBy = CategorySortByDTO.Default,
        [FromQuery] SortDirectionDTO sortDir = SortDirectionDTO.Desc,
        [FromQuery] string? filter = null)
    {
        try
        {
            var series = await this.logic.GetCategoriesAsync(page, pageSize, sortBy, sortDir, filter);
            return Ok(series);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching categories");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Delete categories in bulk.
    /// </summary>
    /// <param name="categoryIdsToDelete">The categories ids to delete.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpDelete]
    public async Task<IActionResult> DeleteBulk(List<Guid> categoryIdsToDelete)
    {
        try
        {
            await this.logic.DeleteCategoriesAsync(categoryIdsToDelete);
            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting categories with ids: {Ids}", categoryIdsToDelete);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Autocomplete the book category.
    /// </summary>
    /// <param name="partialCategoryName">The partial category name.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("autocomplete")]
    public async Task<ActionResult<List<string>>> AutocompleteCategory(string? partialCategoryName)
    {
        try
        {
            var autocompleteResult = await this.logic.AutocompleteAsync(partialCategoryName);
            return Ok(autocompleteResult);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting category for autocomplete");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Update a category.
    /// </summary>
    /// <param name="categoryId">The id of the category to update.</param>
    /// <param name="category">The updated category.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{categoryId}")]
    public async Task<IActionResult> UpdateCategory(Guid categoryId, CategoryDTO category)
    {
        try
        {
            var updatedCategory = await this.logic.UpdateCategoryAsync(categoryId, category);
            if (updatedCategory is null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message == StaticConstants.DuplicateExceptionKey)
        {
            return Conflict(new { error = "A category with this name already exists." });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating category with Id: {CategoryId}", categoryId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Merge all source categories into the target category.
    /// </summary>
    /// <param name="categoryId">The target category id.</param>
    /// <param name="sourceCategoryIds">The source category ids.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{categoryId}/merge")]
    public async Task<IActionResult> MergeCategoriesBulk(Guid categoryId, List<Guid> sourceCategoryIds)
    {
        try
        {
            await this.logic.MergeCategories(categoryId, sourceCategoryIds);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error merging multiple categories into category with Id: {CategoryId}", categoryId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
