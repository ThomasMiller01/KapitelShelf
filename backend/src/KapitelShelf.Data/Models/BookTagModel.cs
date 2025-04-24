// <copyright file="BookTagModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The booktag model.
/// </summary>
public class BookTagModel
{
    /// <summary>
    /// Gets or sets the book id.
    /// </summary>
    public Guid BookId { get; set; }

    /// <summary>
    /// Gets or sets the book.
    /// </summary>
    public BookModel Book { get; set; } = null!;

    /// <summary>
    /// Gets or sets the tag id.
    /// </summary>
    public Guid TagId { get; set; }

    /// <summary>
    /// Gets or sets the tag.
    /// </summary>
    public TagModel Tag { get; set; } = null!;
}
