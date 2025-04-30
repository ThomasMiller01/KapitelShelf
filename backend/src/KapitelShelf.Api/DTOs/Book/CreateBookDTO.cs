// <copyright file="CreateBookDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Tag;

namespace KapitelShelf.Api.DTOs.Book;

/// <summary>
/// The create dto for a book.
/// </summary>
public class CreateBookDTO
{
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
    public CreateSeriesDTO? Series { get; set; }

    /// <summary>
    /// Gets or sets the series number.
    /// </summary>
    public int? SeriesNumber { get; set; }

    /// <summary>
    /// Gets or sets the author.
    /// </summary>
    public CreateAuthorDTO? Author { get; set; }

    /// <summary>
    /// Gets or sets the categories.
    /// </summary>
    public IList<CreateCategoryDTO> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    public IList<CreateTagDTO> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the cover.
    /// </summary>
    public CreateFileInfoDTO? Cover { get; set; }

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public CreateLocationDTO? Location { get; set; }
}
