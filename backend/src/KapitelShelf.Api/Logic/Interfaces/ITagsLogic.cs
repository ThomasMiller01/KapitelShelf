// <copyright file="ITagsLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Tag;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// Interface for the tags logic.
/// </summary>
public interface ITagsLogic
{
    /// <summary>
    /// Get all tags.
    /// </summary>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <param name="sortBy">Sort the tags by this field.</param>
    /// <param name="sortDir">Sort the tags in this direction.</param>
    /// <param name="filter">Filter the tags.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<PagedResult<TagDTO>> GetTagsAsync(int page, int pageSize, TagSortByDTO sortBy, SortDirectionDTO sortDir, string? filter);

    /// <summary>
    /// Delete tags in bulk.
    /// </summary>
    /// <param name="tagIdsToDelete">The tags ids to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteTagsAsync(List<Guid> tagIdsToDelete);

    /// <summary>
    /// Get the autocomplete result for the tag.
    /// </summary>
    /// <param name="partialTagName">The partial tag name.</param>
    /// <returns>The autocomplete result.</returns>
    Task<List<string>> AutocompleteAsync(string? partialTagName);

    /// <summary>
    /// Update an tag.
    /// </summary>
    /// <param name="tagId">The id of the tag to update.</param>
    /// <param name="tagDto">The updated tag dto.</param>
    /// <returns>A <see cref="Task{TagDTO}"/> representing the result of the asynchronous operation.</returns>
    Task<TagDTO?> UpdateTagAsync(Guid tagId, TagDTO tagDto);

    /// <summary>
    /// Merge the source tags into the target tag.
    /// </summary>
    /// <param name="targetTagId">The target tag id.</param>
    /// <param name="sourceTagIds">The source tag ids.</param>
    /// <returns>A task.</returns>
    Task MergeTags(Guid targetTagId, List<Guid> sourceTagIds);
}
