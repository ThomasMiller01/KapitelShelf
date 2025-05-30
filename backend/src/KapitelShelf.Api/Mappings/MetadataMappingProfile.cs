// <copyright file="MetadataMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.MetadataScraper;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The metadata mapping profile.
/// </summary>
public class MetadataMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataMappingProfile"/> class.
    /// </summary>
    public MetadataMappingProfile()
    {
        CreateMap<MetadataDTO, MetadataScraperDTO>()
            .ForMember(dest => dest.TitleMatchScore, opt => opt.Ignore())
            .ForMember(dest => dest.CompletenessScore, opt => opt.Ignore());
    }
}
