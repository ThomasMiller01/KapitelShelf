// <copyright file="ReadingBooksModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Data.Models.User;

namespace KapitelShelf.Data.Models;

/// <summary>
/// The visited books model.
/// </summary>
public class ReadingBooksModel
{
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the book id.
    /// </summary>
    public Guid BookId { get; set; }

    /// <summary>
    /// Gets or sets the book.
    /// </summary>
    public virtual BookModel Book { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    public virtual UserModel User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the last reading date.
    /// </summary>
    public DateTime? LastReadAt { get; set; }

    /// <summary>
    /// Gets or sets the current reading section.
    /// </summary>
    public int CurrentSection { get; set; } = 0;

    /// <summary>
    /// Gets or sets the current reading page.
    /// </summary>
    public int CurrentPage { get; set; } = 0;
}
