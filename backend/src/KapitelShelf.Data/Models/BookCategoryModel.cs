// <copyright file="BookCategoryModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The bookcategory model.
/// </summary>
public class BookCategoryModel
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
    /// Gets or sets the category id.
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    public CategoryModel Category { get; set; } = null!;
}
