// <copyright file="AuthorsLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Resources;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="AuthorsLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The mapper.</param>
public class AuthorsLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, Mapper mapper) : IAuthorsLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

#pragma warning disable IDE0052 // Remove unread private members
    private readonly Mapper mapper = mapper;
#pragma warning restore IDE0052 // Remove unread private members

    /// <inheritdoc/>
    public async Task<PagedResult<AuthorDTO>> GetAuthorsAsync(int page, int pageSize, AuthorSortByDTO sortBy, SortDirectionDTO sortDir, string? filter)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var query = context.Authors
            .AsNoTracking()
            .Include(x => x.Books)

            // apply filter if it is set
            .FilterByAuthorQuery(filter)
            .SortByAuthorQuery(filter);

        // apply sorting, if no filter is specified
        // or if a specific sorting is requested
        if (filter is null || sortBy != AuthorSortByDTO.Default)
        {
            query = query.ApplySorting(sortBy, sortDir);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)

            .Select(x => this.mapper.AuthorModelToAuthorDto(x))
            .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PagedResult<AuthorDTO>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }

    /// <inheritdoc/>
    public async Task DeleteAuthorsAsync(List<Guid> authorIdsToDelete)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        await context.Authors
            .Where(x => authorIdsToDelete.Contains(x.Id))
            .ExecuteDeleteAsync();
    }

    /// <inheritdoc/>
    public async Task<List<string>> AutocompleteAsync(string? partialAuthor)
    {
        if (string.IsNullOrWhiteSpace(partialAuthor))
        {
            return [];
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Authors
            .AsNoTracking()
            .FilterByAuthorQuery(partialAuthor)
            .SortByAuthorQuery(partialAuthor)
            .Take(5)
            .Select(x => x.FirstName + " " + x.LastName)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<AuthorDTO?> UpdateAuthorAsync(Guid authorId, AuthorDTO authorDto)
    {
        if (authorDto is null)
        {
            return null;
        }

        // check for duplicate series
        var duplicates = await this.GetDuplicatesAsync(authorDto.FirstName, authorDto.LastName);
        if (duplicates.Any())
        {
            throw new InvalidOperationException(StaticConstants.DuplicateExceptionKey);
        }

        await using var context = await this.dbContextFactory.CreateDbContextAsync();

        var author = await context.Authors
            .FirstOrDefaultAsync(b => b.Id == authorId);

        if (author is null)
        {
            return null;
        }

        // patch author root scalars
        context.Entry(author).CurrentValues.SetValues(new
        {
            authorDto.FirstName,
            authorDto.LastName,
            UpdatedAt = DateTime.UtcNow,
        });

        // commit
        await context.SaveChangesAsync();

        return this.mapper.AuthorModelToAuthorDto(author);
    }

    /// <inheritdoc/>
    public async Task MergeAuthors(Guid targetAuthorId, List<Guid> sourceAuthorsIds)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var targetAuthor = await context.Authors.FirstOrDefaultAsync(x => x.Id == targetAuthorId);
        if (targetAuthor is null)
        {
            throw new ArgumentException("Unknown target author id.");
        }

        var sourceAuthors = await context.Authors
            .Include(x => x.Books)
            .Where(x => sourceAuthorsIds.Contains(x.Id))
            .ToListAsync();

        if (sourceAuthors.Count == 0)
        {
            throw new ArgumentException("Unknown source author ids.");
        }

        // update the author of all books
        var sourceBooks = sourceAuthors.SelectMany(x => x.Books);
        foreach (var book in sourceBooks)
        {
            book.AuthorId = targetAuthorId;
            book.UpdatedAt = DateTime.UtcNow;
        }

        targetAuthor.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        // delete source authors
        await this.DeleteAuthorsAsync(sourceAuthors.Select(x => x.Id).ToList());
    }

    internal async Task<IList<AuthorModel>> GetDuplicatesAsync(string firstName, string lastName)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Authors
            .AsNoTracking()
            .Where(x => x.FirstName == firstName && x.LastName == lastName)
            .ToListAsync();
    }
}
