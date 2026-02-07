// <copyright file="UserBookMetadata.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace KapitelShelf.Data.Models.User;

/// <summary>
/// The user book metadata model.
/// </summary>
public class UserBookMetadataModel
{
    private int? rating;

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
    /// Gets or sets the user id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    public UserModel User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the rating.
    /// </summary>
    [Range(1, 10)]
    public int? Rating
    {
        get => rating;
        set
        {
            if (value is < 1 or > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(Rating), "Rating must be between 1 and 10");
            }

            rating = value;
        }
    }

    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    public string? Notes { get; set; }
}
