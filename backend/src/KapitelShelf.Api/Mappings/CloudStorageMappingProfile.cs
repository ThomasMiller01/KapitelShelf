// <copyright file="CloudStorageMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
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
    }
}
