// <copyright file="AuthorsLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data;
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
}
