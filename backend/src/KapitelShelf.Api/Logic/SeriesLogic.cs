﻿// <copyright file="SeriesLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="SeriesLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The auto mapper.</param>
/// <param name="booksLogic">The books logic.</param>
public class SeriesLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, IMapper mapper, IBooksLogic booksLogic)
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly IMapper mapper = mapper;

    private readonly IBooksLogic booksLogic = booksLogic;

    /// <summary>
    /// Get all series.
    /// </summary>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    public async Task<PagedResult<SeriesDTO>> GetSeriesAsync(int page, int pageSize)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();
        context.ChangeTracker.LazyLoadingEnabled = false;

        var query = context.Series
            .AsNoTracking();

        var items = await query
            .Include(x => x.Books)
                .ThenInclude(x => x.Author)
            .Include(x => x.Books)
                .ThenInclude(b => b.Cover)
            .AsSingleQuery()

            .OrderByDescending(x => x.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)

            .Select(x => this.mapper.Map<SeriesDTO>(x))
            .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PagedResult<SeriesDTO>
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
    /// Search all series by their name.
    /// </summary>
    /// <param name="name">The series name.</param>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    public async Task<PagedResult<SeriesDTO>> Search(string name, int page, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new PagedResult<SeriesDTO> { Items = [], TotalCount = 0 };
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var query = context.Series
            .AsNoTracking()

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
            .AsSingleQuery()

            .FilterBySeriesNameQuery(name);

        var items = await query
            .SortBySeriesNameQuery(name)

            .Skip((page - 1) * pageSize)
            .Take(pageSize)

            .Select(x => this.mapper.Map<SeriesDTO>(x))
            .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PagedResult<SeriesDTO>
        {
            Items = items,
            TotalCount = totalCount,
        };
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
            .AsSingleQuery()

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

    /// <summary>
    /// Delete a series.
    /// </summary>
    /// <param name="seriesId">The id of the series to delete.</param>
    /// <returns>A <see cref="Task{SeriesDTO}"/> representing the result of the asynchronous operation.</returns>
    public async Task<SeriesDTO?> DeleteSeriesAsync(Guid seriesId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var series = await context.Series.FindAsync(seriesId);
        if (series is null)
        {
            return null;
        }

        await this.DeleteFilesAsync(seriesId);

        context.Series.Remove(series);
        await context.SaveChangesAsync();

        await this.booksLogic.CleanupDatabase();

        return this.mapper.Map<SeriesDTO>(series);
    }

    /// <summary>
    /// Update a series.
    /// </summary>
    /// <param name="seriesId">The id of the series to update.</param>
    /// <param name="seriesDto">The updated series dto.</param>
    /// <returns>A <see cref="Task{SeriesDTO}"/> representing the result of the asynchronous operation.</returns>
    public async Task<SeriesDTO?> UpdateSeriesAsync(Guid seriesId, SeriesDTO seriesDto)
    {
        if (seriesDto is null)
        {
            return null;
        }

        // check for duplicate series
        var duplicates = await this.GetDuplicatesAsync(seriesDto.Name);
        if (duplicates.Any())
        {
            throw new InvalidOperationException(StaticConstants.DuplicateExceptionKey);
        }

        await using var context = await this.dbContextFactory.CreateDbContextAsync();

        var series = await context.Series
            .FirstOrDefaultAsync(b => b.Id == seriesId);

        if (series is null)
        {
            return null;
        }

        // patch series root scalars
        context.Entry(series).CurrentValues.SetValues(new
        {
            seriesDto.Name,
            updatedAt = DateTime.UtcNow,
        });

        // commit
        await context.SaveChangesAsync();

        return this.mapper.Map<SeriesDTO>(series);
    }

    /// <summary>
    /// Delete the files of all books from a series.
    /// </summary>
    /// <param name="seriesId">The id of the series.</param>
    /// <returns>A task.</returns>
    public async Task DeleteFilesAsync(Guid seriesId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var bookIds = await context.Series
            .AsNoTracking()
            .Include(x => x.Books)
            .Where(x => x.Id == seriesId)
            .SelectMany(x => x.Books)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var bookId in bookIds)
        {
            this.booksLogic.DeleteFiles(bookId);
        }
    }

    /// <summary>
    /// Merge all books from the source series into the target series.
    /// </summary>
    /// <param name="sourceSeriesId">The source series id.</param>
    /// <param name="targetSeriesId">The target series id.</param>
    /// <returns>A task.</returns>
    public async Task MergeSeries(Guid sourceSeriesId, Guid targetSeriesId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var sourceSeries = await context.Series
            .Include(x => x.Books)
            .Where(x => x.Id == sourceSeriesId)
            .FirstOrDefaultAsync();

        if (sourceSeries is null)
        {
            throw new ArgumentException("Unknown source series id.");
        }

        var targetSeriesExists = await context.Series.AnyAsync(x => x.Id == targetSeriesId);
        if (!targetSeriesExists)
        {
            throw new ArgumentException("Unknown target series id.");
        }

        // move all books to the target series
        foreach (var book in sourceSeries.Books)
        {
            book.SeriesId = targetSeriesId;
        }

        await context.SaveChangesAsync();

        // delete target series
        await this.DeleteSeriesAsync(sourceSeriesId);
    }

    internal async Task<IList<SeriesModel>> GetDuplicatesAsync(string name)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Series
            .AsNoTracking()
            .Where(x => x.Name == name)
            .ToListAsync();
    }
}
