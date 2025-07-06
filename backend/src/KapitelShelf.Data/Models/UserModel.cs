// <copyright file="UserModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The user model.
/// </summary>
public class UserModel
{
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Gets or sets the profile image.
    /// </summary>
    public ProfileImageType Image { get; set; } = ProfileImageType.Readini;

    /// <summary>
    /// Gets or sets the profile color.
    /// </summary>
    public string Color { get; set; } = "fffff";

    /// <summary>
    /// Gets or sets the books.
    /// </summary>
    public ICollection<UserBookMetadataModel> Books { get; set; } = [];

    /// <summary>
    /// Gets or sets the visited books.
    /// </summary>
    public ICollection<VisitedBooksModel> VisitedBooks { get; set; } = [];
}
