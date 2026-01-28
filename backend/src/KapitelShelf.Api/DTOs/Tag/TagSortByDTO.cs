// <copyright file="TagSortByDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Tag;

/// <summary>
/// The sort by enum for tags.
/// </summary>
public enum TagSortByDTO
{
    /// <summary>
    /// Default sorting order for tags.
    /// </summary>
    Default,

    /// <summary>
    /// Sort by the tag name.
    /// </summary>
    Name,

    /// <summary>
    /// Sort by the tag total books.
    /// </summary>
    TotalBooks,
}
