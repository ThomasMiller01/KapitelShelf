// <copyright file="AuthorMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The author mapping profile.
/// </summary>
public class AuthorMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorMappingProfile"/> class.
    /// </summary>
    public AuthorMappingProfile()
    {
        CreateMap<AuthorModel, AuthorDTO>()
            .ReverseMap();

        CreateMap<CreateAuthorDTO, AuthorModel>();

        CreateMap<AuthorDTO, CreateAuthorDTO>();
    }
}
