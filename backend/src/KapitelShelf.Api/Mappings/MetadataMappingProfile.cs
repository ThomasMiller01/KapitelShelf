// <copyright file="MetadataMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using AutoMapper;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Tag;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The user mapping profile.
/// </summary>
public class MetadataMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataMappingProfile"/> class.
    /// </summary>
    public MetadataMappingProfile()
    {
        CreateMap<MetadataDTO, CreateBookDTO>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.Pages))
            .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(src => MapReleaseDate(src.ReleaseDate)))
            .ForMember(
                dest => dest.Series,
                opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.Series)
                    ? null
                    : new CreateSeriesDTO { Name = src.Series! }))
            .ForMember(dest => dest.SeriesNumber, opt => opt.MapFrom(src => src.Volume))
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => MapAuthor(src)))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => new CreateCategoryDTO { Name = c }).ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(t => new CreateTagDTO { Name = t }).ToList()))
            .ForMember(
                dest => dest.Location,
                opt => opt.MapFrom(src =>
                    src.CoverUrl == null
                        ? new CreateLocationDTO { Type = LocationTypeDTO.KapitelShelf, Url = src.CoverUrl }
                        : null));

        CreateMap<MetadataSources, LocationTypeDTO>()
            .ConvertUsing(src => MapMetadataSourceToLocationType(src));
    }

    /// <summary>
    /// Maps metadata source to the location type.
    /// </summary>
    /// <param name="source">The metadata source.</param>
    /// <returns>The mapped location type.</returns>
    private static LocationTypeDTO MapMetadataSourceToLocationType(MetadataSources source)
    {
        return source switch
        {
            MetadataSources.Amazon => LocationTypeDTO.Kindle,
            MetadataSources.GoogleBooks => LocationTypeDTO.KapitelShelf,
            MetadataSources.OpenLibrary => LocationTypeDTO.KapitelShelf,
            _ => LocationTypeDTO.KapitelShelf
        };
    }

    /// <summary>
    /// Map the created author from the metadata.
    /// </summary>
    /// <param name="metadata">The metadata.</param>
    /// <returns>The created author.</returns>
    private static CreateAuthorDTO? MapAuthor(MetadataDTO metadata)
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
