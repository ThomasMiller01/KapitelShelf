// <copyright file="VisitedBooksModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The visited books model.
/// </summary>
public class VisitedBooksModel
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
    /// Gets or sets the visited date.
    /// </summary>
    public DateTime? VisitedAt { get; set; }
}
