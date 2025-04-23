// <copyright file="BookDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs;

/// <summary>
/// The book dto.
/// </summary>
public class BookDTO
{
    /// <summary>
    /// Gets or sets the book id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Gets or sets the release date.
    /// </summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// Gets or sets the page number.
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the series.
    /// </summary>
    public SeriesDTO? Series { get; set; }

    /// <summary>
    /// Gets or sets the series number.
    /// </summary>
    public int? SeriesNumber { get; set; } = null;

    /// <summary>
    /// Gets or sets the author.
    /// </summary>
    public AuthorDTO? Author { get; set; }

    /// <summary>
    /// Gets or sets the categories.
    /// </summary>
    public IList<CategoryDTO> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    public IList<TagDTO> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the cover.
    /// </summary>
    public FileInfoDTO? Cover { get; set; }

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public LocationDTO? Location { get; set; }
}
