// <copyright file="ReadingBooksModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
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

    /// <summary>
    /// Gets or sets the layout-independent content anchor.
    /// </summary>
    [MaxLength(96)]
    public string? ContentAnchor { get; set; }

    /// <summary>
    /// Gets or sets the current progress revision, or null if no bookmark was saved.
    /// </summary>
    public Guid? ProgressRevision { get; set; }

    /// <summary>
    /// Gets or sets the last accepted guarded write id.
    /// </summary>
    public Guid? LastWriteId { get; set; }
}
