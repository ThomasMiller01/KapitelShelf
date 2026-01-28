// <copyright file="AuthorSortByDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Author;

/// <summary>
/// The sort by enum for authors.
/// </summary>
public enum AuthorSortByDTO
{
    /// <summary>
    /// Default sorting order for authors.
    /// </summary>
    Default,

    /// <summary>
    /// Sort by the author first name.
    /// </summary>
    FirstName,

    /// <summary>
    /// Sort by the author last name.
    /// </summary>
    LastName,

    /// <summary>
    /// Sort by the author total books.
    /// </summary>
    TotalBooks,
}
