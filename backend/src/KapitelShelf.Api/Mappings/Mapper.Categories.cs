﻿// <copyright file="Mapper.Categories.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Data.Models;
using Riok.Mapperly.Abstractions;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The categories mapper.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a category model to a category dto.
    /// </summary>
    /// <param name="model">The category model.</param>
    /// <returns>The category dto.</returns>
    [MapperIgnoreSource(nameof(CategoryModel.Books))]
    public partial CategoryDTO CategoryModelToCategoryDto(CategoryModel model);

    /// <summary>
    /// Map a create category dto to a category model.
    /// </summary>
    /// <param name="dto">The create category dto.</param>
    /// <returns>The category model.</returns>
    [MapperIgnoreTarget(nameof(CategoryModel.Id))]
    [MapperIgnoreTarget(nameof(CategoryModel.Books))]
    public partial CategoryModel CreateCategoryDtoToCategoryModel(CreateCategoryDTO dto);

    /// <summary>
    /// Map a category dto to a create category dto.
    /// </summary>
    /// <param name="dto">The category dto.</param>
    /// <returns>The create category dto.</returns>
    [MapperIgnoreSource(nameof(CategoryDTO.Id))]
    public partial CreateCategoryDTO CategoryDtoToCreateCategoryDto(CategoryDTO dto);
}
