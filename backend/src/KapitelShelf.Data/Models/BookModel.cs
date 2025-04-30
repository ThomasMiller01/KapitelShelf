// <copyright file="BookModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The book model.
/// </summary>
public class BookModel
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
    /// Gets or sets the series id.
    /// </summary>
    public Guid SeriesId { get; set; }

    /// <summary>
    /// Gets or sets the series.
    /// </summary>
    public SeriesModel? Series { get; set; }

    /// <summary>
    /// Gets or sets the series number.
    /// </summary>
    public int SeriesNumber { get; set; } = 0;

    /// <summary>
    /// Gets or sets the author id.
    /// </summary>
    public Guid? AuthorId { get; set; }

    /// <summary>
    /// Gets or sets the author.
    /// </summary>
    public AuthorModel? Author { get; set; }

    /// <summary>
    /// Gets or sets the cover.
    /// </summary>
    public FileInfoModel? Cover { get; set; }

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public LocationModel? Location { get; set; }

    /// <summary>
    /// Gets or sets the categories.
    /// </summary>
    public ICollection<BookCategoryModel> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    public ICollection<BookTagModel> Tags { get; set; } = [];
}
