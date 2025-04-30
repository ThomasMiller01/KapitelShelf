// <copyright file="CategoryMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The category mapping profile.
/// </summary>
public class CategoryMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryMappingProfile"/> class.
    /// </summary>
    public CategoryMappingProfile()
    {
        CreateMap<CategoryModel, CategoryDTO>();

        CreateMap<CreateCategoryDTO, CategoryModel>();
    }
}
