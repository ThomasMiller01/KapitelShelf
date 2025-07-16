// <copyright file="CloudStorageMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.CloudStorage.RClone;
using KapitelShelf.Data.Models.CloudStorage;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The cloud storage mapping profile.
/// </summary>
public class CloudStorageMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloudStorageMappingProfile"/> class.
    /// </summary>
    public CloudStorageMappingProfile()
    {
        CreateMap<CloudConfigurationModel, CloudConfigurationDTO>()
            .ReverseMap();

        CreateMap<CloudType, CloudTypeDTO>()
            .ReverseMap();

        CreateMap<CloudStorageModel, CloudStorageDTO>()
            .ReverseMap()
            .ForMember(dest => dest.RCloneConfig, opt => opt.Ignore());

        CreateMap<RCloneListJsonDTO, CloudStorageDirectoryDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
            .ForMember(dest => dest.ModifiedTime, opt => opt.MapFrom(src => DateTime.Parse(src.ModTime, CultureInfo.InvariantCulture)))
        .ReverseMap();
    }
}
