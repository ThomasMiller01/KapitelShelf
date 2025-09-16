// <copyright file="UserBookMetadata.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models.User;

/// <summary>
/// The user book metadata model.
/// </summary>
public class UserBookMetadataModel
{
    /// <summary>
    /// Gets or sets the metadata id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the book id.
    /// </summary>
    public Guid BookId { get; set; }

    /// <summary>
    /// Gets or sets the book.
    /// </summary>
    public BookModel Book { get; set; } = null!;

    /// <summary>
    /// Gets or sets the rating.
    /// </summary>
    public int? Rating { get; set; }

    /// <summary>
    /// Gets or sets the favourite.
    /// </summary>
    public bool? Favourite { get; set; }

    /// <summary>
    /// Gets or sets the last read date.
    /// </summary>
    public DateTime? LastReadDate { get; set; }

    /// <summary>
    /// Gets or sets the comment.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    public UserModel User { get; set; } = null!;
}
