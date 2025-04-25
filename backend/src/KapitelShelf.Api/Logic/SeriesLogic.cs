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

    /// <summary>
    /// Get a series by id.
    /// </summary>
    /// <param name="seriesId">The id of the series to fetch.</param>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    public async Task<SeriesDTO?> GetSeriesByIdAsync(Guid seriesId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var series = await context.Series
            .AsNoTracking()
            .Where(x => x.Id == seriesId)
            .Select(x => this.mapper.Map<SeriesDTO>(x))
            .FirstOrDefaultAsync();

        if (series is null)
        {
            return null;
        }

        var bookCount = await context.Books
            .AsNoTracking()
            .Where(x => x.SeriesId == seriesId)
            .CountAsync();

        series.TotalBooks = bookCount;

        return series;
    }

    /// <summary>
    /// Get a series by id.
    /// </summary>
    /// <param name="seriesId">The id of the series to fetch.</param>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    public async Task<PagedResult<BookDTO>> GetBooksBySeriesIdAsync(Guid seriesId, int page, int pageSize)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var query = context.Books
            .AsNoTracking();

        var items = await query
            .Include(x => x.Author)
            .Include(x => x.Cover)
            .Include(x => x.Location)
            .Include(x => x.Categories)
                .ThenInclude(x => x.Category)
            .Include(x => x.Tags)
                .ThenInclude(x => x.Tag)

            .Where(x => x.SeriesId == seriesId)

            .OrderBy(x => x.SeriesNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)

            .Select(x => this.mapper.Map<BookDTO>(x))
            .ToListAsync();

        var totalCount = await query
            .Where(x => x.SeriesId == seriesId)
            .CountAsync();

        return new PagedResult<BookDTO>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }
}
