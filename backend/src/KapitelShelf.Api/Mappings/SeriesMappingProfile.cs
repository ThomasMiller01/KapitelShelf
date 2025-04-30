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
            .ReverseMap();

        CreateMap<SeriesModel, SeriesSummaryDTO>()
            .ForMember(dest => dest.LastVolume, opt =>
                opt.MapFrom(src => src.Books
                    .OrderByDescending(b => b.SeriesNumber)
                    .FirstOrDefault()));

        CreateMap<CreateSeriesDTO, SeriesModel>();
    }
}
