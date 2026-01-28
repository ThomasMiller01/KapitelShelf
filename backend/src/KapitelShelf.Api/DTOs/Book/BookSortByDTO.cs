// <copyright file="BookSortByDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Book;

/// <summary>
/// The sort by enum for books.
/// </summary>
public enum BookSortByDTO
{
    /// <summary>
    /// Default sorting order for books.
    /// </summary>
    Default,

    /// <summary>
    /// Sort by the book title.
    /// </summary>
    Title,

    /// <summary>
    /// Sort by the book author.
    /// </summary>
    Author,

    /// <summary>
    /// Sort by the book series name.
    /// </summary>
    Series,

    /// <summary>
    /// Sort by the book volume.
    /// </summary>
    Volume,

    /// <summary>
    /// Sort by the book pages.
    /// </summary>
    Pages,

    /// <summary>
    /// Sort by the book release date.
    /// </summary>
    Release,
}
