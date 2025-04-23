// <copyright file="CategoryDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs;

/// <summary>
/// The category dto.
/// </summary>
public class CategoryDTO
{
    /// <summary>
    /// Gets or sets the category id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = null!;
}
