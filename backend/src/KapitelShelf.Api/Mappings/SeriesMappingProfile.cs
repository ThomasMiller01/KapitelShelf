// <copyright file="SeriesMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Data.Models;

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

#pragma warning disable CS0618 // Type or member is obsolete
        CreateMap<SeriesModel, SeriesSummaryDTO>()
            .ForMember(dest => dest.LastVolume, opt =>
                opt.MapFrom(src => src.Books
                    .OrderByDescending(b => b.SeriesNumber)
                    .FirstOrDefault()))
            .AfterMap((src, dest) => // prevent circular dependencies series -> book -> series ...
            {
                if (dest.LastVolume != null)
                {
                    dest.LastVolume.Series = null;
                }
            });
#pragma warning restore CS0618 // Type or member is obsolete

        CreateMap<CreateSeriesDTO, SeriesModel>();

        CreateMap<SeriesDTO, CreateSeriesDTO>();
    }
}
