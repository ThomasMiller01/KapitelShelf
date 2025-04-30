// <copyright file="TagMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The tag mapping profile.
/// </summary>
public class TagMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TagMappingProfile"/> class.
    /// </summary>
    public TagMappingProfile()
    {
        CreateMap<TagModel, TagDTO>();

        CreateMap<CreateTagDTO, TagModel>();
    }
}
