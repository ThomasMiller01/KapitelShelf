// <copyright file="WatchlistLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Watchlist;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Logic.Interfaces.Storage;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models.Watchlists;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="WatchlistLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The mapper.</param>
/// <param name="seriesLogic">The series logic.</param>
/// <param name="booksLogic">The books logic.</param>
/// <param name="watchlistScraperManager">The watchlist scraper manager.</param>
/// <param name="bookStorage">The book storage.</param>
/// <param name="metadataLogic">The metadata logic.</param>
public class WatchlistLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, Mapper mapper, ISeriesLogic seriesLogic, IBooksLogic booksLogic, IWatchlistScraperManager watchlistScraperManager, IBookStorage bookStorage, IMetadataLogic metadataLogic) : IWatchlistLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly Mapper mapper = mapper;

    private readonly ISeriesLogic seriesLogic = seriesLogic;

    private readonly IBooksLogic booksLogic = booksLogic;

    private readonly IWatchlistScraperManager watchlistScraperManager = watchlistScraperManager;

    private readonly IBookStorage bookStorage = bookStorage;

    private readonly IMetadataLogic metadataLogic = metadataLogic;

    /// <inheritdoc/>
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
                    .Min(r => (DateTime?)(object?)r.ReleaseDate))

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
            var dto = this.mapper.WatchlistModelToSeriesWatchlistDto(x.Watchlist);
            dto.Items = x.Items.Select(this.mapper.WatchlistResultModelToBookDto).ToList();
            return dto;
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<bool> IsOnWatchlist(Guid seriesId, Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Watchlist
            .AnyAsync(x => x.SeriesId == seriesId && x.UserId == userId);
    }

    /// <inheritdoc/>
    public async Task<SeriesWatchlistDTO?> AddToWatchlist(Guid seriesId, Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var series = await this.seriesLogic.GetSeriesByIdAsync(seriesId);
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

        var watchlistModel = new WatchlistModel
        {
            Id = Guid.NewGuid(),
            SeriesId = series.Id,
            UserId = userId,
        };

        await context.Watchlist.AddAsync(watchlistModel);
        await context.SaveChangesAsync();

        // update the watchlist (fire and forget)
        _ = this.UpdateWatchlist(watchlistModel.Id);

        return this.mapper.WatchlistModelToSeriesWatchlistDto(watchlistModel);
    }

    /// <inheritdoc/>
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

        return this.mapper.WatchlistModelToSeriesWatchlistDto(seriesWatchlistModel);
    }

    /// <inheritdoc/>
    public async Task UpdateWatchlist(Guid watchlistId)
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

        var volumes = await this.watchlistScraperManager.Scrape(this.mapper.SeriesModelToSeriesDto(watchlist.Series));

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

    /// <inheritdoc/>
    public async Task<BookDTO?> AddResultToLibrary(Guid resultId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var result = await context.WatchlistResults
            .Include(x => x.Series)
            .FirstOrDefaultAsync(x => x.Id == resultId);

        if (result is null)
        {
            return null;
        }

        // add result to library
        var createBookDto = this.mapper.WatchlistResultModelToCreateBookDto(result);
        var bookDto = await this.booksLogic.CreateBookAsync(createBookDto);
        if (bookDto is null)
        {
            return null;
        }

        // upload cover
        if (result.CoverUrl is not null)
        {
            var (coverBytes, _) = await this.metadataLogic.ProxyCover(result.CoverUrl);
            var coverFile = coverBytes.ToFile("cover.png");

            var cover = await this.bookStorage.Save(bookDto.Id, coverFile);

            bookDto.Cover = cover;
            await this.booksLogic.UpdateBookAsync(bookDto.Id, bookDto);
        }

        // remove result from watchlist results
        context.WatchlistResults.Remove(result);
        await context.SaveChangesAsync();

        return bookDto;
    }
}
