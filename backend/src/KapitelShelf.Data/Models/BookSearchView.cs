// <copyright file="BookSearchView.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using NpgsqlTypes;

namespace KapitelShelf.Data.Models;

/// <summary>
/// The book model.
/// </summary>
public class BookSearchView
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
    /// Gets or sets the series name.
    /// </summary>
    public string SeriesName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the author name.
    /// </summary>
    public string AuthorNames { get; set; } = null!;

    /// <summary>
    /// Gets or sets the category names.
    /// </summary>
    public string CategoryNames { get; set; } = null!;

    /// <summary>
    /// Gets or sets the tag names.
    /// </summary>
    public string TagNames { get; set; } = null!;

    /// <summary>
    /// Gets or sets the search vector.
    /// </summary>
    public NpgsqlTsVector SearchVector { get; set; } = null!;
}
