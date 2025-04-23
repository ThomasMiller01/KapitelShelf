// <copyright file="SeriesModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The series model.
/// </summary>
public class SeriesModel
{
    /// <summary>
    /// Gets or sets the series id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the books.
    /// </summary>
    public ICollection<BookModel> Books { get; set; } = [];
}
