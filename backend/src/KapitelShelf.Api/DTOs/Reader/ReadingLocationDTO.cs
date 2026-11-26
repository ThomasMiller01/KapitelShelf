// <copyright file="ReadingLocationDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Reader;

/// <summary>
/// The reading location dto.
/// </summary>
public class ReadingLocationDTO
{
    /// <summary>
    /// Gets or sets the current section.
    /// </summary>
    public int CurrentSection { get; set; }

    /// <summary>
    /// Gets or sets the current page.
    /// </summary>
    public int CurrentPage { get; set; }
}
