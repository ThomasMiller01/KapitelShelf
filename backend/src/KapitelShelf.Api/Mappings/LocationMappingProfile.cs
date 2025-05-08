// <copyright file="LocationMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The location mapping profile.
/// </summary>
public class LocationMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocationMappingProfile"/> class.
    /// </summary>
    public LocationMappingProfile()
    {
        CreateMap<LocationModel, LocationDTO>()
            .ReverseMap();

        CreateMap<LocationType, LocationTypeDTO>()
            .ReverseMap();

        CreateMap<CreateLocationDTO, LocationModel>()
            .ForMember(dest => dest.FileInfo, opt => opt.Ignore());

        CreateMap<LocationDTO, CreateLocationDTO>();
    }
}
