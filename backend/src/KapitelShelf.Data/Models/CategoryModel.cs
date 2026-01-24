// <copyright file="CategoryModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The category model.
/// </summary>
public class CategoryModel
{
    /// <summary>
    /// Gets or sets the category id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = null!;

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
    public ICollection<BookCategoryModel> Books { get; set; } = [];
}
