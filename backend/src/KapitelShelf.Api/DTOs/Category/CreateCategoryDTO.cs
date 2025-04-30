// <copyright file="CreateCategoryDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Category;

/// <summary>
/// The create dto for a category.
/// </summary>
public class CreateCategoryDTO
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = null!;
}
