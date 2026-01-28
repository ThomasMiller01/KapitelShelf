// <copyright file="TagsController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Resources;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="TagsController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The tags logic.</param>
[ApiController]
[Route("tags")]
public class TagsController(ILogger<TagsController> logger, ITagsLogic logic) : ControllerBase
{
    private readonly ILogger<TagsController> logger = logger;

    private readonly ITagsLogic logic = logic;

    /// <summary>
    /// Fetch all tags.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="sortBy">Sort the tags by this field.</param>
    /// <param name="sortDir">Sort the tags in this direction.</param>
    /// <param name="filter">Filter the tags.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<TagDTO>>> GetTags(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        [FromQuery] TagSortByDTO sortBy = TagSortByDTO.Default,
        [FromQuery] SortDirectionDTO sortDir = SortDirectionDTO.Desc,
        [FromQuery] string? filter = null)
    {
        try
        {
            var series = await this.logic.GetTagsAsync(page, pageSize, sortBy, sortDir, filter);
            return Ok(series);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching tags");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Delete tags in bulk.
    /// </summary>
    /// <param name="tagIdsToDelete">The tags ids to delete.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpDelete]
    public async Task<IActionResult> DeleteBulk(List<Guid> tagIdsToDelete)
    {
        try
        {
            await this.logic.DeleteTagsAsync(tagIdsToDelete);
            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting tags with ids: {Ids}", tagIdsToDelete);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Autocomplete the book tag.
    /// </summary>
    /// <param name="partialTagName">The partial tag name.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("autocomplete")]
    public async Task<ActionResult<List<string>>> AutocompleteTag(string? partialTagName)
    {
        try
        {
            var autocompleteResult = await this.logic.AutocompleteAsync(partialTagName);
            return Ok(autocompleteResult);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting tag for autocomplete");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Update a tag.
    /// </summary>
    /// <param name="tagId">The id of the tag to update.</param>
    /// <param name="tag">The updated tag.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{tagId}")]
    public async Task<IActionResult> UpdateAuthor(Guid tagId, TagDTO tag)
    {
        try
        {
            var updatedAuthor = await this.logic.UpdateTagAsync(tagId, tag);
            if (updatedAuthor is null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message == StaticConstants.DuplicateExceptionKey)
        {
            return Conflict(new { error = "A tag with this name already exists." });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating tag with Id: {TagId}", tagId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Merge all source tags into the target tag.
    /// </summary>
    /// <param name="tagId">The target tag id.</param>
    /// <param name="sourceTagIds">The source tag ids.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{tagId}/merge")]
    public async Task<IActionResult> MergeCategoriesBulk(Guid tagId, List<Guid> sourceTagIds)
    {
        try
        {
            await this.logic.MergeTags(tagId, sourceTagIds);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error merging multiple tags into tag with Id: {TagId}", tagId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
