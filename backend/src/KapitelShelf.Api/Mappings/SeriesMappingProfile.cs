// <copyright file="SeriesMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs;
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
    }
}
