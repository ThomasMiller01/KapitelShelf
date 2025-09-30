// <copyright file="SeriesLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Watchlist;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using KapitelShelf.Data.Models.Watchlists;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="SeriesLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The auto mapper.</param>
/// <param name="booksLogic">The books logic.</param>
/// <param name="watchlistScraperManager">The watchlist scraper manager.</param>
public class SeriesLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, IMapper mapper, IBooksLogic booksLogic, IWatchlistScraperManager watchlistScraperManager)
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly IMapper mapper = mapper;

    private readonly IBooksLogic booksLogic = booksLogic;

    private readonly IWatchlistScraperManager watchlistScraperManager = watchlistScraperManager;

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

            .Include(x => x.Books)
                .ThenInclude(x => x.Author)
            .Include(x => x.Books)
                .ThenInclude(b => b.Cover)
            .Include(x => x.Books)
                .ThenInclude(b => b.Location)
            .AsSingleQuery()

            .Where(x => x.Id == seriesId)
            .Select(x => this.mapper.Map<SeriesDTO>(x))
            .FirstOrDefaultAsync();

        if (series is null)
        {
            return null;
        }

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

    /// <summary>
    /// Get the series watchlists for a user.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <returns>The list of watchlists.</returns>
    public async Task<List<SeriesWatchlistDTO>> GetWatchlistAsync(Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var watchlists = await context.Watchlist
            .AsNoTracking()
            .Include(x => x.Series)
            .Where(x => x.UserId == userId)

            // 1. Order first all series with watchlist items
            .OrderByDescending(sw =>
                context.WatchlistResults
                    .Any(r => r.SeriesId == sw.SeriesId))

            // 2. Order by the watchlist item release date
            .ThenBy(sw =>
                context.WatchlistResults
                    .Where(r => r.SeriesId == sw.SeriesId)
                    .Max(r => (DateTime?)(object?)r.ReleaseDate))

            // 3. Then by the latest book in the series release date (but sort by earliest book)
            .ThenBy(sw =>
                sw.Series.Books
                    .Max(b => (DateTime?)(object?)b.ReleaseDate))

            .Select(sw => new
            {
                Watchlist = sw,
                Items = context.WatchlistResults
                    .Where(r => r.SeriesId == sw.SeriesId)
                    .OrderBy(r => (DateTime?)(object?)r.ReleaseDate)
                    .ThenBy(r => r.Volume)
                    .ToList(),
            })
            .ToListAsync();

        return watchlists.Select(x =>
        {
            var dto = this.mapper.Map<SeriesWatchlistDTO>(x.Watchlist);
            dto.Items = this.mapper.Map<List<BookDTO>>(x.Items);
            return dto;
        }).ToList();
    }

    /// <summary>
    /// Check if the series is on the watchlist.
    /// </summary>
    /// <param name="seriesId">The id of the series.</param>
    /// <param name="userId">The id of the user.</param>
    /// <returns>true, if the series is on the watchlist, otherwise false.</returns>
    public async Task<bool> IsOnWatchlist(Guid seriesId, Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Watchlist
            .AnyAsync(x => x.SeriesId == seriesId && x.UserId == userId);
    }

    /// <summary>
    /// Add the series to the watchlist of a user.
    /// </summary>
    /// <param name="seriesId">The id of the series.</param>
    /// <param name="userId">The id of the user.</param>
    /// <returns>The new watchlist dto.</returns>
    public async Task<SeriesWatchlistDTO?> AddToWatchlist(Guid seriesId, Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var series = await this.GetSeriesByIdAsync(seriesId);
        if (series is null)
        {
            return null;
        }

        if (series.LastVolume?.Location?.Type is not null && !StaticConstants.LocationsSupportSeriesWatchlist.Contains(series.LastVolume.Location.Type))
        {
            throw new ArgumentException($"Location '{series.LastVolume.Location.Type}' does not support watchlists.");
        }

        var existingWatchlist = await context.Watchlist
            .Where(x => x.SeriesId == seriesId && x.UserId == userId)
            .AnyAsync();

        if (existingWatchlist)
        {
            throw new ArgumentException($"Series '{series.Name}' is already on the watchlist.");
        }

        var seriesWatchlistModel = new WatchlistModel
        {
            Id = Guid.NewGuid(),
            SeriesId = series.Id,
            UserId = userId,
        };

        await context.Watchlist.AddAsync(seriesWatchlistModel);
        await context.SaveChangesAsync();

        return this.mapper.Map<SeriesWatchlistDTO>(seriesWatchlistModel);
    }

    /// <summary>
    /// Remove the series from the watchlist of a user.
    /// </summary>
    /// <param name="seriesId">The id of the series.</param>
    /// <param name="userId">The id of the user.</param>
    /// <returns>The new watchlist dto.</returns>
    public async Task<SeriesWatchlistDTO?> RemoveFromWatchlist(Guid seriesId, Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var seriesWatchlistModel = await context.Watchlist
            .FirstOrDefaultAsync(x => x.SeriesId == seriesId && x.UserId == userId);

        if (seriesWatchlistModel is null)
        {
            return null;
        }

        context.Watchlist.Remove(seriesWatchlistModel);
        await context.SaveChangesAsync();

        // delete the series watchlist results, if no other users is watching this series
        var stillWatched = await context.Watchlist.AnyAsync(x => x.SeriesId == seriesId);
        if (!stillWatched)
        {
            await context.WatchlistResults
                .Where(r => r.SeriesId == seriesId)
                .ExecuteDeleteAsync();
        }

        return this.mapper.Map<SeriesWatchlistDTO>(seriesWatchlistModel);
    }

    /// <summary>
    /// Update the watchlist and check for new volumes.
    /// </summary>
    /// <param name="watchlistId">The id of the watchlist.</param>
    /// <returns>A task.</returns>
