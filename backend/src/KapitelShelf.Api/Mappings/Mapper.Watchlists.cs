// <copyright file="Mapper.Watchlists.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Api.DTOs.Watchlist;
using KapitelShelf.Data.Models.Watchlists;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The watchlists mapper.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a watchlist result model to a book dto.
    /// </summary>
    /// <param name="model">The watchlist result model.</param>
    /// <returns>The book dto.</returns>
    public BookDTO WatchlistResultModelToBookDto(WatchlistResultModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var dto = new BookDTO
        {
            Title = model.Title,
            Description = model.Description ?? string.Empty,
            ReleaseDate = ParseDate(model.ReleaseDate),
            PageNumber = model.Pages,
            SeriesNumber = model.Volume ?? 0,
            Author = this.WatchlistResultModelToAuthorDto(model),
            Cover = model.CoverUrl is null ? null : new FileInfoDTO { FilePath = model.CoverUrl },
            Location = new LocationDTO
            {
                Type = this.LocationTypeToLocationTypeDto(model.LocationType),
                Url = model.LocationUrl,
            },
            Categories = model.Categories.Select(name => new CategoryDTO { Name = name }).ToList(),
            Tags = model.Tags.Select(name => new TagDTO { Name = name }).ToList(),
        };

        return dto;
    }

    /// <summary>
    /// Map a watchlist result model to a create book dto.
    /// </summary>
    /// <param name="model">The watchlist result model.</param>
    /// <returns>The create book dto.</returns>
    public CreateBookDTO WatchlistResultModelToCreateBookDto(WatchlistResultModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new CreateBookDTO
        {
            Title = model.Title,
            Description = model.Description ?? string.Empty,
            ReleaseDate = ParseDate(model.ReleaseDate),
            PageNumber = model.Pages,
            SeriesNumber = model.Volume ?? 0,
            Series = new CreateSeriesDTO { Name = model.Series.Name },
            Author = this.WatchlistResultModelToCreateAuthorDto(model),
            Categories = model.Categories.Select(name => new CreateCategoryDTO { Name = name }).ToList(),
            Tags = model.Tags.Select(name => new CreateTagDTO { Name = name }).ToList(),
            Location = new CreateLocationDTO
            {
                Type = this.LocationTypeToLocationTypeDto(model.LocationType),
                Url = model.LocationUrl,
            },
        };
    }

    /// <summary>
    /// Map a metadata dto to a watchlist result model.
    /// </summary>
    /// <param name="dto">The metadata dto.</param>
    /// <returns>The watchlist result model.</returns>
    public WatchlistResultModel MetadataDtoToWatchlistResultModel(MetadataDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new WatchlistResultModel
        {
            Title = dto.Title ?? string.Empty,
            Description = dto.Description,
            Volume = dto.Volume,
            Authors = dto.Authors.ToList(),
            ReleaseDate = dto.ReleaseDate,
            Pages = dto.Pages,
            CoverUrl = dto.CoverUrl,
            Categories = dto.Categories.ToList(),
            Tags = dto.Tags.ToList(),
        };
    }

    /// <summary>
    /// Map a nullable metadata dto to a nullable watchlist result model.
    /// </summary>
    /// <param name="dto">The metadata dto.</param>
    /// <returns>The watchlist result model.</returns>
    public WatchlistResultModel? MetadataDtoToWatchlistResultModelNullable(MetadataDTO? dto) => dto is null ? null : this.MetadataDtoToWatchlistResultModel(dto);

    /// <summary>
    /// Map a watchlist model to a series watchlist dto.
    /// </summary>
    /// <param name="model">The watchlist model.</param>
    /// <returns>The series watchlist dto.</returns>
    public SeriesWatchlistDTO WatchlistModelToSeriesWatchlistDto(WatchlistModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new SeriesWatchlistDTO
        {
            Id = model.Id,
            Series = this.SeriesModelToSeriesDto(model.Series),
            Items = [],
        };
    }

    /// <summary>
    /// Map a watchlist result model to a author dto.
    /// </summary>
    /// <param name="model">The watchlist result model.</param>
    /// <returns>The author dto.</returns>
    public AuthorDTO? WatchlistResultModelToAuthorDto(WatchlistResultModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (model.Authors.Count == 0)
        {
            return null;
        }

        var (firstName, lastName) = SplitName(model.Authors[0]);
        return new AuthorDTO
        {
            FirstName = firstName,
            LastName = lastName,
        };
    }

    /// <summary>
    /// Map a watchlist result model to a create author dto.
    /// </summary>
    /// <param name="model">The watchlist result model.</param>
    /// <returns>The create author dto.</returns>
    public CreateAuthorDTO? WatchlistResultModelToCreateAuthorDto(WatchlistResultModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (model.Authors.Count == 0)
        {
            return null;
        }

        var (firstName, lastName) = SplitName(model.Authors[0]);
        return new CreateAuthorDTO
        {
            FirstName = firstName,
            LastName = lastName,
        };
    }
}
