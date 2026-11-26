// <copyright file="ReadingBookDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Reader;

/// <summary>
/// The book reading dto.
/// </summary>
public class ReadingBookDTO
{
    /// <summary>
    /// Gets or sets the book id.
    /// </summary>
    public Guid BookId { get; set; }

    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the current section number.
    /// </summary>
    public int CurrentSection { get; set; }

    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int CurrentPage { get; set; }
}
