// <copyright file="CategorySortByDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Category;

/// <summary>
/// The sort by enum for categories.
/// </summary>
public enum CategorySortByDTO
{
    /// <summary>
    /// Default sorting order for categories.
    /// </summary>
    Default,

    /// <summary>
    /// Sort by the category name.
    /// </summary>
    Name,

    /// <summary>
    /// Sort by the category total books.
    /// </summary>
    TotalBooks,
}
