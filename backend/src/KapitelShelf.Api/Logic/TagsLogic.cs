// <copyright file="TagsLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Resources;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="TagsLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The mapper.</param>
public class TagsLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, Mapper mapper) : ITagsLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

#pragma warning disable IDE0052 // Remove unread private members
    private readonly Mapper mapper = mapper;
#pragma warning restore IDE0052 // Remove unread private members

    /// <inheritdoc/>
    public async Task<PagedResult<TagDTO>> GetTagsAsync(int page, int pageSize, TagSortByDTO sortBy, SortDirectionDTO sortDir, string? filter)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var query = context.Tags
            .AsNoTracking()
            .Include(x => x.Books)

            // apply filter if it is set
            .FilterByTagQuery(filter)
            .SortByTagQuery(filter);

        // apply sorting, if no filter is specified
        // or if a specific sorting is requested
        if (filter is null || sortBy != TagSortByDTO.Default)
        {
            query = query.ApplySorting(sortBy, sortDir);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)

            .Select(x => this.mapper.TagModelToTagDto(x))
            .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PagedResult<TagDTO>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }

    /// <inheritdoc/>
    public async Task DeleteTagsAsync(List<Guid> tagIdsToDelete)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        await context.Tags
            .Where(x => tagIdsToDelete.Contains(x.Id))
            .ExecuteDeleteAsync();
    }

    /// <inheritdoc/>
    public async Task<List<string>> AutocompleteAsync(string? partialTagName)
    {
        if (string.IsNullOrWhiteSpace(partialTagName))
        {
            return [];
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Tags
            .AsNoTracking()
            .FilterByTagQuery(partialTagName)
            .SortByTagQuery(partialTagName)
            .Take(5)
            .Select(x => x.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<TagDTO?> UpdateTagAsync(Guid tagId, TagDTO tagDto)
    {
        if (tagDto is null)
        {
            return null;
        }

        // check for duplicate tags
        var duplicates = await this.GetDuplicatesAsync(tagDto.Name);
        if (duplicates.Any())
        {
            throw new InvalidOperationException(StaticConstants.DuplicateExceptionKey);
        }

        await using var context = await this.dbContextFactory.CreateDbContextAsync();

        var tag = await context.Tags
            .FirstOrDefaultAsync(b => b.Id == tagId);

        if (tag is null)
        {
            return null;
        }

        // patch tag root scalars
        context.Entry(tag).CurrentValues.SetValues(new
        {
            tagDto.Name,
            UpdatedAt = DateTime.UtcNow,
        });

        // commit
        await context.SaveChangesAsync();

        return this.mapper.TagModelToTagDto(tag);
    }

    /// <inheritdoc/>
    public async Task MergeTags(Guid targetTagId, List<Guid> sourceTagIds)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var targetTag = await context.Tags.FirstOrDefaultAsync(x => x.Id == targetTagId);
        if (targetTag is null)
        {
            throw new ArgumentException("Unknown target tag id.");
        }

        var sourceTags = await context.BookTags
            .Include(x => x.Book)
            .Where(x => sourceTagIds.Contains(x.TagId))
            .ToListAsync();

        if (sourceTags.Count == 0)
        {
            throw new ArgumentException("Unknown source tag ids.");
        }

        // update all categories
        foreach (var tag in sourceTags)
        {
            context.BookTags.Add(new BookTagModel
            {
                BookId = tag.BookId,
                TagId = targetTagId,
            });

            tag.Book.UpdatedAt = DateTime.UtcNow;
        }

        targetTag.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        // delete source tags
        await this.DeleteTagsAsync(sourceTags.Select(x => x.TagId).ToList());
    }

    internal async Task<IList<TagModel>> GetDuplicatesAsync(string name)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Tags
            .AsNoTracking()
            .Where(x => x.Name == name)
            .ToListAsync();
    }
}
