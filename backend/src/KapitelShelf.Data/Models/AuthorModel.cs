// <copyright file="AuthorModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The author model.
/// </summary>
public class AuthorModel
{
    /// <summary>
    /// Gets or sets the author id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the creation time.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the update time.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the books.
    /// </summary>
    public ICollection<BookModel> Books { get; set; } = [];
}
