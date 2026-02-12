// <copyright file="SeriesLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Resources;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="SeriesLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The mapper.</param>
/// <param name="booksLogic">The books logic.</param>
public class SeriesLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, Mapper mapper, IBooksLogic booksLogic) : ISeriesLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly Mapper mapper = mapper;

    private readonly IBooksLogic booksLogic = booksLogic;

    /// <inheritdoc/>
    public async Task<PagedResult<SeriesDTO>> GetSeriesAsync(int page, int pageSize, SeriesSortByDTO sortBy, SortDirectionDTO sortDir, string? filter)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();
        context.ChangeTracker.LazyLoadingEnabled = false;

        var query = context.Series
            .AsNoTracking()

            .Include(x => x.Books)
                .ThenInclude(x => x.Author)
            .Include(x => x.Books)
                .ThenInclude(b => b.Cover)
            .Include(x => x.Books)
                .ThenInclude(b => b.UserMetadata)
                    .ThenInclude(x => x.User)
            .AsSingleQuery()

            // apply filter if it is set
            .FilterBySeriesNameQuery(filter)
            .SortBySeriesNameQuery(filter);

        // apply sorting, if no filter is specified
        // or if a specific sorting is requested
        if (filter is null || sortBy != SeriesSortByDTO.Default)
        {
            query = query.ApplySorting(sortBy, sortDir);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)

            .Select(x => this.mapper.SeriesModelToSeriesDto(x))
            .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PagedResult<SeriesDTO>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }

    /// <inheritdoc/>
    public async Task<SeriesDTO?> GetSeriesByIdAsync(Guid seriesId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var series = await context.Series
            .AsNoTracking()

            .Include(x => x.Books)
                .ThenInclude(x => x.Author)
            .Include(x => x.Books)
                .ThenInclude(b => b.Cover)
            .Include(x => x.Books)
                .ThenInclude(b => b.Location)
            .Include(x => x.Books)
                .ThenInclude(b => b.UserMetadata)
                    .ThenInclude(x => x.User)
            .AsSingleQuery()

            .Where(x => x.Id == seriesId)
            .Select(x => this.mapper.SeriesModelToSeriesDto(x))
            .FirstOrDefaultAsync();

        if (series is null)
        {
            return null;
        }

        return series;
    }

    /// <inheritdoc/>
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

            .Select(x => this.mapper.SeriesModelToSeriesDto(x))
            .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PagedResult<SeriesDTO>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }

    /// <inheritdoc/>
    public async Task<List<string>> AutocompleteAsync(string? partialSeriesName)
    {
        if (string.IsNullOrWhiteSpace(partialSeriesName))
        {
            return [];
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Series
            .AsNoTracking()
            .FilterBySeriesNameQuery(partialSeriesName)
            .SortBySeriesNameQuery(partialSeriesName)
            .Take(5)
            .Select(x => x.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
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
            .Include(x => x.UserMetadata)
                .ThenInclude(x => x.User)
            .AsSingleQuery()

            .Where(x => x.SeriesId == seriesId)

            .OrderBy(x => x.SeriesNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)

            .Select(x => this.mapper.BookModelToBookDto(x))
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

    /// <inheritdoc/>
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

        return this.mapper.SeriesModelToSeriesDto(series);
    }

    /// <inheritdoc/>
    public async Task DeleteSeriesAsync(List<Guid> seriesIdsToDelete)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var series = await context.Series
            .Where(x => seriesIdsToDelete.Contains(x.Id))
            .ToListAsync();

        foreach (var serie in series)
        {
            await this.DeleteFilesAsync(serie.Id);

            context.Series.Remove(serie);
        }

        await context.SaveChangesAsync();

        await this.booksLogic.CleanupDatabase();
    }

    /// <inheritdoc/>
    public async Task<SeriesDTO?> UpdateSeriesAsync(Guid seriesId, SeriesDTO seriesDto)
    {
        if (seriesDto is null)
        {
            return null;
        }

        // check for duplicate series
        var duplicates = await this.GetDuplicatesAsync(seriesDto);
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
            seriesDto.Rating,
            UpdatedAt = DateTime.UtcNow,
        });

        // commit
        await context.SaveChangesAsync();

        return this.mapper.SeriesModelToSeriesDto(series);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task MergeSeries(Guid targetSeriesId, List<Guid> sourceSeriesIds)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var targetSeries = await context.Series.FirstOrDefaultAsync(x => x.Id == targetSeriesId);
        if (targetSeries is null)
        {
            throw new ArgumentException("Unknown target series id.");
        }

        var sourceSeries = await context.Series
            .Include(x => x.Books)
            .Where(x => sourceSeriesIds.Contains(x.Id))
            .ToListAsync();

        if (sourceSeries.Count == 0)
        {
            throw new ArgumentException("Unknown source series ids.");
        }

        // move all books to the target series
        var sourceBooks = sourceSeries.SelectMany(x => x.Books);
        foreach (var book in sourceBooks)
        {
            book.SeriesId = targetSeriesId;
            book.UpdatedAt = DateTime.UtcNow;
        }

        targetSeries.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        // delete source series
        await this.DeleteSeriesAsync(sourceSeries.Select(x => x.Id).ToList());
    }

    internal async Task<IList<SeriesModel>> GetDuplicatesAsync(SeriesDTO series)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Series
            .AsNoTracking()
            .Where(x => x.Name == series.Name && x.Id != series.Id)
            .ToListAsync();
    }
}
