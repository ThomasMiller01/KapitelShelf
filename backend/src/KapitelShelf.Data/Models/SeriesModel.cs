// <copyright file="SeriesModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace KapitelShelf.Data.Models;

/// <summary>
/// The series model.
/// </summary>
public class SeriesModel
{
    private int? rating;

    /// <summary>
    /// Gets or sets the series id.
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
    /// Gets or sets the books.
    /// </summary>
    public virtual ICollection<BookModel> Books { get; set; } = [];
}
