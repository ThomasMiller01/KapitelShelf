// <copyright file="WatchlistMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using AutoMapper;
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
/// The watchlist mapping profile.
/// </summary>
public class WatchlistMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WatchlistMappingProfile"/> class.
    /// </summary>
    public WatchlistMappingProfile()
    {
        CreateMap<WatchlistModel, SeriesWatchlistDTO>()
            .ReverseMap();

        CreateMap<WatchlistResultModel, BookDTO>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.Pages))
            .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(src => MapReleaseDate(src.ReleaseDate)))
            .ForMember(dest => dest.SeriesNumber, opt => opt.MapFrom(src => src.Volume))
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => MapAuthor(src)))
            .ForMember(dest => dest.Cover, opt => opt.MapFrom(src =>
                src.CoverUrl != null
                    ? new FileInfoDTO
                    {
                        FilePath = src.CoverUrl,
                    }
                    : null))
            .ForMember(dest => dest.Location, opt => opt.MapFrom((src, _, _, context) => new LocationDTO
            {
                Type = context.Mapper.Map<LocationTypeDTO>(src.LocationType),
                Url = src.LocationUrl,
            }))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => new CategoryDTO { Name = c }).ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(t => new TagDTO { Name = t }).ToList()));

        CreateMap<MetadataDTO, WatchlistResultModel>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SeriesId, opt => opt.Ignore())
            .ForMember(dest => dest.Series, opt => opt.Ignore())
            .ForMember(dest => dest.LocationType, opt => opt.Ignore())
            .ForMember(dest => dest.LocationUrl, opt => opt.Ignore())

            .ReverseMap();

        CreateMap<WatchlistResultModel, CreateBookDTO>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(src => MapReleaseDate(src.ReleaseDate)))
            .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.Pages))
            .ForMember(dest => dest.SeriesNumber, opt => opt.MapFrom(src => src.Volume))
            .ForMember(dest => dest.Series, opt => opt.MapFrom(src => new CreateSeriesDTO { Name = src.Series.Name }))
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => MapCreateAuthor(src)))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => new CreateCategoryDTO { Name = c }).ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(t => new CreateTagDTO { Name = t }).ToList()))
            .ForMember(dest => dest.Location, opt => opt.MapFrom((src, _, _, context) => new CreateLocationDTO
            {
                Type = context.Mapper.Map<LocationTypeDTO>(src.LocationType),
                Url = src.LocationUrl,
            }));
    }

    /// <summary>
    /// Map the created author from the metadata.
    /// </summary>
    /// <param name="metadata">The metadata.</param>
    /// <returns>The created author.</returns>
    private static AuthorDTO? MapAuthor(WatchlistResultModel metadata)
    {
        if (metadata.Authors.Count > 0)
        {
            var nameParts = metadata.Authors[0].Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            return new AuthorDTO
            {
                FirstName = nameParts.Length > 1 ? nameParts[0] : string.Empty,
                LastName = nameParts.Length > 1 ? nameParts[1] : metadata.Authors[0],
            };
        }

        return null;
    }

    /// <summary>
    /// Map an author for CreateBookDTO.
    /// </summary>
    private static CreateAuthorDTO? MapCreateAuthor(WatchlistResultModel metadata)
    {
        if (metadata.Authors.Count > 0)
        {
            var nameParts = metadata.Authors[0].Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            return new CreateAuthorDTO
            {
                FirstName = nameParts.Length > 1 ? nameParts[0] : string.Empty,
                LastName = nameParts.Length > 1 ? nameParts[1] : metadata.Authors[0],
            };
        }

        return null;
    }

    /// <summary>
    /// Map the release date from a string.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>The release date.</returns>
    private static DateTime? MapReleaseDate(string? value)
    {
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            return dt.ToUniversalTime();
        }

        return null;
    }
}
