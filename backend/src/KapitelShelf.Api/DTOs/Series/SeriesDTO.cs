// <copyright file="SeriesDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Book;

namespace KapitelShelf.Api.DTOs.Series;

/// <summary>
/// The series dto.
/// </summary>
public class SeriesDTO
{
    /// <summary>
    /// Gets or sets the series id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the creation time.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the update time.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last volume of this series.
    /// </summary>
    public BookDTO? LastVolume { get; set; } = null!;

    /// <summary>
    /// Gets or sets the total number of books.
    /// </summary>
    public int? TotalBooks { get; set; }
}
