// <copyright file="TagModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The tag model.
/// </summary>
public class TagModel
{
    /// <summary>
    /// Gets or sets the tag id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the books.
    /// </summary>
    public ICollection<BookTagModel> Books { get; set; } = [];
}
