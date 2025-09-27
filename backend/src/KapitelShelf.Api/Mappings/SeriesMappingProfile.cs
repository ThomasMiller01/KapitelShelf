// <copyright file="SeriesMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using AutoMapper;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Api.DTOs.Watchlist;
using KapitelShelf.Data.Models;
using KapitelShelf.Data.Models.Watchlists;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The series mapping profile.
/// </summary>
public class SeriesMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeriesMappingProfile"/> class.
    /// </summary>
    public SeriesMappingProfile()
    {
        CreateMap<SeriesModel, SeriesDTO>()
            .ForMember(dest => dest.LastVolume, opt =>
                opt.MapFrom(src => src.Books
                    .OrderByDescending(b => b.SeriesNumber)
                    .FirstOrDefault()))
            .ForMember(dest => dest.TotalBooks, opt =>
                opt.MapFrom(src => src.Books.Count()))
            .AfterMap((src, dest) => // prevent circular dependencies series -> book -> series ...
            {
                if (dest.LastVolume != null)
                {
                    dest.LastVolume.Series = null;
                }
            })
            .ReverseMap();

        CreateMap<CreateSeriesDTO, SeriesModel>();

        CreateMap<SeriesDTO, CreateSeriesDTO>();

        CreateMap<SeriesWatchlistModel, SeriesWatchlistDTO>()
            .ReverseMap();

        CreateMap<SeriesWatchlistItemModel, BookDTO>()
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
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => new CategoryDTO { Name = c }).ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(t => new TagDTO { Name = t }).ToList()));
    }

    /// <summary>
    /// Map the created author from the metadata.
    /// </summary>
    /// <param name="metadata">The metadata.</param>
    /// <returns>The created author.</returns>
    private static AuthorDTO? MapAuthor(SeriesWatchlistItemModel metadata)
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