#pragma warning disable IDE0060 // Remove unused parameter
    public async Task UpdateWatchlist(Guid watchlistId)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var watchlist = await context.Watchlist
            .AsNoTracking()
            .Include(x => x.Series)
                .ThenInclude(x => x.Books)
                    .ThenInclude(x => x.Location)
            .AsSingleQuery()
            .FirstOrDefaultAsync(x => x.Id == watchlistId);

        if (watchlist?.Series is null)
        {
            return;
        }

        var volumes = await this.watchlistScraperManager.Scrape(this.mapper.Map<SeriesDTO>(watchlist.Series));

        // filter any volumes that do not exist in the library
        var existingBookTitles = watchlist.Series.Books
            .Select(x => x.Title);

        volumes = volumes
            .Where(x => !existingBookTitles.Contains(x.Title ?? string.Empty))
            .ToList();

        // only keep the volumes after the last from the library
        var lastExistingVolume = watchlist.Series.Books
            .OrderByDescending(x => x.ReleaseDate)
            .FirstOrDefault();

        volumes = volumes
            .Where(x => x.Volume > (lastExistingVolume?.SeriesNumber ?? 0))
            .ToList();

        // add/update the volumes
        foreach (var volume in volumes)
        {
            var volumeModel = await context.WatchlistResults
                .FirstOrDefaultAsync(x => x.SeriesId == volume.SeriesId && x.Volume == volume.Volume);

            if (volumeModel is null)
            {
                // add new volume
                volume.Id = Guid.NewGuid();
                context.WatchlistResults.Add(volume);
            }
            else
            {
                // update volume data
                volumeModel.Title = volume.Title;
                volumeModel.Description = volume.Description;
                volumeModel.ReleaseDate = volume.ReleaseDate;
                volumeModel.Pages = volume.Pages;
                volumeModel.CoverUrl = volume.CoverUrl;
                volumeModel.Categories = volume.Categories;
                volumeModel.Tags = volume.Tags;
            }
        }

        await context.SaveChangesAsync();
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
