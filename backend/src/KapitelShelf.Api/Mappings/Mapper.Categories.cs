// <copyright file="Mapper.Categories.cs" company="KapitelShelf">
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
    /// Map an category model to an category dto, applying additional manual field logic.
    /// </summary>
    /// <param name="model">The category model.</param>
    /// <returns>The mapped category dto.</returns>
    [UserMapping(Default = true)]
    public CategoryDTO CategoryModelToCategoryDto(CategoryModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var dto = this.CategoryModelToCategoryDtoCore(model);

        dto.TotalBooks = model.Books.Count;

        return dto;
    }

    /// <summary>
    /// Map a category model to a category dto.
    /// </summary>
    /// <param name="model">The category model.</param>
    /// <returns>The category dto.</returns>
    [MapperIgnoreSource(nameof(CategoryModel.Books))]
    [MapperIgnoreSource(nameof(CategoryModel.CreatedAt))]
    [MapperIgnoreSource(nameof(CategoryModel.UpdatedAt))]
    [MapperIgnoreTarget(nameof(CategoryDTO.TotalBooks))]
    public partial CategoryDTO CategoryModelToCategoryDtoCore(CategoryModel model);

    /// <summary>
    /// Map a create category dto to a category model.
    /// </summary>
    /// <param name="dto">The create category dto.</param>
    /// <returns>The category model.</returns>
    [MapperIgnoreTarget(nameof(CategoryModel.Id))]
    [MapperIgnoreTarget(nameof(CategoryModel.Books))]
    [MapperIgnoreTarget(nameof(CategoryModel.CreatedAt))]
    [MapperIgnoreTarget(nameof(CategoryModel.UpdatedAt))]
    public partial CategoryModel CreateCategoryDtoToCategoryModel(CreateCategoryDTO dto);

    /// <summary>
    /// Map a category dto to a create category dto.
    /// </summary>
    /// <param name="dto">The category dto.</param>
    /// <returns>The create category dto.</returns>
    [MapperIgnoreSource(nameof(CategoryDTO.Id))]
    [MapperIgnoreSource(nameof(CategoryDTO.TotalBooks))]
    public partial CreateCategoryDTO CategoryDtoToCreateCategoryDto(CategoryDTO dto);
}
