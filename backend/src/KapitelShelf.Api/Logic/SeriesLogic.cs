// <copyright file="SeriesLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Data;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="SeriesLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The auto mapper.</param>
public class SeriesLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, IMapper mapper)
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Get all series.
    /// </summary>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    public async Task<PagedResult<SeriesSummaryDTO>> GetSeriesSummaryAsync(int page, int pageSize)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var query = context.Series
            .AsNoTracking();

        var items = await query
            .Include(x => x.Books)
                .ThenInclude(x => x.Author)
            .Include(x => x.Books)
                .ThenInclude(b => b.Cover)
            .Include(x => x.Books)
                .ThenInclude(b => b.Categories)
                    .ThenInclude(x => x.Category)
            .Include(x => x.Books)
                .ThenInclude(b => b.Tags)
                    .ThenInclude(x => x.Tag)
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => this.mapper.Map<SeriesSummaryDTO>(x))
            .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PagedResult<SeriesSummaryDTO>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }
}
